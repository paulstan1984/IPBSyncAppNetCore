using Hangfire;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog;

namespace IPBSyncAppNetCore.Jobs
{
    public abstract class JobBase
    {
        public void Execute()
        {
            if (!IsRunning) RunJob().Wait();
        }

        public abstract Task RunJob();

        public bool IsRunning => JobStorage
                .Current
                .GetMonitoringApi()
                .ProcessingJobs(0, int.MaxValue)
                .Count(j => j.Value.Job.Type.Name == this.GetType().Name) > 1;

        protected void ChangeFileDirectory(Logger Logger, string sourceFilePath, string targetDirectory)
        {
            // Check if source file exists
            if (!File.Exists(sourceFilePath))
            {
                Logger.Error($"Source file does not exist: {sourceFilePath}.");
                return;
            }

            // Ensure the target directory exists
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Get the file name from the source file path
            string fileName = Path.GetFileName(sourceFilePath);

            // Combine the target directory and file name to create the new path
            string targetFilePath = Path.Combine(targetDirectory, fileName);

            // Move the file to the new directory
            File.Move(sourceFilePath, targetFilePath, true);
        }

        protected async Task OCCallRepairSeoUrls(Logger Logger)
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", ConfigService.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.PutAsync("seo-urls-repair", null);
                var strResponse = await response.Content.ReadAsStringAsync();
                Logger.Debug("Response from OpenCart");
                Logger.Debug(strResponse);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JObject data = (JObject)JToken.ReadFrom(reader);
                        Logger.Debug("Received response from OpenCart");
                        Logger.Debug(data.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when calling /seo-urls-repair endpoint on OpenCart");
                Logger.Error(ex);
            }
        }
    }
}
