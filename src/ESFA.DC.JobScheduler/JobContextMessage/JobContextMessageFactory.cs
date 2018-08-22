﻿using System;
using System.Collections.Generic;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Settings;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.JobScheduler.JobContextMessage
{
    public sealed class JobContextMessageFactory
    {
        private readonly IlrFirstStageMessageTopics _ilrFirstStageMessageTopics;
        private readonly IlrSecondStageMessageTopics _ilrSecondStageMessageTopics;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger _logger;

        public JobContextMessageFactory(
            IlrFirstStageMessageTopics ilrFirstStageMessageTopics,
            IlrSecondStageMessageTopics ilrSecondStageMessageTopics,
            IKeyGenerator keyGenerator,
            ILogger logger)
        {
            _ilrFirstStageMessageTopics = ilrFirstStageMessageTopics;
            _ilrSecondStageMessageTopics = ilrSecondStageMessageTopics;
            _keyGenerator = keyGenerator;
            _logger = logger;
        }

        public JobContext.JobContextMessage CreateJobContextMessage(IJob job)
        {
            switch (job.JobType)
            {
                case JobType.IlrSubmission:
                    return CreateIlrJobContextMessage((IlrJob)job);
                default:
                    throw new NotImplementedException();
            }
        }

        public JobContext.JobContextMessage CreateIlrJobContextMessage(IlrJob ilrJob)
        {
            var topics = CreateIlrTopicsList(ilrJob.IsFirstStage);

            var message = new JobContext.JobContextMessage(
                ilrJob.JobId,
                topics,
                ilrJob.Ukprn.ToString(),
                ilrJob.StorageReference,
                ilrJob.FileName,
                ilrJob.SubmittedBy,
                0,
                ilrJob.DateTimeSubmittedUtc);

            AddExtraKeys(message, ilrJob);

            return message;
        }

        public void AddExtraKeys(JobContext.JobContextMessage message, IlrJob job)
        {
            message.KeyValuePairs.Add(JobContextMessageKey.FileSizeInBytes, job.FileSize);

            if (job.IsFirstStage)
            {
                message.KeyValuePairs.Add(JobContextMessageKey.PauseWhenFinished, "1");
            }

            if (!job.Ukprn.HasValue)
            {
                _logger.LogWarning("Can't get UKPRN, so unable to populate ILR keys");
                return;
            }

            message.KeyValuePairs.Add(JobContextMessageKey.InvalidLearnRefNumbers, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.ValidationInvalidLearners));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidLearnRefNumbers, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.ValidationValidLearners));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrors, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.ValidationErrors));
            message.KeyValuePairs.Add(JobContextMessageKey.ValidationErrorLookups, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.ValidationErrorsLookup));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingAlbOutput, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.FundingAlbOutput));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm35Output, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.FundingFm35Output));
            message.KeyValuePairs.Add(JobContextMessageKey.FundingFm25Output, _keyGenerator.GenerateKey(job.Ukprn.Value, job.JobId, TaskKeys.FundingFm25Output));
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
