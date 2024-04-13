namespace IPBSyncAppNetCore.Jobs
{
    public class Config
    {
        public static bool IsDebug = true;
        public static string WMERESTAPIURL = "http://iuliusrds.ipb.ro:8080/datasnap/rest/TServerMethods/";
        public static string WebRESTAPIURL = "http://ipb.test/ipb_sync_app/sync-app/api/";
        public static string WebAuthorizationToken = "frefierofreiofj";
        public static int BatchSize = 200;
    }
}
