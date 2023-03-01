using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.SecurityLogic.Repositories;

namespace UDS.SecurityLogic.Services
{
    class Entity1Service
    {
        private readonly IOrganizationService service;
        private readonly Entity1Repository entity1Repository;

        public Entity1Service(IOrganizationService service)
        {
            this.service = service;
            entity1Repository = new Entity1Repository(service);
        }

        internal void SingletonEntity1ToEntity2(Entity target)
        {
            var entity2 = target.GetAttributeValue<EntityReference>("sample_entity2id");
            if (entity2 == null)
                return;

            if (entity1Repository.IsEntity2AlredyHasRelated(entity2.Id, target.Id))
            {
                throw new InvalidPluginExecutionException("Other Entity1 include this Entity2! Please, change Entity2!");
            }
        }
    }
}
