using System;
using Microsoft.Extensions.DependencyInjection;

namespace Versagen.Scenarios
{
    public interface IScenarioScope :IDisposable
    {
        VersaCommsID ScenarioID { get; }
        ScenarioServiceProvider GetScenarioServiceProvider(IServiceProvider baseProvider);
    }

    /// <summary>
    /// To be only used by the <see cref="IScenarioScopeFactory"/> to manage services for scenarios.
    /// </summary>
    internal class ScenarioScope :IScenarioScope
    {
        public VersaCommsID ScenarioID { get; }
        protected IServiceProvider ScenarioProviderUnscoped { get; set; }

        public ScenarioServiceProvider GetScenarioServiceProvider(IServiceProvider baseProvider) =>
            new ScenarioServiceProvider(ScenarioID, baseProvider, ScenarioProviderUnscoped);

        public ScenarioScope(VersaCommsID scenarioID, IServiceProvider scenarioProvider)
        {
            this.ScenarioID = scenarioID;
            ScenarioProviderUnscoped = scenarioProvider;
        }

        public bool IsDisposed { get; protected set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (IsDisposed) return;
            IsDisposed = true;
            foreach (var item in ScenarioProviderUnscoped.GetServices<IDisposable>())
                item.Dispose();
            ScenarioProviderUnscoped = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
