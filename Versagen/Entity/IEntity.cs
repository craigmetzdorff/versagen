using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Versagen.Events.Actions;
using Versagen.Events.Commands;

namespace Versagen.Entity
{
    public interface IEntity : IDescribable, IMergeable<IEntity>
    {
        VersaCommsID Id { get; }
        string Name { get; set; }
        IDictionary<string, IStat> Stats { get; }
        VersaCommsID ActiveScenarioID { get; }
        ILocation CurrentLocation { get; }
        bool AddOwner(VersaCommsID id);
        IQueryable<VersaCommsID> Owners { get; }
        bool RemoveOwner(VersaCommsID id);
        IReaction[] Reactions { get; }
        ICommandGroup ActAsCommands { get; }
        IList<IVersaCommand> ActUponCommands { get; }
    }
}
