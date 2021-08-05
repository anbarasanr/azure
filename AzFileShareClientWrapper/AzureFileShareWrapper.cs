using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System;
using System.Collections.Generic;

namespace AzFileShareClientWrapper
{

    public class AzureFileShareWrapper
    {
        public string ConnectionString { get; }
        public string ShareName { get; }
        public string SourceDirectory { get; }
        public string DestinationDirectory { get; }
        public ShareClient FileShareClient { get; }
        public ShareDirectoryClient FileShareRootClient { get => FileShareClient.GetRootDirectoryClient(); }
        public ShareDirectoryClient SourceDirectoryClient { get => FileShareRootClient.GetSubdirectoryClient(SourceDirectory); }
        public ShareDirectoryClient DestinationDirectoryClient { get => FileShareRootClient.GetSubdirectoryClient(DestinationDirectory); }

        /// <summary>
        /// This wrapper class is meant for accessing Azure File Share via simpler methods.
        /// All methods are currently designed as synchronous
        /// </summary>
        /// <param name="connectionString">Azure storage connection string</param>
        /// <param name="shareName">Asure File share name</param>
        /// <param name="sourceDirectory">Source directory from which the files are to be returned</param>
        /// <param name="destinationDirectory">Destination directory to which the files to go to</param>
        public AzureFileShareWrapper(string connectionString, string shareName, string sourceDirectory, string destinationDirectory)
        {
            ConnectionString = connectionString;
            ShareName = shareName;
            SourceDirectory = sourceDirectory;
            DestinationDirectory = destinationDirectory;
            FileShareClient = new ShareClient(ConnectionString, ShareName);
        }

        /// <summary>
        /// Gets file names from Source directory
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFileNamesFromSourceDirectory()
        {
            if (SourceDirectoryClient != null)
            {
                var sourceFilesAndDirectories = SourceDirectoryClient.GetFilesAndDirectories();
                foreach (ShareFileItem item in sourceFilesAndDirectories)
                {
                    if (!item.IsDirectory)
                    {
                        yield return item.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the file client object from source directory in Azure Fileshare using file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ShareFileClient GetFile(string fileName)
        {
            return SourceDirectoryClient.GetFileClient(fileName);
        }

        /// <summary>
        /// Copies the file with the given name from source directory in Azure Fileshare and puts into the destination directory.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public CopyStatus CopyFileToDestination(string fileName)
        {
            var sourceFileClient = SourceDirectoryClient.GetFileClient(fileName);
            var sourceFileSasUri = sourceFileClient.GenerateSasUri(Azure.Storage.Sas.ShareFileSasPermissions.All, DateTimeOffset.UtcNow.AddHours(1));
            
            var destinationFileClient = DestinationDirectoryClient.GetFileClient(fileName);

            var response = destinationFileClient.StartCopy(sourceFileSasUri);

            return response.Value.CopyStatus;
        }

        /// <summary>
        /// Deletes a given file using the fileName in source directory in Azure Fileshare.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>

        public Response DeleteFileInSource(string fileName)
        {
            return SourceDirectoryClient.DeleteFile(fileName);
        }
    }
}
