﻿using static System.Console;
using System.IO;
using System;

namespace DataProcessor
{
    internal class FileProcessor
    {
        private static readonly string BackupDirectoryName = "backup";
        private static readonly string InProgressDirectoryName = "processing";
        private static readonly string CompletedDirectoryName = "complete";

        public string InputFilePath { get; }

        public FileProcessor(string filePath)
        {
            this.InputFilePath = filePath;
        }

        public void Process()
        {
            WriteLine($"Begin Process of {InputFilePath}");

            //Check if file exists
            if (!File.Exists(InputFilePath))
            {
                WriteLine($"ERROR: file {InputFilePath} does not exist.");
                return;
            }

            string rootDirectoryPath = new DirectoryInfo(InputFilePath).Parent.Parent.FullName;
            WriteLine($"Root data path is {rootDirectoryPath}");

            //check if backup dir exists
            string inputFileDirectoryPath = Path.GetDirectoryName(InputFilePath);
            string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);

            //if (!Directory.Exists(backupDirectoryPath))
            //{
                //WriteLine($"Creating {backupDirectoryPath}");
                Directory.CreateDirectory(backupDirectoryPath); //won't throw exception if directory already exists
            //}

            //copy file to backup dir
            string inputFileName = Path.GetFileName(InputFilePath);
            string backupFilePath = Path.Combine(backupDirectoryPath, inputFileName);
            WriteLine($"Copying {InputFilePath} to {backupFilePath}");
            File.Copy(InputFilePath, backupFilePath, true); //true for overwrite, so that if file already exists, exception is not thrown

            //move file to in-progress directory
            Directory.CreateDirectory(Path.Combine(rootDirectoryPath, InProgressDirectoryName));
            string inProgressFilePath = Path.Combine(rootDirectoryPath, InProgressDirectoryName, inputFileName);

            if (File.Exists(inProgressFilePath))
            {
                WriteLine($"ERROR: a file with name {inProgressFilePath} is already being processed.");
                return;
            }

            WriteLine($"Moving {InputFilePath} to {inProgressFilePath}");
            File.Move(InputFilePath, inProgressFilePath);

            //Determine type of File
            string extension = Path.GetExtension(InputFilePath);
            switch (extension)
            {
                case ".txt":
                    ProcessTextFile(inProgressFilePath);
                    break;
                default:
                    WriteLine($"{extension} is unsupported file type.");
                    break;
            }

            string completedDirectoryPath = Path.Combine(rootDirectoryPath, CompletedDirectoryName);
            Directory.CreateDirectory(completedDirectoryPath);

            WriteLine($"Moving {inProgressFilePath} to {completedDirectoryPath}");
            //File.Move(inProgressFilePath, Path.Combine(completedDirectoryPath, inputFileName));

            var completedFileName = $"{Path.GetFileNameWithoutExtension(InputFilePath)}-{Guid.NewGuid()}{extension}";
            //completedFileName = Path.ChangeExtension(completedFileName, ".complete");
            var completedFilePath = Path.Combine(completedDirectoryPath, completedFileName); 
            File.Move(inProgressFilePath, completedFilePath);

            //string inProgressDirectoryPath = Path.GetDirectoryName(inProgressFilePath);
            //Directory.Delete(inProgressDirectoryPath, true); //true to remove subdirectories and files
            //commented the above lines that delete inProgress Directory because the filepaths in MemoryCache are processed parallely in threads, therefore even though
            //one thread may have completed the process, another thread may still need the inProgress Directory.
        }

        private void ProcessTextFile(string inProgressFilePath)
        {
            WriteLine($"Processing Text File {inProgressFilePath}");

            //Read in and process
        }
    }
}