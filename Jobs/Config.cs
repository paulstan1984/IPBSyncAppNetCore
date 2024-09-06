namespace IPBSyncAppNetCore.Jobs
{
    public class Config
    {
        public static bool IsDebug = false;
        public static string WMERESTAPIURL = "http://iuliusrds.ipb.ro:8080/datasnap/rest/TServerMethods/";
        public static string WebRESTAPIURL = "http://ipb.test/ipb_sync_app/sync-app/api/";
        public static string WebAuthorizationToken = "frefierofreiofj";
        public static int BatchSize = 200;

        public static string ImagesPathDir = @"C:\laragon\www\ipb\images\";
        public static string UploadedImagesPathDir = @"C:\laragon\www\ipb\images\uploaded";
        public static string FTPHost = @"89.39.190.165";
        public static string FTPUser = @"ipbsyncapp-img";
        public static string FTPPassword = @"Dev65gt!@";
    }
}
