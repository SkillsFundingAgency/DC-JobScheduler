using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Base;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobScheduler.Settings;

namespace ESFA.DC.JobScheduler.JobContextMessage
{
    public sealed class JobContextMessageFactory
    {
        private readonly IlrFirstStageMessageTopics _ilrFirstStageMessageTopics;
        private readonly IlrSecondStageMessageTopics _ilrSecondStageMessageTopics;

        public JobContextMessageFactory(
            IlrFirstStageMessageTopics ilrFirstStageMessageTopics,
            IlrSecondStageMessageTopics ilrSecondStageMessageTopics)
        {
            _ilrFirstStageMessageTopics = ilrFirstStageMessageTopics;
            _ilrSecondStageMessageTopics = ilrSecondStageMessageTopics;
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
                topics.Add(new TopicItem(_ilrFirstStageMessageTopics.TopicDeds_TaskGenerateValidationReport, _ilrFirstStageMessageTopics.TopicDeds_TaskGenerateValidationReport, tasks));
            }
            else
            {
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicValidation, _ilrSecondStageMessageTopics.TopicValidation, tasks));
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicDeds_TaskGenerateValidationReport, _ilrSecondStageMessageTopics.TopicDeds_TaskGenerateValidationReport, tasks));
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicFunding, _ilrSecondStageMessageTopics.TopicFunding, tasks));
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicDeds, _ilrSecondStageMessageTopics.TopicDeds, tasks));
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicReports, _ilrSecondStageMessageTopics.TopicReports, tasks));
                topics.Add(new TopicItem(_ilrSecondStageMessageTopics.TopicDeds_TaskPersistDataToDeds, _ilrSecondStageMessageTopics.TopicDeds_TaskPersistDataToDeds, tasks));
            }

            return topics;
        }
    }
}
