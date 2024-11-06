using System.Text.Json.Serialization;

namespace IPBSyncAppNetCore.Jobs.Models
{
    public class WMEProductStock
    {
        public string Subunitate { get; set; }
        public string StocMinim { get; set; }
    }
    public class WMEProduct
    {
        public string CodObiect { get; set; }
        public string Denumire { get; set; }
        public string UM { get; set; }
        [JsonIgnore]
        public string PretVanzare { get; set; }
        public decimal PretVanzareDec
        {
            get
            {
                decimal outPretVanzare = 0;
                PretVanzare = PretVanzare.Replace(",", ".");
                decimal.TryParse(PretVanzare, out outPretVanzare);

                return outPretVanzare;
            }
        }
        [JsonIgnore]
        public string PretCuTVA { get; set; }
        public decimal PretCuTVADec
        {
            get
            {
                decimal outPretCuTVA = 0;
                PretCuTVA = PretCuTVA.Replace(",", ".");
                decimal.TryParse(PretCuTVA, out outPretCuTVA);

                return outPretCuTVA;
            }
        }
        public string SimbolClasa { get; set; }
        public string Producator { get; set; }
        public string CodExtern { get; set; }
        public string CodIntern { get; set; }
        [JsonIgnore]
        public string Masa { get; set; }
        public decimal MasaDec {
            get
            {
                decimal outMasa = 0;
                decimal.TryParse(Masa, out outMasa);

                return outMasa;
            }
        }
        [JsonIgnore]
        public WMEProductStock[] StocPeSubunitati { get; set; }
        public int Stock
        {
            get
            {
                if (StocPeSubunitati == null || StocPeSubunitati.Length == 0) return 0;

                int s = 0;
                int cStock = 0;
                foreach (var stock in StocPeSubunitati)
                {
                    cStock = 0;
                    int.TryParse(stock.StocMinim, out cStock);
                    s += cStock;
                }

                return s;
            }
        }
    }
}
