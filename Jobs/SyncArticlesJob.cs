using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using NLog;
using System.Text.Json;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncArticlesJob : JobBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public override async Task RunJob()
        {
            try
            {
                Logger.Info("Start Sync Articles");

                Logger.Info("Call Truncate Table");
                await OCTruncateTable();
                Logger.Info("WME_Products table truncated");

                Logger.Info("Get articles from WME using REST API");
                JArray? WMEProducts = Config.IsDebug 
                    ? await LoadProducts() // load products from a local file
                    : await GetWMEProducts(); // load products from WME Rest API

                if (WMEProducts == null)
                {
                    Logger.Error("Cannot receive products from WME");
                    return;
                }
                Logger.Info("Articles received from WME");

                Logger.Info("Send articles to OC/API developed using laravel");
                
                int currentIndex = 0;

                while (currentIndex < WMEProducts.Count)
                {
                    WMEProduct[] products = WMEProducts
                        .Skip(currentIndex)
                        .Take(Config.BatchSize)
                        .Select(x => x.ToObject<WMEProduct>())
                        .ToArray();

                    currentIndex += Config.BatchSize;

                    Logger.Info($"Sending {products.Length} products to oc API...");
                    string response = await OCSyncArticles(products);
                    Logger.Info($"Response from OC API:");
                    Logger.Info(response);
                }

                Logger.Info("Call Transfer Data");
                await OCTransferProducts();

                Logger.Info("End Sync Articles");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when sync-articles");
                Logger.Error(ex);
            }
        }

        #region internal operations
        private async Task<JArray?> GetWMEProducts()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WMERESTAPIURL);

            try
            {
                // Call the API asynchronously
                HttpResponseMessage response = await client.GetAsync("GetInfoArticole");

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JObject data = (JObject)JToken.ReadFrom(reader);

                        return data["InfoArticole"] as JArray;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when receiving products from WME");
                Logger.Error(ex);
            }

            return new JArray();
        }

        private async Task<JArray?> LoadProducts()
        {
            //C:\laragon\www\ipb\GetInfoArticole.json
            using (var responseStream = new FileStream(@"C:\laragon\www\ipb\GetInfoArticole.json", FileMode.Open))
            using (var reader = new JsonTextReader(new StreamReader(responseStream)))
            {
                JObject data = (JObject)JToken.ReadFrom(reader);

                return data["InfoArticole"] as JArray;
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
                HttpResponseMessage response = await client.DeleteAsync("truncate-articles");
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

        private async Task OCTransferProducts()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", Config.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.PostAsync("transfer-articles", null);
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

        private async Task<string> OCSyncArticles(WMEProduct[] products)
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

                var content = JsonContent.Create(products, new MediaTypeWithQualityHeaderValue("application/json"), options);

                // Call the API asynchronously
                var strContent = await content.ReadAsStringAsync();
                Logger.Debug("Sending products to OpenCart");
                Logger.Debug(strContent);
                HttpResponseMessage response = await client.PostAsync("sync-articles", content);
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
                    return "Products have not been inserted.";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when pushing products to OpenCart");
                Logger.Error(ex);
                return ex.Message;
            }
        }
        #endregion
    }
}
