using NLog;
using System.Net.Http.Headers;
using System.Text.Json;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncDescriptionsJobs : JobBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SyncDescriptionsJobs(IHttpClientFactory httpClientFactory) 
            : base(httpClientFactory)
        {
        }

        public override async Task RunJob()
        {
            try
            {
                Logger.Info("Start sync descriptions job");

                string[] txtFiles = LoadTextFilesFromFolder(ConfigService.DescriptionsPathDir);

                if (txtFiles == null || txtFiles.Length == 0)
                {
                    Logger.Error("No new descriptions files that have to be uploaded");
                    return;
                }

                foreach (var descriptionTextFile in txtFiles)
                {
                    Logger.Info($"set description for ean {descriptionTextFile}...");

                    //set the description for the product
                    bool updated = await SetDescriptionForEan(descriptionTextFile);

                    if (!updated)
                    {
                        Logger.Error($"Desctiption for ean {descriptionTextFile} couldn't be updated.");
                    }

                    ChangeFileDirectory(Logger, descriptionTextFile, ConfigService.UploadedDescriptionsPathDir);
                }

                Logger.Info("End sync images job");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when syncing descriptions job");
                Logger.Error(ex);
            }
        }


        private string[] LoadTextFilesFromFolder(string folderPath)
            => Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        .Take(ConfigService.BatchSize)
                        .ToArray();


        private async Task<bool> SetDescriptionForEan(string descriptionTextFile)
        {
            using var client = GetWebAPIHttpClient();
            string fileContent = File.ReadAllText(descriptionTextFile);
            string ean = Path.GetFileNameWithoutExtension(descriptionTextFile);
            string[] contentParts = fileContent.Split(ConfigService.DescriptionSeparator);
            string description = contentParts[0];
            string keywords = contentParts.Length == 1 
                ? string.Empty 
                : string.Join(',', contentParts[1].Split(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s)));
            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var content = JsonContent.Create(new { description, keywords }, new MediaTypeWithQualityHeaderValue("application/json"), options);
                HttpResponseMessage response = await client.PutAsync($"set-description/{ean}", content);
                var strResponse = await response.Content.ReadAsStringAsync();
                Logger.Debug("Response from OpenCart");
                Logger.Debug(strResponse);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error setting description for EAN: {ean}");
                Logger.Error(ex);
                return false;
            }
        }
    }
}
