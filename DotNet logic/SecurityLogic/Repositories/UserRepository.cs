using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDS.SecurityLogic.Repositories
{
    class UserRepository
    {
        private readonly IOrganizationService service;

        public UserRepository(IOrganizationService service)
        {
            this.service = service;
        }

        internal bool UserHasRole(Guid userId, string roleName)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "systemuserroles",
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity("systemuserroles", "role", "roleid", "roleid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new  ConditionExpression("name", ConditionOperator.Equal, roleName)
                            }
                        }
                    }
                }
            };

            var results = service.RetrieveMultiple(query);
            return results.Entities.Count > 0;
        }
    }
}
