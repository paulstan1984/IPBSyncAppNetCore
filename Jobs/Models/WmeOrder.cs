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

        public WmeItem[]? Items { get; set; }
    }

    public class WmeItem
    {
        public string ID { get; set; }
        public int Cant { get; set; }
        public string UM { get; set; }
        public decimal Pret { get; set; }
    }

    /*
     * 
     * PackageData.Add(String.Format("Item_{0}={1};{2};{3};{4};{5};",
                LineNo++,
                SKU,
                UM,
                OrderItem["quantity"],
                double.Parse((string)OrderItem["standard_price"]).ToString("F4", CultureInfo.InvariantCulture),
                0
            ));
     * 
     * PackageData.Add("[InfoPachet]");
            PackageData.Add(String.Format("AnLucru={0}", OrderDate.Year));
            PackageData.Add(String.Format("LunaLucru={0}", OrderDate.Month));
            PackageData.Add("Tipdocument=COMANDA");
            PackageData.Add("TotalComenzi=1");
            PackageData.Add("");
            PackageData.Add("[Comanda_1]");

            PackageData.Add("SimbolCarnet=" + Store);
            PackageData.Add("Operatie=A");
            PackageData.Add(String.Format("NrDoc={0}", Order["order_id"]));
            PackageData.Add(String.Format("Data={0}", OrderDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));

            bool PF = String.IsNullOrEmpty((string)Order["payment_tax_id"]) || (string)Order["payment_tax_id"] == "0";
            String ClientExternalCode = PF ? ((string)Order["email"]).ToUpper() : (string)Order["payment_tax_id"];

            ClientExternalCode = ClientExternalCode.Replace(" ", "").ToUpper();

            if (!PF)
            {
                ClientExternalCode = Utils.FormatCUI(ClientExternalCode);
            }
            
            log.InfoFormat("payment_tax_id={0}", ClientExternalCode);

            var client = this.GetClient(ClientExternalCode);
            if (client == null)//the client does not exist
            {
                MailWrapper mailWrapper = WrappersFactory.GetMailWrapper();
                mailWrapper.SendMail(ConfigurationManager.AppSettings["mail_new_partener"], "Partener Nou", "Comanda de la un partener nou. Cod Client:" + ClientExternalCode + ". Tip client: " + (PF ? "PF" : "PJ"));
                //if the client does not exist, save it into database
                this.SaveOrderClient(ClientExternalCode, PF, Order);
                client = this.GetClient(ClientExternalCode);
            }

            if (client != null)
            {
                var CUI = client.Split(new char[] { ';' })[2];
                PackageData.Add(String.Format("CodClient={0}", CUI));
            }
            else
            {
                PackageData.Add(String.Format("CodClient={0}", ClientExternalCode));
            }

            PackageData.Add(String.Format("Moneda={0}", "LEI"));
            PackageData.Add(String.Format("TotalArticole={0}", ((JArray)Order["Products"]).Count));
            
     */
}
