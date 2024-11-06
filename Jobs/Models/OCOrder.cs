using IPBSyncAppNetCore.utils;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IPBSyncAppNetCore.Jobs.Models
{
    public class OCOrder
    {
        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("firstname")]
        public string? FirstName { get; set; }

        [JsonProperty("lastname")]
        public string? LastName { get; set; }

        [JsonProperty("payment_company")]
        public string? Firma { get; set; }
        [JsonProperty("payment_city")]
        public string? City { get; set; }
        [JsonProperty("payment_address1")]
        public string? Address { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("telephone")]
        public string Phone { get; set; }

        [JsonProperty("products")]
        public List<OCOrderProduct>? Products { get; set; }
        [JsonProperty("totals")]
        public List<OCOrderTotal>? Totals { get; set; }

        [JsonProperty("date_added")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime DateAdded { get; set; }

        [JsonProperty("tax")]
        public string CUI {  get; set; }

        public WmeOrder WmeOrder => new WmeOrder
        {
            AnLucru = DateAdded.Year.ToString(),
            LunaLucru = DateAdded.Month.ToString(),
            DataDoc = DateAdded.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            NrDoc = OrderId.ToString(),
            PF = string.IsNullOrEmpty(CUI),
            PhoneOrCUI = !string.IsNullOrEmpty(CUI) 
                ? Regex.Replace(CUI, @"\D", "")
                : Phone, 

            Items = Products
                ?.Select(p => new WmeItem
                {
                    Pret = p.ProdPrice,
                    Cant = p.Quantity,
                    UM = p.Location ?? "BUC",
                    ID = p.Ean
                })
                .ToArray()
        };
    }

    public class OCOrderProduct
    {
        [JsonProperty("product_id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }
        [JsonProperty("tax")]
        public decimal Tax { get; set; }

        [JsonProperty("prod_price")]
        public decimal ProdPrice { get; set; }
    }

    public class OCOrderTotal
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }
    }
}
