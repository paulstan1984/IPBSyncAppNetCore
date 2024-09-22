namespace IPBSyncAppNetCore.Jobs.Models
{
    public class WMECategory
    {
        public int Cod { get; set; }
        public string? Denumire { get; set; }
        public string? Simbol { get; set; }
        public int Nivel { get; set; }
        public int? CodParinte {  get; set; } 
    }
}
