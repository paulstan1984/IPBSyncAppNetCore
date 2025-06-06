﻿using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog;
using System.Net.Http.Headers;
using System.Text.Json;

namespace IPBSyncAppNetCore.Jobs
{
    public class DownloadOrdersJob : JobBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DownloadOrdersJob(IHttpClientFactory httpClientFactory) 
            : base(httpClientFactory)
        {
        }

        public override async Task RunJob()
        {
            try
            {
                Logger.Info("Start Download Orders");

                Logger.Info("Get orders from OC/Laravel API ");
                OCOrder[] OCOrders = await OCDownloadOrders();
                    
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

                    var WMEOrder = OCOrder.WmeOrder;

                    if (!string.IsNullOrEmpty(WMEOrder.PhoneOrCUI))
                    {
                        WMEOrder.IDClient = await GetIdClientFromWME(WMEOrder.PF, WMEOrder.PhoneOrCUI);
                    }

                    //if the client does not exist create it
                    if (string.IsNullOrEmpty(WMEOrder.IDClient))
                    {
                        WMEOrder.IDClient = await CreatePartenerOnWME(WMEOrder.PF, OCOrder);
                    }

                    imported = await SendOrderToWME(WMEOrder);
                    if (imported)
                    {
                        Logger.Info("Order has been successfully imported into WME");
                        OCMarkOrderAsExported(OCOrder);
                    }
                    
                }

                Logger.Info("End Downloer orders from OC/Laravel");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when downloading orders job");
                Logger.Error(ex);
            }
        }

        private async Task<string?> GetIdClientFromWME(bool PF, string searchField)
        {
            using var client = GetWMERestAPIHttpClient();
            try
            {
                dynamic? searchRequest = null;
                if (PF)
                {
                    searchRequest = new { Telefon = searchField };
                }
                else
                {
                    searchRequest = new { CodFiscal = searchField };
                }
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var content = JsonContent.Create(searchRequest, new MediaTypeWithQualityHeaderValue("application/json"), options);
                HttpResponseMessage response = await client.PostAsync("getInfoParteneri", content);
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JObject data = (JObject)JToken.ReadFrom(reader);
                        var partner = (data["InfoParteneri"] as JArray)?.FirstOrDefault();
                        if (partner != null)
                        {
                            return searchField;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting client from WME");
                Logger.Error(ex);
            }
            return string.Empty;
        }

        private async Task<string> CreatePartenerOnWME(bool PF, OCOrder order)
        {
            // Create an instance of HttpClient
            using var client = GetWMERestAPIHttpClient();

            try
            {
                WMEAddPartenerRequest addRequest = CreateAddPartenerRequest(PF, order);

                // Configure JSON serializer options to use PascalCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };
                var content = JsonContent.Create(addRequest, new MediaTypeWithQualityHeaderValue("application/json"), options);

                // Call the API asynchronously
                HttpResponseMessage response = await client.PostAsync("InfoPartener//", content);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                    {
                        JObject data = (JObject)JToken.ReadFrom(reader);

                        if (data["Error"].ToString() == "ok")
                        {
                            return PF
                                ? order.Phone
                                : order.CUI;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when receiving products from WME");
                Logger.Error(ex);
            }

            return string.Empty;
        }

        private WMEAddPartenerRequest CreateAddPartenerRequest(bool PF, OCOrder order)
        {
            var request = new WMEAddPartenerRequest
            {
                CUI = order.CUI,
                CodExtern = PF ? order.Phone : order.CUI,
                Nume = PF ? $"{order.FirstName} {order.LastName}" : order.Firma ?? string.Empty,
                PersoanaFizica = PF ? "DA" : "NU",
                Sedii = [
                    new WMESediu
                    {
                        Localitate = order.City ?? string.Empty,
                        Strada = order.Address ?? string.Empty,
                        Telefon = order.Phone,
                    }
                ],
                PersoaneContect = [
                    new WMEPersoana {
                        Prenume = order.FirstName ?? string.Empty,
                        Nume = order.LastName ?? string.Empty,
                        Telefon = order.Phone
                    }
                ]
            };

            return request;

        }

        private async Task<OCOrder[]> OCDownloadOrders()
        {
            // Create an instance of HttpClient
            using var client = GetWebAPIHttpClient();

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

        private async Task<bool> SendOrderToWME(WmeOrder order)
        {
            if (ConfigService.IsDebug)
            {
                File.WriteAllText(@$"C:\laragon\www\ipb\order-{order.NrDoc}-{order.DataDoc}.json", JsonConvert.SerializeObject(order));
                return true;
            }
            else
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
                    var content = JsonContent.Create(order, new MediaTypeWithQualityHeaderValue("application/json"), options);

                    // Call the API asynchronously
                    HttpResponseMessage response = await client.PostAsync("ComandaClient//", content);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseStream = await response.Content.ReadAsStreamAsync())
                        using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                        {
                            JObject data = (JObject)JToken.ReadFrom(reader);

                            if (data.ContainsKey("Error") && data["Error"].ToString() != "ok")
                            {
                                throw new Exception(data["Error"].ToString());
                            }

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("An error appeared when importing order to WME");
                    Logger.Error(ex);

                    return false;
                }
            };

            return false;
        }

        private async void OCMarkOrderAsExported(OCOrder order)
        {
            // Create an instance of HttpClient
            using var client = GetWebAPIHttpClient();

            try
            {
                dynamic[] request = [new
                {
                    order_id = order.OrderId
                }];

                // Configure JSON serializer options to use PascalCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                };
                var content = JsonContent.Create(request, new MediaTypeWithQualityHeaderValue("application/json"), options);

                HttpResponseMessage response = await client.PostAsync("orders/mark-exported", content);
                var strResponse = await response.Content.ReadAsStringAsync();
                Logger.Debug("Response from OpenCart");
                Logger.Debug(strResponse);
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when calling truncate endpoint on OpenCart");
                Logger.Error(ex);
            }
        }
    }

}
