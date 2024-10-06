using NLog;
using System.Net;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncImagesJob : JobBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SyncImagesJob(IHttpClientFactory httpClientFactory) 
            : base(httpClientFactory)
        {
        }

        public override async Task RunJob()
        {
            try
            {
                Logger.Info("Start sync images job");

                string[] images = LoadImagesFromFolder(ConfigService.ImagesPathDir);

                if (images == null || images.Length ==0)
                {
                    Logger.Error("No new images that have to be uploaded");
                    return;
                }
                
                foreach (var image in images)
                {
                    Logger.Info($"upload image {image}...");

                    //upload the file
                    if(UploadFileToFtp(ConfigService.FTPHost, image, ConfigService.FTPUser, ConfigService.FTPPassword))
                    {
                        //assign the file to the product
                        bool assigned = await AssignImageToProduct(Path.GetFileNameWithoutExtension(image));

                        if (!assigned)
                        {
                            Logger.Error($"Image {image} couldn't be assigned to the corresponding product.");
                        }

                        ChangeFileDirectory(Logger, image, ConfigService.UploadedImagesPathDir);
                    }
                }

                Logger.Info("End sync images job");
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when sync images job");
                Logger.Error(ex);
            }
        }


        private string[] LoadImagesFromFolder(string folderPath)
            => Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                        .Take(50)
                        .ToArray();


        private bool UploadFileToFtp(string ftpUrl, string filePath, string ftpUsername, string ftpPassword)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileName = fileInfo.Name;

            // Combine the FTP URL and the file name to get the destination path on the FTP server
            string ftpFullUrl = $"ftp://{ftpUrl}/{fileName}";

            // Create an FtpWebRequest object
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Set the credentials (username and password)
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.EnableSsl = false; // Set to true if using FTP over SSL
            request.UsePassive = true;  // Use passive mode if necessary
            request.UseBinary = true;   // Upload the file in binary mode
            request.KeepAlive = false;  // Close the connection when done

            // Read the file contents and write them to the request stream
            byte[] fileContents;
            using (FileStream fs = fileInfo.OpenRead())
            {
                fileContents = new byte[fs.Length];
                fs.Read(fileContents, 0, fileContents.Length);
            }

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            // Get the response from the FTP server
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusCode == FtpStatusCode.ClosingData;
            }
        }

        private async Task<bool> AssignImageToProduct(string productEanImage)
        {
            // Create an instance of HttpClient
            using var client = GetWebAPIHttpClient();

            try
            {
                HttpResponseMessage response = await client.PutAsync($"assign-image-to-article/{productEanImage}", null);
                var strResponse = await response.Content.ReadAsStringAsync();
                Logger.Debug("Response from OpenCart");
                Logger.Debug(strResponse); 

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("An error appeared when assigning an image to a product");
                Logger.Error(ex);
                return false;
            }
        }
    }
}
