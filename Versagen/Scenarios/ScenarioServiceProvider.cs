using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Versagen.Scenarios
{
    public class ScenarioServiceProvider :IServiceProvider,IDisposable
    {
        public class DefaultScopedServicesFactory:IServiceScopeFactory
        {
            public class DefaultScenarioServicesScope :IServiceScope
            {
                private IServiceScope baseScope;

                private IServiceScope scenarioScope;


                public void Dispose()
                {
                    baseScope.Dispose();
                    scenarioScope.Dispose();
                }

                public IServiceProvider ServiceProvider { get; }

                public DefaultScenarioServicesScope(VersaCommsID id, IServiceScope baseScope, IServiceScope scenarioScope)
                {
                    this.baseScope = baseScope;
                    this.scenarioScope = scenarioScope;
                    ServiceProvider = new ScenarioServiceProvider(id, baseScope.ServiceProvider,scenarioScope.ServiceProvider);
                }
            }

            private IServiceProvider baseProvider;
            private IServiceProvider scenarioProvider;
            private VersaCommsID id;

            public IServiceScope CreateScope() => new DefaultScenarioServicesScope(id,baseProvider.CreateScope(), scenarioProvider.CreateScope());

            public DefaultScopedServicesFactory(VersaCommsID id, IServiceProvider baseProvider,
                IServiceProvider scenarioProvider)
            {
                this.baseProvider = baseProvider;
                this.scenarioProvider = scenarioProvider;
                this.id = id;
            }

        }

        public VersaCommsID ScenarioID { get; }
        protected IServiceProvider BaseProvider { get; }
        protected IServiceProvider ScenarioProvider { get; }

        /// <inheritdoc />
        /// <summary>
        /// Note: as a result of how this is called, this will only create scoped services for the scenario service provider.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType ||
                serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>)) //To support GetServices.
                return serviceType == typeof(IServiceScopeFactory)
                    ? new DefaultScopedServicesFactory(ScenarioID, BaseProvider, ScenarioProvider)
                    : ScenarioProvider.GetService(serviceType) ?? BaseProvider.GetService(serviceType);

            var temphold = ((IEnumerable<object>) ScenarioProvider.GetService(serviceType));
            var baseOut = (IEnumerable<object>) BaseProvider.GetService(serviceType);
            return baseOut == null ? temphold : temphold.Concat(baseOut);

        }

        internal ScenarioServiceProvider(VersaCommsID scenarioID, IServiceProvider baseProvider, IServiceProvider scenarioScope, params object[] instanceObjects)
        {
            ScenarioID = scenarioID;
            this.BaseProvider = baseProvider;
            this.ScenarioProvider = scenarioScope;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
