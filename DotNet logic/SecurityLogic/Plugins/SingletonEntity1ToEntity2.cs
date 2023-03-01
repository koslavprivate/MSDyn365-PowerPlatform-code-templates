using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.SecurityLogic.Services;

namespace UDS.SecurityLogic.Plugins
{
    public class SingletonEntity1ToEntity2 : BasePlugin
    {
        public SingletonEntity1ToEntity2()
        {
            StepManager
               .NewStep()
                   .Message("Create")
                   .EntityName("sample_entity1")
                   .Stage(PluginStage.PreValidation)
                   .PluginAction(ExecuteAction)
               .Register();

            StepManager
                .NewStep()
                    .Message("Update")
                    .EntityName("sample_entity1")
                    .Stage(PluginStage.PreValidation)
                    .PluginAction(ExecuteAction)
                .Register();
        }

        protected void ExecuteAction(LocalPluginContext context)
        {
            if (!context.PluginExecutionContext.InputParameters.Contains("Target") || !(context.PluginExecutionContext.InputParameters["Target"] is Entity))
                return;
            var target = (Entity)context.PluginExecutionContext.InputParameters["Target"];

            try
            {
                new Entity1Service(context.OrganizationService)
                    .SingletonEntity1ToEntity2(target);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"An error occurred in SecurityLogic.SingletonEntity1ToEntity2.\n{ex.Message}");
            }
        }
    }
}
