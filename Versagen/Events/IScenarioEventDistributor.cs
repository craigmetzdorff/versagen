using System;
using System.Linq;
using System.Threading.Tasks;
using Versagen.Events.Commands;
using Versagen.Scenarios;

namespace Versagen.Events
{
    public interface IScenarioEventDistributor<in TE> where TE:IEvent
    {
        IQueryable<IScenario> Scenarios { get; }
        IScenario CreateScenario(IServiceProvider provider, VersaCommsID gameMasterID, VersaCommsID[] partyIDs);
        IScenario BuildWorldScenario(IServiceProvider provider);
        Task ProcessEvent(TE @event);
    }
}
