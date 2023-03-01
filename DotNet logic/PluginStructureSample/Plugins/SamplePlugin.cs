using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDS.PluginStructureSample.Services;

namespace UDS.PluginStructureSample.Plugins
{
    public class SamplePlugin : BasePlugin
    {
        public SamplePlugin()
        {
            StepManager
                .NewStep()
                    .EntityName("sample_entity")
                    .Message("Create")
                    .Stage(PluginStage.PreOperation)
                    .PluginAction(ExecuteAction)
                    .RequiredPreImages("PreImage")
                .Register();
            StepManager
                .NewStep()
                    .EntityName("sample_entity")
                    .Message("Update")
                    .Stage(PluginStage.PreOperation)
                    .PluginAction(ExecuteAction)
                    .RequiredPreImages("PreImage")
                .Register();
        }

        protected void ExecuteAction(LocalPluginContext context)
        {
            if (!context.PluginExecutionContext.InputParameters.Contains("Target") || !(context.PluginExecutionContext.InputParameters["Target"] is Entity))
                return;
            var target = (Entity)context.PluginExecutionContext.InputParameters["Target"];
            var preImage = context.PluginExecutionContext.PreEntityImages.Contains("PreImage")
                ? context.PluginExecutionContext.PreEntityImages["PreImage"]
                : new Entity();

            try
            {
                new SampleService(context.InitUserOrganizationService)
                    .SampleFunction(target, preImage);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"An error occurred in UDS.SamplePlugin.\n{ex.Message}");
            }
        }
    }
}
