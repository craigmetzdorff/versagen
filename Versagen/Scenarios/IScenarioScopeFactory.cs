using Microsoft.Extensions.DependencyInjection;

namespace Versagen.Scenarios
{
    public interface IScenarioScopeFactory
    {
        IScenarioScopeFactory SetScenarioServices(IServiceCollection collection);
        IScenarioScope ConfigureScenarioServices(VersaCommsID id);
    }
}
