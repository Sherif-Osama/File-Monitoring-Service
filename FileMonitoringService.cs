using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace File_Monitoring
{
    // Windows Service that watches a source folder and moves new files to a destination folder,
    // writing simple logs to a file and the Event Viewer on error.
    public partial class FileMonitoringService : ServiceBase
    {
        // File system watcher used to detect new files in the source directory.
        private FileSystemWatcher Watcher { get; set; }

        // Paths read from app settings: source folder, destination folder and log folder.
        private readonly string DirectorySourcePath = ConfigurationManager.AppSettings["Source"];
        private readonly string DirectoryDestinationPath = ConfigurationManager.AppSettings["Destination"];
        private readonly string LogFolder = ConfigurationManager.AppSettings["Log"];

        // Event source name used for writing to the Windows Event Log.
        private const string EventSource = "FileMonitoringService";

        public FileMonitoringService()
        {
            // Initialize Windows Service components (designer generated).
            InitializeComponent();
        }

        // Ensure a folder exists; create it if missing.
        void CreateFolder(string FolderPath)
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);
            }
            catch (Exception ex) { Logs(ex.Message); } // Log creation errors
        }

        // Called when the service starts.
        protected override void OnStart(string[] args)
        {


            // Ensure required folders exist.
            CreateFolder(DirectorySourcePath);
            CreateFolder(DirectoryDestinationPath);
            CreateFolder(LogFolder);
            Logs("Service Started");

            if (Environment.UserInteractive)
                Console.WriteLine($"Folders are exist: {DirectorySourcePath},{DirectoryDestinationPath}, {LogFolder}");

            // Configure the FileSystemWatcher to monitor the source directory for new files.
            Watcher = new FileSystemWatcher();
            Watcher.Filter = "*.*";
            Watcher.Path = DirectorySourcePath;
            Watcher.Created += OnFileCreated;
            Watcher.EnableRaisingEvents = true;
        }

        // Event handler: called when a new file is created in the source folder.
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Logs($"File detected: {e.FullPath}");


            // Generate a new destination filename (GUID preserving original extension).
            string newFileName = GenerateFileName(e.FullPath);

            if (Environment.UserInteractive)
                Console.WriteLine($"New file name : {newFileName}");

            // Combine destination folder and new filename, then attempt to move.
            string fullFilePath = Path.Combine(DirectoryDestinationPath, newFileName);

            if (Environment.UserInteractive)
            {
                Console.WriteLine($"File to move: {e.FullPath}");
                Console.WriteLine($"New file full path : {fullFilePath}");
            }

            MoveFile(e.FullPath, fullFilePath);
        }

        // Attempts to move a file with retries to handle temporary IO locks.
        private void MoveFile(string sourceFile, string destinationFile)
        {
            const int maxRetries = 10;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.Move(sourceFile, destinationFile);
                    Logs($"File moved: {sourceFile} -> {destinationFile}");
                    if (Environment.UserInteractive)
                    {
                        Console.WriteLine("File moved successfully.");
                        Console.WriteLine($"From: {sourceFile}");
                        Console.WriteLine($"To  : {destinationFile}");
                        Console.WriteLine();
                    }
                    return;
                }
                catch (IOException ex) { if (i == maxRetries - 1) Logs(ex.Message); Thread.Sleep(500); }
            }

            // If all retries fail, log a failure message.
            Logs($"Failed to move file: {sourceFile}");
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Failed to move file.");
                Console.WriteLine($"File: {sourceFile}");
            }

        }

        // Create a new filename based on a GUID and preserve the original extension.
        private string GenerateFileName(string OriginalFilePath)
        {
            FileInfo File = new FileInfo(OriginalFilePath);

            return (Guid.NewGuid() + File.Extension);
        }

        // Append simple timestamped messages to the service log file.
        void Logs(string Message)
        {
            try
            {
                string LogFile = Path.Combine(LogFolder, "ServiceLog.txt");
                File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {Message} {Environment.NewLine}");
            }
            catch (Exception ex) { LogInEventViewer(ex); } // If file logging fails, write to Event Viewer.
        }

        // Write exceptions to the Windows Event Viewer under a specific source.
        void LogInEventViewer(Exception ex)
        {
            if (!EventLog.SourceExists(EventSource))
            { EventLog.CreateEventSource(EventSource, "Application"); }

            EventLog.WriteEntry(EventSource, ex.Message, EventLogEntryType.Error);
        }

        // Called when the service stops; detach events and dispose the watcher.
        protected override void OnStop()
        {
            if (Watcher != null)
            {
                Watcher.Created -= OnFileCreated;
                Watcher.EnableRaisingEvents = false;
                Watcher.Dispose();
                Watcher = null;
                Logs("Service Stopped");
            }
        }
        public void DebugMode()
        {
            Console.Title = "File Monitoring Service - Debug Mode";

            Console.Clear();

            Console.WriteLine("==============================================");
            Console.WriteLine("      File Monitoring Service (Debug Mode)");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            OnStart(null);

            Console.WriteLine("File Monitoring Service is running.");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            OnStop();

            Console.WriteLine("File Monitoring Service stopped.");
        }
    }
}