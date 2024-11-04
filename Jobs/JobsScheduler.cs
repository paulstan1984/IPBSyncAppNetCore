using Hangfire;

namespace IPBSyncAppNetCore.Jobs
{
    public class JobsScheduler
    {
        public static void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate<SyncImagesJob>("sync-images", job => job.Execute(), "*/15 * * * *");
            RecurringJob.AddOrUpdate<SyncDescriptionsJobs>("sync-descriptions", job => job.Execute(), "*/15 * * * *");

            RecurringJob.AddOrUpdate<DownloadOrdersJob>("download-orders", job => job.Execute(), "*/5 * * * *");
            RecurringJob.AddOrUpdate<SyncStocksJob>("sync-stocks", job => job.Execute(), "0,30 * * * *");
            
            RecurringJob.AddOrUpdate<SyncCategoriesJob>("sync-categories", job => job.Execute(), Cron.Daily(0));
            RecurringJob.AddOrUpdate<SyncArticlesJob>("sync-articles", job => job.Execute(), Cron.Daily(2));
        }

        public static void TestScheduledJob(JobBase job)
        {
            ClearJob();
            job.Execute();
        }

        private static void ClearJob()
        {
            RecurringJob.RemoveIfExists("sync-images");
            RecurringJob.RemoveIfExists("sync-descriptions");

            RecurringJob.RemoveIfExists("download-orders");
            RecurringJob.RemoveIfExists("sync-stocks");

            RecurringJob.RemoveIfExists("sync-categories");
            RecurringJob.RemoveIfExists("sync-articles");


        }
    }
}
