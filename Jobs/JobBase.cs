using NLog;

namespace IPBSyncAppNetCore.Jobs
{
    public class JobBase
    {
        protected void ChangeFileDirectory(Logger Logger, string sourceFilePath, string targetDirectory)
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
