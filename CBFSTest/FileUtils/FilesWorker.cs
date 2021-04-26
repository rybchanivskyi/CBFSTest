using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBFSTest.FileUtils
{
    public class FilesWorker
    {
        private string _filesPath;

        public FilesWorker()
        {
            _filesPath = @"E:\FilesTest";
        }

        public string GenerateFileContent(int contentLength)
        {
            if(contentLength < 0)
            {
                return string.Empty;
            }
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789\n";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, contentLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public List<string> CreateFileNames(uint numberOfFiles)
        {
            var result = new List<string>();
            for(int i = 1; i <= numberOfFiles; ++i)
            {
                result.Add($"TestFile{i}.txt");
            }
            return result;
        }

        public void CreateFiles(List<string> fileNames, string fileContent)
        {
            foreach(var name in fileNames)
            {
                try
                {
                    using (FileStream fs = File.Create($@"{_filesPath}/{name}"))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes(fileContent);
                        fs.Write(info, 0, info.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Create file Error : {ex.Message}. Name - {name}");
                }
            }
        }

        public void ReadFromFiles(List<string> fileNames)
        {
            foreach (var name in fileNames)
            {
                try
                {
                    File.ReadAllText($@"{_filesPath}/{name}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Read file Error : {ex.Message}. Name - {name}");
                }
            }
        }

        public void EnumerateDirectory()
        {
            Directory.EnumerateFiles(_filesPath);
        }

        public void DeleteFiles(List<string> fileNames)
        {
            foreach (var name in fileNames)
            {
                try
                {
                    File.Delete($@"{_filesPath}/{name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Delete file Error : {ex.Message}. Name - {name}");
                }
            }
        }
    }
}
