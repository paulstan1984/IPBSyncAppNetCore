namespace IPBSyncAppNetCore.Jobs.Models
{
    public class WMEAddPartenerRequest
    {
        public string TipOperatie => "A";

        public string CUI {  get; set; }
        public string CodExtern { get; set; }
        public string Nume { get; set; }
        public string PersoanaFizica { get; set; }
        public string PartenerBlocat => "NU";
        public WMESediu[] Sedii { get; set; }
        public WMEPersoana[] PersoaneContect {  get; set; }
    }

    public class WMESediu
    {
        public string Localitate { get; set; }
        public string Tip = "S";
        public string Strada { get; set; }
        public string Telefon { get; set; }
    }

    public class WMEPersoana
    {
        public string Nume { get; set; }
        public string Prenume {  get; set; }
        public string Telefon { get; set; }
    }
}
