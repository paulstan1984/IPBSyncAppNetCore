using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using NLog;
using System.Text.Json;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncStocksJob : JobBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public override async Task RunJob()
        {
            try
            {
                Logger.Info("Start Sync Stocks");

                Logger.Info("Call Truncate Table");
                await OCTruncateTable();
                Logger.Info("WME_Stocks table truncated");

                Logger.Info("Get stocks from WME using REST API");
                JArray? WMEStocks = ConfigService.IsDebug
                    ? await LoadStocks() // load stocks from a local file
                    : await GetWMEStocks(); // load stocks from WME Rest API

                if (WMEStocks == null)
                {
                    Logger.Error("Cannot receive stocks from WME");
                    return;
                }
                Logger.Info("Stocks received from WME");

                Logger.Info("Send stocks to OC/API developed using laravel");

                int currentIndex = 0;

                while (currentIndex < WMEStocks.Count)
                {
                    WmeStock[] stocks = WMEStocks
                        .Skip(currentIndex)
                        .Take(ConfigService.BatchSize)
                        .Select(x => x.ToObject<WmeStock>())
                        .ToArray();

                    currentIndex += ConfigService.BatchSize;

                    Logger.Info($"Sending {stocks.Length} records to oc API...");
                    string response = await OCSyncStocks(stocks);
                    Logger.Info($"Response from OC API:");
                    Logger.Info(response);
                }

                Logger.Info("Call Transfer Data");
                await OCTransferStocks();

                Logger.Info("End Sync Stocks");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when sync-stocks");
                Logger.Error(ex);
            }
        }

        #region internal operations
        private async Task<JArray?> GetWMEStocks()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(ConfigService.WMERESTAPIURL);

            try
            {
                // Call the API asynchronously
                HttpResponseMessage response = await client.GetAsync("GetStocArticoleExt");

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JObject data = (JToken.ReadFrom(reader) as JArray).Last() as JObject;

                        return data["Data"] as JArray;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when receiving stocks from WME");
                Logger.Error(ex);
            }

            return new JArray();
        }

        private async Task<JArray?> LoadStocks()
        {
            //C:\laragon\www\ipb\GetInfoArticole.json
            using (var responseStream = new FileStream(@"C:\laragon\www\ipb\GetStocArticoleExt.json", FileMode.Open))
            using (var reader = new JsonTextReader(new StreamReader(responseStream)))
            {
                JObject data = (JToken.ReadFrom(reader) as JArray).Last() as JObject;

                return data["Data"] as JArray;
            }
        }

        private async Task OCTruncateTable()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", ConfigService.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.DeleteAsync("truncate-stocks");
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

        private async Task OCTransferStocks()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", ConfigService.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.PostAsync("transfer-stocks", null);
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
                Logger.Error("An error appeared when calling transfer-stocks endpoint on OpenCart");
                Logger.Error(ex);
            }
        }

        private async Task<string> OCSyncStocks(WmeStock[] stocks)
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(ConfigService.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", ConfigService.WebAuthorizationToken);

            try
            {
                // Configure JSON serializer options to use PascalCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };

                var content = JsonContent.Create(stocks, new MediaTypeWithQualityHeaderValue("application/json"), options);

                // Call the API asynchronously
                var strContent = await content.ReadAsStringAsync();
                Logger.Debug("Sending stocks to OpenCart");
                Logger.Debug(strContent);
                HttpResponseMessage response = await client.PostAsync("sync-stocks", content);
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
                    return "Stocks have not been inserted.";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when pushing stocks to OpenCart");
                Logger.Error(ex);
                return ex.Message;
            }
        }

        #endregion
    }
}
