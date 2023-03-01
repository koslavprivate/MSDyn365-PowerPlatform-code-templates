using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDS.ImportFromFileConsoleSample.Services
{
    public class ImportFromFileConsoleSampleService
    {
        private readonly IOrganizationService service;

        public ImportFromFileConsoleSampleService(IOrganizationService service)
        {
            this.service = service;
        }

        internal void ImportData(List<string[]> data)
        {
            foreach (string[] dataRow in data)
            {
                var record = new Entity("sample_entity");
                record["field1"] = data[0];
                record["field2"] = data[1];
                service.Create(record);
            }
        }
    }
}
