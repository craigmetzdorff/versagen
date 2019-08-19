using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Versagen.Events;

namespace Versagen.Scenarios
{
    public class VersaScenarioDistributor : IScenarioEventDistributor<MessageEvent>
    {
        
        public ConcurrentDictionary<VersaCommsID,Scenario> _Scenarios { get; }

        public IQueryable<IScenario> Scenarios => _Scenarios.Values.AsQueryable();

        public IScenario CreateScenario(IServiceProvider provider, VersaCommsID gameMasterID, VersaCommsID[] partyIDs)
        {
            throw new NotImplementedException();
        }

        public IScenario BuildWorldScenario(IServiceProvider provider)
        {
            return _Scenarios.GetOrAdd(VersaCommsID.FromEnum(EVersaCommIDType.Scenario, 0), new Scenario(VersaCommsID.FromEnum(EVersaCommIDType.Scenario, 0), provider));
        }

        public Task ProcessEvent(MessageEvent messageEvent)
            =>
                _Scenarios[messageEvent.GetScenarioID()].ProcessEvent(messageEvent);

        public VersaScenarioDistributor(IServiceProvider provider)
        {
            _Scenarios = new ConcurrentDictionary<VersaCommsID, Scenario>();
            BuildWorldScenario(provider);
        }
    }
}
