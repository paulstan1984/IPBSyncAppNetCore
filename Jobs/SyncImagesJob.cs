﻿using IPBSyncAppNetCore.Jobs.Models;
using Newtonsoft.Json;
using NLog;
using System.Net;

namespace IPBSyncAppNetCore.Jobs
{
    public class SyncImagesJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Execute() => RunJob().Wait();

        private async Task RunJob()
        {
            try
            {
                Logger.Info("Start sync images job");

                string[] images = LoadImagesFromFolder(Config.ImagesPathDir);

                if (images == null || images.Length ==0)
                {
                    Logger.Error("No new images that have to be uploaded");
                    return;
                }
                
                foreach (var image in images)
                {
                    Logger.Info($"upload image {image}...");

                    //upload the file
                    if(UploadFileToFtp(Config.FTPHost, image, Config.FTPUser, Config.FTPPassword))
                    {
                        //assign the file to the product
                        bool assigned = await AssignImageToProduct(image);

                        if (!assigned)
                        {
                            Logger.Error($"Image {image} couldn't be assigned to the corresponding product.");
                        }

                        ChangeFileDirectory(image, Config.FTPUser);
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
            string ftpFullUrl = $"{ftpUrl}/{fileName}";

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
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WebRESTAPIURL);
            client.DefaultRequestHeaders.Add("Authorization", Config.WebAuthorizationToken);

            try
            {
                HttpResponseMessage response = await client.GetAsync($"assign-image-to-article/{productEanImage}");
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

        private void ChangeFileDirectory(string sourceFilePath, string targetDirectory)
        {
            // Check if source file exists
            if (!File.Exists(sourceFilePath))
            {
                Logger.Error($"Source file does not exist: {sourceFilePath}.");
                return;
            }

            // Ensure the target directory exists
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Get the file name from the source file path
            string fileName = Path.GetFileName(sourceFilePath);

            // Combine the target directory and file name to create the new path
            string targetFilePath = Path.Combine(targetDirectory, fileName);

            // Move the file to the new directory
            File.Move(sourceFilePath, targetFilePath);
        }
    }
}
