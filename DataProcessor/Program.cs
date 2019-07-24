using System;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace DataProcessor
{
    class Program
    {
        private static ConcurrentDictionary<string, string> FilesToProcess = new ConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {
            WriteLine("Parsing Command Line Options");

            var directoryToWatch = args[0];

            if (!Directory.Exists(directoryToWatch))
            {
                WriteLine($"ERROR: {directoryToWatch} does not exist");
            }
            else
            {
                WriteLine($"Watching directory {directoryToWatch} for changes");

                using (var inputFileWatcher = new FileSystemWatcher(directoryToWatch))
                using (var timer =  new Timer(ProcessFiles, null, 0, 1000))
                {
                    inputFileWatcher.IncludeSubdirectories = false;
                    inputFileWatcher.InternalBufferSize = 32768; //32KB, default:8KB
                    inputFileWatcher.Filter = "*.*";
                    //inputFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                    inputFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

                    inputFileWatcher.Created += FileCreated;
                    inputFileWatcher.Changed += FileChanged;
                    inputFileWatcher.Deleted += FileDeleted;
                    inputFileWatcher.Renamed += FileRenamed;
                    inputFileWatcher.Error += WatchError;

                    inputFileWatcher.EnableRaisingEvents = true;

                    WriteLine("Press Enter to quit");
                    ReadLine();
                }
            }
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File created: {e.Name} - type: {e.ChangeType}");

            //var fileProcessor = new FileProcessor(e.FullPath);
            //fileProcessor.Process();

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);  //if a fullpath key already exists, it won't be added again
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File changed: {e.Name} - type: {e.ChangeType}");

            //var fileProcessor = new FileProcessor(e.FullPath);
            //fileProcessor.Process();

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
        }

        private static void FileDeleted(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File Changed: {e.Name} - type: {e.ChangeType}");
        }

        private static void FileRenamed(object sender, RenamedEventArgs e)
        {
            WriteLine($"* File Renamed: {e.OldName} to {e.Name} - type: {e.ChangeType}");
        }

        private static void WatchError(object sender, ErrorEventArgs e)
        {
            WriteLine($"ERROR: file system watching may no longer be active: {e.GetException()}");
        }

        private static void ProcessFiles(Object stateInfo)
        {
            foreach (var fileName in FilesToProcess.Keys) // May not be in order of adding
            {
                if (FilesToProcess.TryRemove(fileName, out _))
                {
                    var fileProcessor = new FileProcessor(fileName);
                    fileProcessor.Process();
                }
            }
        }

        private static void ProcessSingleFile(string filePath)
        {
            var fileProcessor = new FileProcessor(filePath);
            fileProcessor.Process();
        }

        private static void ProcessDirectory(string directoryPath, string fileType)
        {
            //var allFiles = Directory.GetFiles(directoryPath); // to get all files

            switch (fileType)
            {
                case "TEXT":
                    string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                    foreach(var textFilePath in textFiles)
                    {
                        var fileProcessor = new FileProcessor(textFilePath);
                        fileProcessor.Process();
                    }
                    break;
                default:
                    WriteLine($"ERROR: {fileType} is not supported");
                    return;
            }
        }
    }
}
