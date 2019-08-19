using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.Entity;
using Versagen.Events.Commands;
using Versagen.Items;
using Newtonsoft.Json;
using Versagen.Utils;

namespace Versagen.Locations
{
    public class Location : ILocation, IDeepCloneable<Location>
    {
        //TODO: change to Dictionary type to ensure that we can assign conditionals for movement.
        List<ILocation> _ChildLocations { get; }

        public List<IEntity> _containedEntities { get; }

        List<IItem> _Items { get; }
        public ICommandGroup Commands { get; }
        public bool TryAddEntity(IEntity ent)
        {
            _containedEntities.Add(ent);
            return true;
        }

        public bool TryRemoveEntity(IEntity ent) => _containedEntities.Remove(ent);

        public ILocation Parent { get; }

        private Lazy<PathBrowser<ILocation>> pathFinder = new Lazy<PathBrowser<ILocation>>(() =>
            new PathBrowser<ILocation>(l => l.Parent, l => l.ChildLocations, l => l.Name, l => l.Root));

        public Location()
        {
            _ChildLocations = new List<ILocation>();
            _Items = new List<IItem>();
            _containedEntities = new List<IEntity>();
            LocalizedCommands = new CommandGroup("","/locations/");
        }

        public VersaCommsID LocationId { get; }
        public string Name { get; set; }
        public string PathName { get; set; }
        public void Add(ILocation location) => _ChildLocations.Add(location);
        public void Remove(ILocation location) => _ChildLocations.Remove(location);
        public IQueryable<ILocation> ChildLocations => _ChildLocations.AsQueryable();
        public IQueryable<ILocation> ConnectedLocations => throw new NotImplementedException();
        public VersaDescription Description { get; set; } = new VersaDescription();
        public VersaDescription GetDefaultDescription() =>
            ContainedEntities.Any()
                ? Description + "\n" + ContainedEntities.Select(c => c.Description).Aggregate(new StringBuilder(),
                      (stringBuilder, entityDescription) =>
                          stringBuilder.AppendFormat("A creature: {0}", entityDescription).AppendLine()).ToString()
                : Description;
        public string PrintFullDescription(ICommandContext context, IServiceProvider provider) => GetDefaultDescription().BuildDescription(context, provider);
        public IQueryable<IEntity> ContainedEntities => _containedEntities.AsQueryable();
        public IQueryable<IItem> ContainedItems => _Items.AsQueryable();
        public IEnumerable<IVersaCommand> GetCommandsFor(IEntity entity) => _containedEntities.Where(e => e.Id != entity.Id).SelectMany(e => e.ActUponCommands).Concat(LocalizedCommands);

        public ICommandGroup LocalizedCommands { get; }

        public string CurrentPath => pathFinder.Value.GetPath(this);

        public ILocation PathBrowse(string path) => pathFinder.Value.BrowseTo(this, path);

        public ILocation Root { get; set; }

        ILocation IDeepCloneable<ILocation>.DeepClone(bool requireNewRef)
        {
            return DeepClone(requireNewRef);
        }

        public Location DeepClone(bool requireNewRef = false)
        {
            var clonedLoc = (Location) MemberwiseClone();
            clonedLoc._ChildLocations.DeepCloneInPlace();
            clonedLoc._Items.SelectiveDeepCloneInPlace();
            clonedLoc._containedEntities.DeepCloneInPlace();
            return clonedLoc;
        }

        object IDeepCloneable.DeepClone(bool requireNewRef)
        {
            return DeepClone(requireNewRef);
        }

        //ILocation IMergeable<ILocation>.Union(ILocation other)
        //{
        //    if (other is Location l)
        //        return Union(l);
        //    throw new InvalidCastException("Location type not compatible.");
        //}

        //void IMergeable<ILocation>.Merge(ILocation other)
        //{
        //    if (other is Location l)
        //        Merge(l);
        //    throw new InvalidCastException("Location type not compatible.");
        //}

        public Location(VersaCommsID locationID) :this()
        {
            LocationId = locationID;            
        }

        public bool Equals(ILocation other)
        {
            return LocationId.Equals(other.LocationId);
        }
    }
}
