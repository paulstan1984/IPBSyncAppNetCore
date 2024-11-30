using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using NLog;
using System.Text.Json;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncOrdersJob : JobBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SyncOrdersJob(IHttpClientFactory httpClientFactory) 
            : base(httpClientFactory)
        {
        }

        public override async Task RunJob()
        {
            try
            {
                Logger.Info("Start Sync Orders");

                Logger.Info("Get orders from WME using REST API");
                JArray? WMEOrders = await GetWMEOrders();

                if (WMEOrders == null)
                {
                    Logger.Error("Cannot receive irders from WME");
                    return;
                }
                Logger.Info("Irders received from WME");

                Logger.Info("Send Orders statuses to OC/API developed using laravel");

                int currentIndex = 0;

                while (currentIndex < WMEOrders.Count)
                {
                    WmeExportedOrder[] orders = WMEOrders
                        .Skip(currentIndex)
                        .Take(ConfigService.BatchSize)
                        .Select(x => x.ToObject<WmeExportedOrder>())
                        .ToArray();

                    currentIndex += ConfigService.BatchSize;

                    if (orders.Length == 0)
                    {
                        continue;
                    }

                    Logger.Info($"Sending {orders.Length} orders statuses to oc API...");
                    string response = await OCSyncOrdersStatuses(orders);
                    Logger.Info($"Response from OC API:");
                    Logger.Info(response);
                }

                Logger.Info("End Sync Orders statuses");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when sync-order-statuses");
                Logger.Error(ex);
            }
        }

        #region internal operatio
        private async Task<JArray?> GetWMEOrders()
        {
            // Create an instance of HttpClient
            using var client = GetWMERestAPIHttpClient();

            try
            {
                // Configure JSON serializer options to use PascalCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };

                var refDate = DateTime.Today
                    .AddDays(-ConfigService.OrderWindowDays)// here => I have to set only 5 days maybe
                    .ToString("dd.MM.yyyy HH:mm");//01.11.2024 00:00

                var content = JsonContent.Create(new { DataReferinta = refDate }, new MediaTypeWithQualityHeaderValue("application/json"), options);

                // Call the API asynchronously
                HttpResponseMessage response = await client.PostAsync("\"GetInfoComenziExt\"", content);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JObject data = (JObject)JToken.ReadFrom(reader);

                        var infoComenzi = data["InfoComenzi"] as JArray;
                        if (infoComenzi != null)
                        {
                            return infoComenzi;
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when receiving last orders from WME");
                Logger.Error(ex);
            }

            return new JArray();
        }

        private async Task<string> OCSyncOrdersStatuses(WmeExportedOrder[] wmeExportedOrders)
        {
            // Create an instance of HttpClient
            using var client = GetWebAPIHttpClient();

            try
            {
                // Configure JSON serializer options to use PascalCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };

                var content = JsonContent.Create(wmeExportedOrders, new MediaTypeWithQualityHeaderValue("application/json"), options);

                // Call the API asynchronously
                var strContent = await content.ReadAsStringAsync();
                Logger.Debug("Sending orders to OpenCart");
                Logger.Debug(strContent);
                HttpResponseMessage response = await client.PostAsync("sync-orders-statuses", content);
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
                    return "Orders have not been updated.";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when pushing orders to OpenCart");
                Logger.Error(ex);
                return ex.Message;
            }
        }

        #endregion
    }
}
