using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.PluginStructureSample.Repositories;

namespace UDS.PluginStructureSample.Services
{
    class SampleService
    {
        private readonly IOrganizationService service;
        private readonly SomeEntityRepository someEntityRepository;

        public SampleService(IOrganizationService service)
        {
            this.service = service;
            someEntityRepository = new SomeEntityRepository(service);
        }

        internal void SampleFunction(Entity target, Entity preImage)
        {
            var someEntities = someEntityRepository.GetBySampleId(target.Id, new ColumnSet("sample_someenityfield1", "sample_someenityfield2"));
            //do some logic
        }
    }
}
