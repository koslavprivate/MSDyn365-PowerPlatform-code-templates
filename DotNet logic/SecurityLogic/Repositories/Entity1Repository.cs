using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDS.SecurityLogic.Repositories
{
    class Entity1Repository
    {
        private readonly IOrganizationService service;

        public Entity1Repository(IOrganizationService service)
        {
            this.service = service;
        }

        internal bool IsEntity2AlredyHasRelated(Guid entity2, Guid entity1)
        {
            var query = new QueryExpression("sample_entity1")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(false),
                Criteria = {
                    FilterOperator = LogicalOperator.And,
                    Conditions = {
                        new ConditionExpression("sample_entity1id", ConditionOperator.NotEqual, entity1),
                        new ConditionExpression("sample_entity2id", ConditionOperator.Equal, entity2),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                }
            };

            return service.RetrieveMultiple(query).Entities.Count > 0;
        }
    }
}
