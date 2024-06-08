using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using NLog;

namespace IPBSyncAppNetCore.Jobs
{
    public class DownloadOrdersJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Execute()
        {
            InternalExecute().Wait();
        }

        private async Task InternalExecute()
        {
            try
            {
                Logger.Info("Start Download Orders");

                Logger.Info("Get orders from OC/Laravel API ");
                OCOrder[] OCOrders = await DownloadOrders();
                    
                if (OCOrders == null)
                {
                    Logger.Error("Cannot receive orders from OC/Laravel");
                    return;
                }
                Logger.Info("Orders received from OC/Laravel");

                Logger.Info("Proces orders one by one. Import them into WME using REST API");

                foreach (var OCOrder in OCOrders)
                {
                    Logger.Info($"Sending order to WME...");
                    bool imported = false;
                    //aici 
                    //if (Config.IsDebug)
                    //{
                    //    imported = WriteOrderToFile(OCOrder);
                    //}
                    //else
                    //{
                    //    imported = SendOrderToWME(OCOrder);
                    //};

                    if (imported)
                    {
                        Logger.Info("Order has been successfully imported into WME");
                        //MarkOrderAsExported(OCOrder);
                    }
                    
                }

                Logger.Info("End Downloer orders from OC/Laravel");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when download orders from OC/Laravel");
                Logger.Error(ex);
            }
        }

        private async Task<JArray?> GetWMEResponse()
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

        private async Task<OCOrder[]> DownloadOrders()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", Config.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.GetAsync("orders/export");
                var strResponse = await response.Content.ReadAsStringAsync();
                Logger.Debug("Response from OpenCart");
                Logger.Debug(strResponse);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<OCOrder[]>(strResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when calling truncate endpoint on OpenCart");
                Logger.Error(ex);
            }

            return Array.Empty<OCOrder>();
        }

        private async Task CallTransferData()
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

        private async Task<string> GetWebAPIResponse(WMEProduct[] products)
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
    }

}
