using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Versagen.Scenarios
{
    public class DefaultScenarioScopeFactory :IScenarioScopeFactory
    {

        private IServiceCollection ScenarioServices { get; set; } = new ServiceCollection();

        public IScenarioScopeFactory SetScenarioServices(IServiceCollection collection)
        {
            ScenarioServices = collection;
            return this;
        }

        public IScenarioScope ConfigureScenarioServices(VersaCommsID id) => new ScenarioScope(id, ScenarioServices.BuildServiceProvider());
    }
}
