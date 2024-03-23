using Hangfire;

namespace IPBSyncAppNetCore.Jobs
{
    public class JobsScheduler
    {
        public static void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate("sync-images", () => new SyncImagesJob().Execute(), Cron.MinuteInterval(5));
            RecurringJob.AddOrUpdate("sync-descriptions", () => new SyncDescriptionsJobs().Execute(), Cron.MinuteInterval(5));

            RecurringJob.AddOrUpdate("sync-articles", () => new SyncArticlesJob().Execute(), Cron.Hourly(30));
            RecurringJob.AddOrUpdate("sync-stocks", () => new SyncStocksJob().Execute(), Cron.MinuteInterval(30));
            RecurringJob.AddOrUpdate("download-orders", () => new DownloadOrdersJob().Execute(), Cron.MinuteInterval(30));

            RecurringJob.AddOrUpdate("sync-articles-full", () => new SyncArticlesFullJobs().Execute(), Cron.Daily);
            RecurringJob.AddOrUpdate("sync-categories", () => new SyncCategoriesJob().Execute(), Cron.Daily);
        }
    }
}
