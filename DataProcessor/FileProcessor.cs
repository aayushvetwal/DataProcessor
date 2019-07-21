using static System.Console;

namespace DataProcessor
{
    internal class FileProcessor
    {
        public string InputFilePath { get; }

        public FileProcessor(string filePath)
        {
            this.InputFilePath = filePath;
        }

        public void Process()
        {
            WriteLine($"Begin Process of {InputFilePath}");
        }
    }
}