using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using NLog;
using System.Text.Json;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncCategoriesJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Execute() => RunJob().Wait();

        private async Task RunJob()
        {
            try
            {
                Logger.Info("Start Sync Categories");

                Logger.Info("Call Truncate Table");
                await OCTruncateTable();
                Logger.Info("WME_Categories table truncated");

                Logger.Info("Get categories from WME using REST API");
                JArray? WMECategories = Config.IsDebug
                    ? await LoadCategories()  // load categories from a local file
                    : await GetWMECategories(); // load categories from WME Rest API

                if (WMECategories == null)
                {
                    Logger.Error("Cannot receive categories from WME");
                    return;
                }
                Logger.Info("Categories received from WME");

                Logger.Info("Send categories to OC/API developed using laravel");

                int currentIndex = 0;

                while (currentIndex < WMECategories.Count)
                {
                    WMECategory[] categories = WMECategories
                        .Skip(currentIndex)
                        .Take(Config.BatchSize)
                        .Select(x => x.ToObject<WMECategory>())
                        .ToArray();

                    currentIndex += Config.BatchSize;

                    Logger.Info($"Sending {categories.Length} categories to oc API...");
                    string response = await OCSyncCategories(categories);
                    Logger.Info($"Response from OC API:");
                    Logger.Info(response);
                }

                Logger.Info("Call Transfer Data");
                await OCCallTransferCategories();

                Logger.Info("End Sync Categories");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when sync-categories");
                Logger.Error(ex);
            }
        }

        #region internal operations
        private async Task<JArray?> GetWMECategories()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WMERESTAPIURL);

            try
            {
                // Call the API asynchronously
                HttpResponseMessage response = await client.GetAsync("GetClaseArticole");

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JArray data = (JArray)JToken.ReadFrom(reader);

                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when receiving categories from WME");
                Logger.Error(ex);
            }

            return new JArray();
        }

        private async Task<JArray?> LoadCategories()
        {
            //C:\laragon\www\ipb\GetInfoArticole.json
            using (var responseStream = new FileStream(@"C:\laragon\www\ipb\GetInfoClase.json", FileMode.Open))
            using (var reader = new JsonTextReader(new StreamReader(responseStream)))
            {
                JArray data = (JArray)JToken.ReadFrom(reader);

                return data;
            }
        }

        private async Task OCTruncateTable()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", Config.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.DeleteAsync("truncate-categories");
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
                Logger.Error("An error appeared when calling truncate endpoint on OpenCart");
                Logger.Error(ex);
            }
        }

        private async Task OCCallTransferCategories()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", Config.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.PostAsync("transfer-categories", null);
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
                Logger.Error("An error appeared when calling trabsfer categories endpoint on OpenCart");
                Logger.Error(ex);
            }
        }

        private async Task<string> OCSyncCategories(WMECategory[] categories)
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", Config.WebAuthorizationToken);

            try
            {
                // Configure JSON serializer options to use PascalCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };

                var content = JsonContent.Create(categories, new MediaTypeWithQualityHeaderValue("application/json"), options);

                // Call the API asynchronously
                var strContent = await content.ReadAsStringAsync();
                Logger.Debug("Sending categories to OpenCart");
                Logger.Debug(strContent);
                HttpResponseMessage response = await client.PostAsync("sync-categories", content);
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
                        return data["message"].ToString();
                    }
                }
                else
                {
                    return "Categories have not been inserted.";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when pushing categories to OpenCart");
                Logger.Error(ex);
                return ex.Message;
            }
        }

        #endregion
    }
}
