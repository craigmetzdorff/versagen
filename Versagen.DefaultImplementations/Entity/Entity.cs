using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Versagen.Events.Actions;
using Versagen.Events.Commands;
using Versagen.Items;
using Versagen.Rules;

namespace Versagen.Entity
{
    public class GetLocationHandler: JsonConverter
    {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jobj = JObject.FromObject(value);
            jobj[nameof(Entity.CurrentLocation)].Replace(JToken.FromObject(((ILocation) value).LocationId));
            jobj.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override bool CanConvert(Type objectType) => typeof(IEntity).IsAssignableFrom(objectType);
    }


    public class Entity : IEntity, IMergeable<Entity>
    {

        public List<IItem> ItemsList { get; }

        public VersaCommsID Id { get; }
        public VersaCommsID CurrentScenario { get; set; }
        private List<VersaCommsID> _owners;
        public Entity(VersaCommsID Id, IEnumerable<IVersaCommand> initCommands = default, IReaction[] reactions = default)
        {
            this.Id = Id;
            Reactions = reactions;

            ActAsCommands = new CommandGroup("self", "/commands/self/" + Id);
            ActAsCommands.Preconditions.Add(new LambdaRule("IsEntity", "", (context, provider) => Task.FromResult((context.ActingEntity == this, "wrongEntity"))));
            initCommands.ForEach(x => ActAsCommands.TryAdd(x));
            Stats = new ConcurrentDictionary<string, IStat>();
            _owners = new List<VersaCommsID>();
        }

        public string Name { get; set; }
        public IDictionary<string, IStat> Stats { get; }
        public VersaCommsID ActiveScenarioID { get; set; }
        public ILocation CurrentLocation { get; set; }
        public VersaDescription Description { get; set; }

        //   public IQueryable<VersaCommsID> Owners => throw new NotImplementedException();

        public VersaDescription GetDefaultDescription()
        {
            return Description;
        }

        public string PrintFullDescription(ICommandContext context, IServiceProvider provider) => GetDefaultDescription().BuildDescription(context, provider);

        IEntity IDeepCloneable<IEntity>.DeepClone(bool req)
        {
            return DeepClone();
        }

        public virtual Entity DeepClone()
        {
            var retObj =
                JsonConvert.DeserializeObject<Entity>(JsonConvert.SerializeObject(this, new GetLocationHandler()));
            retObj.CurrentLocation = this.CurrentLocation;
            return retObj;
        }

        public Entity Union(Entity other)
        {
            throw new NotImplementedException();
        }

        public void Merge(Entity other)
        {
            throw new NotImplementedException();
        }

        object IDeepCloneable.DeepClone(bool req)
        {
            return DeepClone();
        }

        IEntity IMergeable<IEntity>.Union(IEntity other)
        {
            if (other is Entity e)
                return Union(e);
            throw new InvalidCastException("Entity type unsupported.");
        }

        void IMergeable<IEntity>.Merge(IEntity other)
        {
            if (other is Entity e)
                Merge(e);
            throw new InvalidCastException("Entity type unsupported.");
        }

        
        public bool AddOwner(VersaCommsID id)
        {
            _owners.Add(id);
            return true;
        }

        [JsonIgnore]
        public IQueryable<VersaCommsID> Owners => _owners.AsQueryable();
        public bool RemoveOwner(VersaCommsID id)
        {
            return _owners.Remove(id);
        }

        public IReaction[] Reactions { get; }

        public ICommandGroup ActAsCommands { get; }
        public IList<IVersaCommand> ActUponCommands { get; } = new List<IVersaCommand>();

        Entity IDeepCloneable<Entity>.DeepClone(bool requireNewRef)
        {
            throw new NotImplementedException();
        }


    }
}
