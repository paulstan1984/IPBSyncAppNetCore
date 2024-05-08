using System.Text.Json.Serialization;

namespace IPBSyncAppNetCore.Jobs.Models
{
    public class WmeStock
    {
        public string CodExtern { get; set; }
        public string Denumire { get; set; }
        [JsonIgnore]
        public string Stoc { get; set; }
        public int Stock
        {
            get
            {
                int cStock = 0;
                int.TryParse(Stoc, out cStock);

                return cStock;
            }
        }
    }
}
