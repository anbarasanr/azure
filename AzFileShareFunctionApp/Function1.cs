using AzFileShareClientWrapper;
using Azure.Storage.Files.Shares.Models;
using FileStreamProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatternMatchLogicProvider;
using PatternMatchLogicProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzFileShareFunctionApp
{
    public static class Function1
    {
        public static IPatternMatchLogicProvider PatternMatchLogicProvider { get; set; }
        public static AzureFileShareWrapper AzureFileShareWrapper { get; set; }
        public static StreamProcessor StreamProcessor { get; set; }
        public static ILogger Logger { get; set; }
        public static IConfigurationRoot ConfigRoot { get; set; }

        public static string ConnectionString => ConfigRoot["AzureFileShare"];
        public static string PatternToMatch => ConfigRoot["PatternToMatch"];
        public static string SourceDirectory => ConfigRoot["SourceDirectory"];
        public static string DestinationDirectory => ConfigRoot["DestinationDirectory"];
        public static string FileShareName => ConfigRoot["FileShareName"];

        //Runs everyday at 6 PM
        [FunctionName("ProcessFilesInAzureFileShare")]
        public static void Run([TimerTrigger("0 0 18 * * *")] TimerInfo myTimer, ExecutionContext context, ILogger logger)
        {
            Initialise(context, logger);

            StartFileProcessing();
        }

        public static void Initialise(ExecutionContext context, ILogger logger)
        {
            if (Logger == null) Logger = logger;
            if (ConfigRoot == null) BuildConfiguration(context);
            if (PatternMatchLogicProvider == null) PatternMatchLogicProvider = new PatternMatcher(PatternToMatch);
            if (AzureFileShareWrapper == null) AzureFileShareWrapper = new AzureFileShareWrapper(ConnectionString, FileShareName, SourceDirectory, DestinationDirectory);
            if (StreamProcessor == null) StreamProcessor = new StreamProcessor(PatternMatchLogicProvider);
        }

        public static void BuildConfiguration(ExecutionContext context)
        {
            ConfigRoot = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public static void StartFileProcessing()
        {
            var fileNames = AzureFileShareWrapper.GetFileNamesFromSourceDirectory();

            List<Task> tasks = new List<Task>();

            foreach (var fileName in fileNames)
            {
                tasks.Add(Task.Run(() => ProcessFile(fileName)));

                if (tasks.Count == 10)
                    Task.WaitAll(tasks.ToArray());
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void ProcessFile(string fileName)
        {
            var fileClient = AzureFileShareWrapper.GetFile(fileName);

            using (var fileStream = fileClient.OpenRead(new ShareFileOpenReadOptions(false)))
            {
                if (StreamProcessor.IsMatchFoundInStream(fileStream))
                {
                    var copyStatus = AzureFileShareWrapper.CopyFileToDestination(fileName);
                    
                    if (copyStatus == CopyStatus.Success)
                    {
                        Logger.LogInformation($"Successfully copied [{fileName}] at {DateTime.Now}");

                        var deleteResponse = AzureFileShareWrapper.DeleteFileInSource(fileName);

                        if (deleteResponse.Status == 202)
                            Logger.LogInformation($"Successfully deleted [{fileName}] at {DateTime.Now}");
                        else
                            Logger.LogError($"Delete failed for [{fileName}] at {DateTime.Now}");
                    }
                    else
                    {
                        Logger.LogError($"Copy failed for [{fileName}] at {DateTime.Now}");
                    }
                }
            }

        }
    }
}