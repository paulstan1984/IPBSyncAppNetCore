using IPBSyncAppNetCore.utils;
using Newtonsoft.Json;
using System.Globalization;
using System.Runtime.Serialization;

namespace IPBSyncAppNetCore.Jobs.Models
{
    public class OCOrder
    {
        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("telephone")]
        public string? Phone { get; set; }

        [JsonProperty("products")]
        public List<OCOrderProduct>? Products { get; set; }
        [JsonProperty("totals")]
        public List<OCOrderTotal>? Totals { get; set; }

        [JsonProperty("date_added")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime DateAdded { get; set; }

        public WmeOrder WmeOrder => new WmeOrder
        {
            AnLucru = DateAdded.Year.ToString(),
            LunaLucru = DateAdded.Month.ToString(),
            DataDoc = DateAdded.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            NrDoc = OrderId.ToString(),
            Items = Products
                ?.Select(p => new WmeItem
                {
                    Pret = p.Price,
                    Cant = p.Quantity,
                    UM = p.Location ?? "BUC",
                    ID = p.Model
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
