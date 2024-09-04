using System.Text.Json.Serialization;

namespace IPBSyncAppNetCore.Jobs.Models
{
    public class WmeOrder
    {
        public string? AnLucru { get; set; }
        public string? LunaLucru { get; set; }
        public string TipDocument = "COMANDA";
        public string Moneda = "LEI";
        public string? NrDoc {  get; set; }
        public string? DataDoc { get; set; }
        public string? IDClient { get; set; }
        [JsonIgnore]
        public bool PF {  get; set; }
        [JsonIgnore]
        public string PhoneOrCUI { get; set; }

        public WmeItem[]? Items { get; set; }
    }

    public class WmeItem
    {
        public string ID { get; set; }
        public int Cant { get; set; }
        public string UM { get; set; }
        public decimal Pret { get; set; }
    }
}
