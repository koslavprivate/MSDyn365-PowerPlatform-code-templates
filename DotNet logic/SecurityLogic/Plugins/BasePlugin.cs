using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace UDS.SecurityLogic.Plugins
{
    public abstract class BasePlugin : IPlugin
    {
        protected class LocalPluginContext
        {
            internal IServiceProvider ServiceProvider { get; private set; }

            internal IOrganizationService OrganizationService { get; private set; }

            internal IPluginExecutionContext PluginExecutionContext { get; private set; }

            internal ITracingService TracingService { get; private set; }

            internal IOrganizationService SystemOrganizationService { get; private set; }

            internal IOrganizationService InitUserOrganizationService { get; private set; }

            internal IOrganizationServiceFactory ServiceFactory { get; private set; }

            private LocalPluginContext() { }

            internal LocalPluginContext(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                    throw new ArgumentNullException("serviceProvider");

                // Obtain the execution context service from the service provider.
                this.PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                // Obtain the tracing service from the service provider.
                this.TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                // Obtain the Organization Service factory service from the service provider
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                ServiceFactory = factory;
                // Use the factory to generate the Organization Service.
                this.OrganizationService = factory.CreateOrganizationService(this.PluginExecutionContext.UserId);
                this.SystemOrganizationService = factory.CreateOrganizationService(null);
                this.InitUserOrganizationService = factory.CreateOrganizationService(this.PluginExecutionContext.InitiatingUserId);
            }

            internal void Trace(string message)
            {
                if (string.IsNullOrWhiteSpace(message) || this.TracingService == null)
                    return;

                if (this.PluginExecutionContext == null)
                {
                    this.TracingService.Trace(message);
                }
                else
                {
                    this.TracingService.Trace(
                        "{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        this.PluginExecutionContext.CorrelationId,
                        this.PluginExecutionContext.InitiatingUserId);
                }
            }
        }

        protected class PluginStepManager
        {
            private List<PluginStep> _pluginSteps;

            public PluginStepManager()
            {
                _pluginSteps = new List<PluginStep>();
            }

            public void Add(PluginStep step)
            {
                if (step != null)
                {
                    _pluginSteps.Add(step);
                }
            }

            public PluginStepBuilder NewStep()
            {
                return new PluginStepBuilder(this);
            }

            public IEnumerable<Action<LocalPluginContext>> GetStepEvents(string message, string entityName, PluginStage stage,
                IEnumerable<string> preImages,
                IEnumerable<string> postImages)
            {
                return _pluginSteps
                    .Where(s =>
                    {
                        return
                            (s.Message == null || s.Message == message) &&
                            (s.EntityName == null || s.EntityName == entityName) &&
                            ((int)s.Stage == 0 || s.Stage == stage) &&
                            (s.RequiredPreImages.Count == 0 ||
                                (preImages != null && !s.RequiredPreImages.Except(preImages).Any())) &&
                            (s.RequiredPostImages.Count == 0 ||
                                (postImages != null && !s.RequiredPostImages.Except(postImages).Any()));
                    }
                     )
                    .Select(s => s.PluginAction);
            }
        }

        protected class PluginStepBuilder
        {
            private PluginStep _pluginStep;
            private PluginStepManager _stepManager;

            public PluginStepBuilder(PluginStepManager manager)
            {
                _pluginStep = new PluginStep();
                _stepManager = manager;
            }

            public PluginStepBuilder PluginAction(Action<LocalPluginContext> pluginAction)
            {
                _pluginStep.PluginAction = pluginAction;

                return this;
            }

            public PluginStepBuilder Message(string message)
            {
                _pluginStep.Message = message;

                return this;
            }

            public PluginStepBuilder EntityName(string entityName)
            {
                _pluginStep.EntityName = entityName;

                return this;
            }

            public PluginStepBuilder Stage(PluginStage stage)
            {
                _pluginStep.Stage = stage;

                return this;
            }

            public PluginStepBuilder RequiredPreImages(params string[] images)
            {
                if (images != null)
                {
                    foreach (string image in images)
                    {
                        if (!String.IsNullOrEmpty(image) && !_pluginStep.RequiredPreImages.Contains(image))
                        {
                            _pluginStep.RequiredPreImages.Add(image);
                        }
                    }
                }

                return this;
            }

            public PluginStepBuilder RequiredPostImages(params string[] images)
            {
                if (images != null)
                {
                    foreach (string image in images)
                    {
                        if (!String.IsNullOrEmpty(image) && !_pluginStep.RequiredPostImages.Contains(image))
                        {
                            _pluginStep.RequiredPostImages.Add(image);
                        }
                    }
                }

                return this;
            }

            public void Register()
            {
                _stepManager.Add(_pluginStep);
            }
        }

        protected class PluginStep
        {
            public PluginStep()
            {
                RequiredPreImages = new List<string>();
                RequiredPostImages = new List<string>();
            }

            public Action<LocalPluginContext> PluginAction { get; set; }
            public string Message { get; set; }
            public string EntityName { get; set; }
            public PluginStage Stage { get; set; }
            public List<string> RequiredPreImages { get; private set; }
            public List<string> RequiredPostImages { get; private set; }
        }

        public BasePlugin()
        {
            StepManager = new PluginStepManager();
        }

        protected PluginStepManager StepManager { get; private set; }

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances. 
        /// The plug-in's Execute method should be written to be stateless as the constructor 
        /// is not called for every invocation of the plug-in. Also, multiple system threads 
        /// could execute the plug-in at the same time. All per invocation state information 
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            LocalPluginContext localcontext = new LocalPluginContext(serviceProvider);

            localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", this.GetType()));

            try
            {
                IEnumerable<Action<LocalPluginContext>> availableActions = StepManager
                    .GetStepEvents(localcontext.PluginExecutionContext.MessageName,
                        localcontext.PluginExecutionContext.PrimaryEntityName,
                        (PluginStage)localcontext.PluginExecutionContext.Stage,
                        localcontext.PluginExecutionContext.PreEntityImages.Keys,
                        localcontext.PluginExecutionContext.PostEntityImages.Keys);

                foreach (Action<LocalPluginContext> action in availableActions)
                {
                    if (action != null)
                    {
                        localcontext.Trace(string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} is firing for Entity: {1}, Message: {2}",
                            this.GetType(),
                            localcontext.PluginExecutionContext.PrimaryEntityName,
                            localcontext.PluginExecutionContext.MessageName));

                        action.Invoke(new LocalPluginContext(serviceProvider));
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", e));
                throw;
            }
            finally
            {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exiting {0}.Execute()", this.GetType()));
            }
        }
    }

    public enum PluginStage
    {
        /// <summary>
        /// Stage in the pipeline for plug-ins that are to execute before the main system operation. 
        /// Plug-ins registered in this stage may execute outside the database transaction.
        /// </summary>
        PreValidation = 10,

        /// <summary>
        /// Stage in the pipeline for plug-ins that are to execute before the main system operation. 
        /// Plug-ins registered in this stage are executed within the database transaction.
        /// </summary>
        PreOperation = 20,

        /// <summary>
        /// Stage in the pipeline for plug-ins which are to execute after the main operation. 
        /// Plug-ins registered in this stage are executed within the database transaction.
        /// </summary>
        PostOperation = 40
    }
}
