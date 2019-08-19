using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Versagen.Events.Commands;
using Versagen.Scenarios;
using Versagen.XML.Schemas;

namespace Versagen
{
    /// <summary>
    /// The main configuration class for setting up Versagen. Contains many helper methods for setting up many different types of services that Versagen expects.
    /// </summary>
    /// <remarks>
    /// Does not modify the initial collection until either one of the finish methods or <see cref="Dispose"/> is called.
    /// Throws errors for some, but not all, elements that need to be added, if they're missing when configuration occurs.
    /// Libraries adding their own Versagen classes should add extension methods to this class instead of <see cref="IServiceCollection"/> directly.
    /// </remarks>
    public class VersagenServiceConfig :IDisposable
    {

        private List<IVersaCommand> defaultCommands { get; } = new List<IVersaCommand>();

        internal class PostConfigStore
        {
            private Action<IServiceProvider> _postConfigAction;
            public bool PostBuildFunctionsWereRun { get; set; }
            public void RunPostServiceProviderConfigFunctions(IServiceProvider provider)
            {
                _postConfigAction(provider);
                PostBuildFunctionsWereRun = true;
            }

            public Action<IScenario, IServiceProvider> ScenarioConfigurator { get; }

            public PostConfigStore(Action<IServiceProvider> postbuildGeneralConfigFunc, Action<IScenario, IServiceProvider> scenarioConfigurator)
            {
                _postConfigAction = postbuildGeneralConfigFunc;
                ScenarioConfigurator = scenarioConfigurator;
            }
        }

        private bool _configDone;
        private bool _scenarioServiceManagerSet;
        private bool _upToDatePostBuildActionRetrieved;
        private readonly IServiceCollection _initialServiceCollection;
        public IEnumerable<ServiceDescriptor> ExternalServices => _initialServiceCollection;

        private readonly List<Action<IServiceProvider>> _postBuildActions;

        private Action<IScenario, IServiceProvider> _scenarioConfigAction = (scenario, provider) => { };

        /// <summary>
        /// If true, will automatically add a <see cref="PostConfigStore"/> implementation to hold post-build configuration options for the application.
        /// </summary>
        public bool AddPostBuildServiceOnDispose { get; set; }

        /// <summary>
        /// Services configured by this system. Separate from the original collection until <see cref="FinishConfig"/> is called.
        /// </summary>
        public IServiceCollection VersagenServices { get; private set; }

        public IServiceCollection ScenarioServices { get; private set; }

        public VersagenServiceConfig(IServiceCollection collection)
        {
            _initialServiceCollection = collection;
            VersagenServices = new ServiceCollection();
            ScenarioServices = new ServiceCollection();
            _postBuildActions = new List<Action<IServiceProvider>>();
        }

        /// <summary>
        /// TODO: Add version taking custom delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addScenarioServicesAction">Func taking general service provider and collection of scenario services, returning the instance.</param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")] //Because we want it to be implemented as this interface without fail, so we specify it explicitly.
        public VersagenServiceConfig ConfigureScenarioServiceScopeFactory<T>(ServiceLifetime lifetime = ServiceLifetime.Transient) where T : class,IScenarioScopeFactory
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    VersagenServices.AddSingleton<IScenarioScopeFactory>(p =>
                        ActivatorUtilities.CreateInstance<T>(p).SetScenarioServices(ScenarioServices));
                    break;
                case ServiceLifetime.Scoped:
                    VersagenServices.AddScoped<IScenarioScopeFactory>(p =>
                        ActivatorUtilities.CreateInstance<T>(p).SetScenarioServices(ScenarioServices));
                    break;
                case ServiceLifetime.Transient:
                    VersagenServices.AddTransient<IScenarioScopeFactory>(p =>
                        ActivatorUtilities.CreateInstance<T>(p).SetScenarioServices(ScenarioServices));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, default);
            }
            _scenarioServiceManagerSet = true;
            return this;
        }

        public VersagenServiceConfig SetupCommandService<T>(string globalPrefix,
            ServiceLifetime lifetime = ServiceLifetime.Transient) where T : class, ICommandService
        {
            T generatorFunc(IServiceProvider p)
            {
                var gened = ActivatorUtilities.CreateInstance<T>(p, globalPrefix);
                defaultCommands.ForEach(c => gened.DefaultGroup.TryAdd(c));
                return gened;
            }
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    ScenarioServices.AddSingleton<ICommandService>(generatorFunc);
                    break;
                case ServiceLifetime.Scoped:
                    ScenarioServices.AddScoped<ICommandService>(generatorFunc);
                    break;
                case ServiceLifetime.Transient:
                    ScenarioServices.AddTransient<ICommandService>(generatorFunc);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
            return this;
        }

        public VersagenServiceConfig AddCommandsToDefaultCommandGroup(params IVersaCommand[] newCommands)
        {
            foreach (var versaCommand in newCommands)
            {
                defaultCommands.Add(versaCommand);
            }
            return this;
        }
        


        public (bool addedNew, VersagenServiceConfig config) TryConfigureScenarioServiceScopeFactory<T>(
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where T : class, IScenarioScopeFactory =>
            VersagenServices.Any(c => c.ServiceType == typeof(IScenarioScopeFactory)) ? (false, this) : (true, ConfigureScenarioServiceScopeFactory<T>(lifetime));

        /// <summary>
        /// Ensure that post-build functions are configured.
        /// </summary>
        /// <param name="provider">Provider expected to contain Versagen services.</param>
        /// <param name="configFunction">Configuration function, or none if null</param>
        /// <param name="throwOnNothingToRun"></param>
        /// <returns></returns>
        public static bool EnsurePostBuildFunctionsAreRun(IServiceProvider provider,
            Action<IServiceProvider> configFunction = null,
            bool throwOnNothingToRun = false)
        {
            if (configFunction != null)
            {
                configFunction(provider);
                return true;
            }
            var storageService = provider.GetService<PostConfigStore>();
            if (storageService == null)
            {
                if (throwOnNothingToRun)
                    throw new Exception("No post-build function found!");
                return false;
            }
            if (!storageService.PostBuildFunctionsWereRun)
                storageService.RunPostServiceProviderConfigFunctions(provider);
            return true;
        }

        /// <summary>
        /// Add a function that will run AFTER Versagen's services are added to the configuration.
        /// This requires retrieving object from the <see cref="FinishConfig"/> method or the GetPostConfigFuncAction().
        /// </summary>
        /// <param name="postConfigAction"></param>
        /// <returns></returns>
        public VersagenServiceConfig AddPostConfigurationFunction(Action<IServiceProvider> postConfigAction)
        {
            _upToDatePostBuildActionRetrieved = false;
            _postBuildActions.Add(postConfigAction);
            return this;
        }

        public VersagenServiceConfig AddScenarioConfigFunction(
            Action<IScenario, IServiceProvider> scenarioConfigAction)
        {
            _scenarioConfigAction += scenarioConfigAction;
            return this;
        }

        /// <summary>
        /// Get all the currently-configured post-build actions for Versagen. These should be run exactly once after building the base-level service container.
        /// </summary>
        /// <param name="removeServiceContainer">If true, remove any functions that would store the postconfig build actions to the service container.</param>
        /// <returns>Function to run after the service is configured</returns>
        public Action<IServiceProvider> GetPostConfigFuncAction()
        {
            if (_postBuildActions.Count == 0)
                return _ => { };
            var retAct = _postBuildActions.Aggregate((aOne, aTwo)=>aOne+aTwo); //Yes, you can add functions like this. It's weird but it works.
            _upToDatePostBuildActionRetrieved = true;
            return retAct;
        }


        /// <summary>
        /// TODO: Verify all essential services have been added here?
        /// </summary>
        /// <returns></returns>
        internal IServiceCollection FinishConfigInternal()
        {
            if (!_scenarioServiceManagerSet)
                throw new InvalidOperationException($"No object configured to implement the {nameof(IScenarioScopeFactory)} interface! If you're adding a custom implementation, use the " + nameof(TryConfigureScenarioServiceScopeFactory) + " method afterwards instead.");
            _configDone = true;
            _initialServiceCollection.Add(VersagenServices);
            return _initialServiceCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal (IServiceCollection services, Action<IServiceProvider> postRunConfigOptions) FinishConfig() => (FinishConfigInternal(), GetPostConfigFuncAction());

        public IServiceCollection FinishConfigAndAddPostConfigOptionsToServiceProvider()
        {
            var pbAction = GetPostConfigFuncAction();
            VersagenServices.AddSingleton(p => new PostConfigStore(pbAction, _scenarioConfigAction));
            return FinishConfigInternal();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (!(_upToDatePostBuildActionRetrieved || AddPostBuildServiceOnDispose))
                throw new InvalidOperationException(
                    $"There are still services that need additional functions run on them. This may have been added in another service. Please either call {nameof(FinishConfig)} to return the action you must run as part of a tuple, or {nameof(FinishConfigAndAddPostConfigOptionsToServiceProvider)} to add a service to the collection to retrieve these functions from later. Alternatively, you can set {AddPostBuildServiceOnDispose} to true to do this automatically.");
            if (!_configDone)
            {
                if (AddPostBuildServiceOnDispose)
                    FinishConfigAndAddPostConfigOptionsToServiceProvider();
                else
                    FinishConfig();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public static class VersagenServiceConfigExtensions
    {
        public static VersagenServiceConfig AddVersagen(
            this Microsoft.Extensions.DependencyInjection.IServiceCollection collection,
            bool addPostBuildConfigServiceOnDispose = false) =>
            new VersagenServiceConfig(collection){AddPostBuildServiceOnDispose = addPostBuildConfigServiceOnDispose};
    }
}
