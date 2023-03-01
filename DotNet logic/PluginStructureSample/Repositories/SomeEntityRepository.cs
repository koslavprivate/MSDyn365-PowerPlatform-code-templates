using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDS.PluginStructureSample.Repositories
{
    class SomeEntityRepository
    {
        private readonly IOrganizationService service;

        public SomeEntityRepository(IOrganizationService service)
        {
            this.service = service;
        }

        internal List<Entity> GetBySampleId(Guid sampleId, ColumnSet columnSet)
        {
            var query = new QueryExpression()
            {
                NoLock = true,
                EntityName = "sample_someentity",
                ColumnSet = new ColumnSet(false),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("sample_entityid", ConditionOperator.Equal, sampleId)
                    }
                }
            };

            return service.RetrieveMultiple(query).Entities?.ToList();
        }
    }
}
