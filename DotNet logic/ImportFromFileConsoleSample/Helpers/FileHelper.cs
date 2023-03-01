using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDS.ImportFromFileConsoleSample.Helpers
{
    public class FileHelper
    {
        public static List<string[]> GetDataFromFile(string path, char separator)
        {
            var result = new List<string[]>();
            var lines = File.ReadAllLines(path);
            foreach (string line in lines)
                result.Add(line.Split(separator));
            return result;
        }
    }
}
