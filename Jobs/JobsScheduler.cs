using Hangfire;

namespace IPBSyncAppNetCore.Jobs
{
    public class JobsScheduler
    {
        public static void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate<SyncImagesJob>("sync-images", job => job.Execute(), "*/15 * * * *");
            RecurringJob.AddOrUpdate<SyncDescriptionsJobs>("sync-descriptions", job => job.Execute(), "*/15 * * * *");

            RecurringJob.AddOrUpdate<DownloadOrdersJob>("download-orders", job => job.Execute(), "10,40 * * * *");
            RecurringJob.AddOrUpdate<SyncStocksJob>("sync-stocks", job => job.Execute(), "0,30 * * * *");
            
            RecurringJob.AddOrUpdate<SyncCategoriesJob>("sync-categories", job => job.Execute(), Cron.Daily(0));
            RecurringJob.AddOrUpdate<SyncArticlesJob>("sync-articles", job => job.Execute(), Cron.Daily(2));
        }
    }
}
