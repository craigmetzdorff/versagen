using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.Entity;
using Versagen.Events.Commands;
using Versagen.Items;

namespace Versagen
{

    public interface ILocation : IDescribable, IDeepCloneable<ILocation>, IEquatable<ILocation>
    {
        bool TryAddEntity(IEntity ent);
        bool TryRemoveEntity(IEntity ent);
        ILocation Parent { get; }
        VersaCommsID LocationId { get; }
        string Name { get; }
        string PathName { get; }
        void Add(ILocation location);
        void Remove(ILocation location);
        IQueryable<ILocation> ChildLocations { get; }
        IQueryable<ILocation> ConnectedLocations { get; }
        IQueryable<IEntity> ContainedEntities { get; }
        IQueryable<IItem> ContainedItems { get; }
        IEnumerable<IVersaCommand> GetCommandsFor(IEntity entity);
        ICommandGroup LocalizedCommands { get; }
        string CurrentPath { get; }
        ILocation PathBrowse(string relativePath);
        ILocation Root { get; }
    }

}
