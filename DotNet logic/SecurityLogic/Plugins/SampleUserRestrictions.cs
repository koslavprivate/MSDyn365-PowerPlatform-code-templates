using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.SecurityLogic.Services;

namespace UDS.SecurityLogic.Plugins
{
    public class SampleUserRestrictions : BasePlugin
    {
        public SampleUserRestrictions()
        {
            StepManager
                .NewStep()
                    .EntityName("sample_entity")
                    .Message("Update")
                    .Stage(PluginStage.PreOperation)
                    .PluginAction(ExecuteAction)
                    .RequiredPreImages("PreImage")
                .Register();
            StepManager
                .NewStep()
                    .EntityName("sample_entity")
                    .Message("Delete")
                    .Stage(PluginStage.PreOperation)
                    .PluginAction(ExecuteAction)
                    .RequiredPreImages("PreImage")
                .Register();
        }

        protected void ExecuteAction(LocalPluginContext context)
        {
            if (!context.PluginExecutionContext.PreEntityImages.Contains("PreImage"))
                return;
            var preImage = context.PluginExecutionContext.PreEntityImages["PreImage"];
            try
            {
                if (context.PluginExecutionContext.InputParameters["Target"] is Entity)
                    new SampleEntityService(context.OrganizationService)
                        .SampleUserUpdateRestrictions(preImage, context.PluginExecutionContext.InitiatingUserId);
                else
                    new SampleEntityService(context.OrganizationService)
                        .SampleUserDeleteRestrictions(preImage, context.PluginExecutionContext.InitiatingUserId);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"An error occurred in SecurityLogic.SampleUserRestrictions.\n{ex.Message}");
            }
        }
    }
}
