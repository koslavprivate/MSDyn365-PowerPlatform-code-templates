using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.SecurityLogic.Repositories;

namespace UDS.SecurityLogic.Services
{
    class SampleEntityService
    {
        private readonly IOrganizationService service;
        private readonly UserRepository userRepository;

        public SampleEntityService(IOrganizationService service)
        {
            this.service = service;
            userRepository = new UserRepository(service);
        }

        internal void SampleUserUpdateRestrictions(Entity preImage, Guid userId)
        {
            bool isAdmin = userRepository.UserHasRole(userId, "System Administrator");
            if (isAdmin)
                return;
            bool isSampleUser = userRepository.UserHasRole(userId, "Sample User");
            if (!isSampleUser)
                throw new InvalidPluginExecutionException("You don't have permissions to update this record\n");
        }

        internal void SampleUserDeleteRestrictions(Entity preImage, Guid userId)
        {
            bool isAdmin = userRepository.UserHasRole(userId, "System Administrator");
            if (isAdmin)
                return;
            bool isSampleUser = userRepository.UserHasRole(userId, "Sample User");
            if (isSampleUser)
                throw new InvalidPluginExecutionException("You don't have permissions to delete this record\n");
        }
    }
}
