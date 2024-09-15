namespace IPBSyncAppNetCore.Jobs
{
    public class IpbSyncAppConfig
    {
        public bool IsDebug { get; set; }

        public string? WMERESTAPIURL { get; set; }
        public string? WebRESTAPIURL { get; set; }
        public string? WebAuthorizationToken { get; set; }
        public int BatchSize { get; set; }

        public string? ImagesPathDir { get; set; }
        public string? UploadedImagesPathDir { get; set; }
        public string? FTPHost { get; set; }
        public string? FTPUser { get; set; }
        public string? FTPPassword { get; set; }

        public string? DescriptionsPathDir { get; set; }
        public string? UploadedDescriptionsPathDir { get; set; }
        public string? DescriptionSeparator { get; set; }
    }

    public class ConfigService
    {
        public static IpbSyncAppConfig? Configuration { get; set; }

        public static bool IsDebug => Configuration?.IsDebug ?? false;
        public static string WMERESTAPIURL => Configuration?.WMERESTAPIURL ?? "http://iuliusrds.ipb.ro:8080/datasnap/rest/TServerMethods/";
        public static string WebRESTAPIURL => Configuration?.WebRESTAPIURL ?? "http://ipb.test/ipb_sync_app/sync-app/api/";
        public static string WebAuthorizationToken => Configuration?.WebAuthorizationToken ?? "frefierofreiofj";
        public static int BatchSize => Configuration?.BatchSize ?? 20;
        public static string ImagesPathDir => Configuration?.ImagesPathDir ?? "C:\\laragon\\www\\ipb\\images\\";
        public static string UploadedImagesPathDir => Configuration?.UploadedImagesPathDir ?? "C:\\laragon\\www\\ipb\\images\\uploaded";
        public static string FTPHost => Configuration?.FTPHost ?? "89.39.190.165";
        public static string FTPUser => Configuration?.FTPUser ?? "ipbsyncapp-img";
        public static string FTPPassword => Configuration?.FTPPassword ?? "Dev65gt!@";
        public static string DescriptionsPathDir => Configuration?.DescriptionsPathDir ?? "C:\\laragon\\www\\ipb\\descriptions\\";
        public static string UploadedDescriptionsPathDir => Configuration?.UploadedDescriptionsPathDir ?? "C:\\laragon\\www\\ipb\\descriptions\\uploaded";
        public static string DescriptionSeparator => Configuration?.DescriptionSeparator ?? "________________________________________";

    }
}
