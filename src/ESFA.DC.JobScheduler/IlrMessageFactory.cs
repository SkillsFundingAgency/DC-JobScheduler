﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac.Features.AttributeFilters;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Interfaces;
using ESFA.DC.JobScheduler.Interfaces.Models;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface.Configuration;

namespace ESFA.DC.JobScheduler
{
    public sealed class IlrMessageFactory : IMessageFactory
    {
        private readonly IlrFirstStageMessageTopics _ilrFirstStageMessageTopics;
        private readonly IlrSecondStageMessageTopics _ilrSecondStageMessageTopics;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger _logger;
        private readonly IFileUploadJobManager _fileUploadJobManager;
        private readonly ITopicConfiguration _topicConfiguration;

        public IlrMessageFactory(
            IlrFirstStageMessageTopics ilrFirstStageMessageTopics,
            IlrSecondStageMessageTopics ilrSecondStageMessageTopics,
            IKeyGenerator keyGenerator,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            [KeyFilter(JobType.IlrSubmission)]ITopicConfiguration topicConfiguration)
        {
            _ilrFirstStageMessageTopics = ilrFirstStageMessageTopics;
            _ilrSecondStageMessageTopics = ilrSecondStageMessageTopics;
            _keyGenerator = keyGenerator;
            _logger = logger;
            _fileUploadJobManager = fileUploadMetaDataManager;
            _topicConfiguration = topicConfiguration;
        }

        public MessageParameters CreateMessageParameters(long jobId, bool isCrossLoaded)
        {
            var job = _fileUploadJobManager.GetJobById(jobId);

            var topics = CreateIlrTopicsList(job.IsFirstStage);

            var contextMessage = new JobContextMessage(
                job.JobId,
                topics,
                job.Ukprn.ToString(),
                job.StorageReference,
                job.FileName,
                job.SubmittedBy,
                0,
                job.DateTimeSubmittedUtc);

            AddExtraKeys(contextMessage, job, isCrossLoaded);

            var message = new MessageParameters(JobType.IlrSubmission)
            {
                JobContextMessage = contextMessage,
                TopicParameters = new Dictionary<string, object>
                {
                    {
                        "To", _topicConfiguration.SubscriptionName
                    }
                },
                SubscriptionLabel = _topicConfiguration.SubscriptionName
            };

            return message;
        }

        public void AddExtraKeys(JobContextMessage message, FileUploadJob metaData, bool isCrossLoaded)
        {
            if (message.KeyValuePairs == null)
            {
                message.KeyValuePairs = new Dictionary<string, object>();
            }

            message.KeyValuePairs.Add(JobContextMessageKey.FileSizeInBytes, metaData.FileSize);

            if (metaData.IsFirstStage)
            {
                message.KeyValuePairs.Add(JobContextMessageKey.PauseWhenFinished, "1");
            }

            if (metaData.Ukprn == 0)
            {
                _logger.LogWarning("Can't get UKPRN, so unable to populate ILR keys");
                return;
            }

            if (isCrossLoaded)
            {
                message.KeyValuePairs.Add(JobContextMessageKey.JobIsCrossLoaded, "1");
            }

            message.KeyValuePairs.Add(JobContextMessageKey.InvalidLearnRefNumbers, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationInvalidLearners));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidLearnRefNumbers, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationValidLearners));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrors, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationErrors));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrorLookups, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.ValidationErrorsLookup));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingAlbOutput, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.FundingAlbOutput));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm35Output, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.FundingFm35Output));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm25Output, _keyGenerator.GenerateKey(metaData.Ukprn, metaData.JobId, TaskKeys.FundingFm25Output));
        }

        public List<TopicItem> CreateIlrTopicsList(bool isFirstStage)
        {
            var topics = new List<TopicItem>();

            var tasks = new List<ITaskItem>()
            {
                new TaskItem()
                {
                    Tasks = new List<string>() { string.Empty },
                    SupportsParallelExecution = false
                }
            };

            if (isFirstStage)
            {
                topics.Add(new TopicItem(_ilrFirstStageMessageTopics.TopicValidation, _ilrFirstStageMessageTopics.TopicValidation, tasks));
                topics.Add(new TopicItem(
                    _ilrFirstStageMessageTopics.TopicReports,
                    _ilrFirstStageMessageTopics.TopicReports,
                    new List<ITaskItem>()
                    {
                        new TaskItem()
                        {
                            Tasks = new List<string>()
                            {
                                _ilrFirstStageMessageTopics.TopicReports_TaskGenerateValidationReport
                            },
                            SupportsParallelExecution = false
                        }
                    }));
            }
            else
            {
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicValidation, _ilrSecondStageMessageTopics.TopicValidation, tasks));

                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicFunding, _ilrSecondStageMessageTopics.TopicFunding, new List<ITaskItem>()
                {
                    new TaskItem()
                    {
                        Tasks = new List<string>()
                        {
                            _ilrSecondStageMessageTopics.TopicFunding_TaskPerformALBCalculation,
                            _ilrSecondStageMessageTopics.TopicFunding_TaskPerformFM25Calculation,
                            _ilrSecondStageMessageTopics.TopicFunding_TaskPerformFM35Calculation
                        },
                        SupportsParallelExecution = false
                    }
                }));

                topics.Add(new TopicItem(
                    _ilrSecondStageMessageTopics.TopicDeds,
                    _ilrSecondStageMessageTopics.TopicDeds,
                    new List<ITaskItem>()
                    {
                        new TaskItem()
                        {
                            Tasks = new List<string>()
                            {
                                _ilrSecondStageMessageTopics.TopicDeds_TaskPersistDataToDeds
                            },
                            SupportsParallelExecution = false
                        }
                    }));

                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicReports, _ilrSecondStageMessageTopics.TopicReports, new List<ITaskItem>()
                {
                    new TaskItem()
                    {
                        Tasks = new List<string>()
                        {
                            _ilrSecondStageMessageTopics.TopicReports_TaskGenerateValidationReport,
                            _ilrSecondStageMessageTopics.TopicReports_TaskGenerateAllbOccupancyReport,
                            _ilrSecondStageMessageTopics.TopicReports_TaskGenerateFundingSummaryReport,
                            _ilrSecondStageMessageTopics.TopicReports_TaskGenerateMainOccupancyReport
                        },
                        SupportsParallelExecution = false
                    }
                }));
            }

            return topics;
        }
    }
}
