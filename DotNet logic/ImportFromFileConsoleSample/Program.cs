using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.ImportFromFileConsoleSample.Helpers;
using UDS.ImportFromFileConsoleSample.Services;

namespace UDS.ImportFromFileConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {

            var service = ConnectionHelper.InitializeService();
            var importFromFileConsoleSampleService = new ImportFromFileConsoleSampleService(service);
            Console.Write("File path: ");
            string filePath = Console.ReadLine();
            Console.Write("Separator: ");
            char separator = Console.ReadLine()[0];
            var data = FileHelper.GetDataFromFile(filePath, separator);
            try
            {
                importFromFileConsoleSampleService.ImportData(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }
}
