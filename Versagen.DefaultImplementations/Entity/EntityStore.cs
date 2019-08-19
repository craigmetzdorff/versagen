using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Versagen.Utils;

namespace Versagen.Entity
{
    public class EntityStore : IEntityStore<Entity>
    {
        internal class DefaultEntityStorePermanentBacking
        {
            public List<Entity> PermanentEntities { get; }

            public DefaultEntityStorePermanentBacking()
            {
                PermanentEntities = new List<Entity>();
            }
        }

        List<Entity> ScenarioEntities { get; }
        List<Entity> PermanentEntities { get; }

        //public IEntity GetEntity(VersaCommsID id) => ScenarioEntities.FirstOrDefault(c => c.Id == id) ?? PermanentEntities.First(c => c.Id == id);

        //public IEntity ToSavedNPC(IEntity entity)
        //{
        //    PermanentEntities.Add(entity);
        //    return entity;
        //}

        //public IEntity ToScenarioNPC(IEntity entity)
        //{
        //    ScenarioEntities.Add(entity);
        //    return entity;
        //}

        //public IPlayerEntity ToPlayerEntity(IEntity entity, params VersaCommsID[] owners)
        //{
        //    var jobj = JObject.FromObject(entity);
        //    jobj.Add(nameof(IPlayerEntity.Owners), new JArray(owners));
        //    return jobj.ToObject<PlayerEntity>();
        //}

        //public bool UpdatePermanentEntity(IEntity newVersion)
        //{
        //    var entity = PermanentEntities.FirstOrDefault(c => c.Id == newVersion.Id);
        //    if (entity == null) return false;
        //    PermanentEntities[PermanentEntities.IndexOf(entity)] = newVersion;
        //    return true;
        //}

        //public IQueryable<IEntity> Entities =>
        //    ScenarioEntities.Union(PermanentEntities, new IDComparer<IEntity>(e => e.Id)).AsQueryable();


        public string MakeActUponPrefix(VersaCommsID id) => GetEntity(id).Name;

        public Entity GetEntity(VersaCommsID id) => ScenarioEntities?.FirstOrDefault(c => c.Id == id) ?? PermanentEntities.First(c => c.Id == id);

        IEntity IEntityStore.ToSavedNPC(IEntity entity)
        {
            if (entity is Entity tentity)
                return ToSavedNPC(tentity);
            throw new ArgumentException("Entity type not supported.", nameof(entity));
        }

        IEntity IEntityStore.ToScenarioNPC(IEntity entity)
        {
            if (entity is Entity tentity)
                return ToScenarioNPC(tentity);
            throw new ArgumentException("Entity type not supported.", nameof(entity));
        }

        IEntity IEntityStore.ToPlayerEntity(IEntity entity)
        {
            if (entity is Entity tentity)
                return ToPlayerEntity(tentity);
            throw new ArgumentException("Entity type not supported.", nameof(entity));
        }

        public bool UpdatePermanentEntity(IEntity newVersion)
        {
            if (newVersion is Entity tentity)
                return UpdatePermanentEntity(tentity);
            throw new ArgumentException("Entity type not supported.", nameof(newVersion));
        }

        IEntity IEntityStore.GetEntity(VersaCommsID id)
        {
            return GetEntity(id);
        }

        IQueryable<IEntity> IEntityStore.Entities => Entities;

        public IQueryable<Entity> Entities => ScenarioEntities?.AsQueryable().Union(PermanentEntities) ?? PermanentEntities.AsQueryable();

        public Entity ToSavedNPC(Entity entity)
        {
            PermanentEntities.Add(entity);
            return entity;
        }

        public Entity ToScenarioNPC(Entity entity)
        {
                ScenarioEntities.Add(entity);
                return entity;
        }

        public Entity ToPlayerEntity(Entity entity)
        {
            if (!entity.Owners.Any())
                throw new System.IndexOutOfRangeException("At least one owner is required.");
            //var jobj = JObject.FromObject(entity);
            //if (entity.Id.IdType != EVersaCommIDType.PlayerCharacter || Entities.Any(c => c.Id == entity.Id))
            //    jobj[nameof(Entity.Id)].Replace(new JValue(VersaCommsID.RandomOutsideRange(EVersaCommIDType.PlayerCharacter, Entities.Select(e => e.Id))));
            //var newPlayer = jobj.ToObject<Entity>();
            if (PermanentEntities.Contains(entity))
                return entity;
            PermanentEntities.Add(entity);
            return entity;
        }

        public bool UpdatePermanentEntity(Entity newVersion)
        {
            var entity = PermanentEntities.FirstOrDefault(c => c.Id == newVersion.Id);
            if (entity == null) return false;
            PermanentEntities[PermanentEntities.IndexOf(entity)] = newVersion;
            return true;
        }


        public EntityStore(List<Entity> backing, bool inScenario)
        {
            PermanentEntities = backing;
            if (inScenario)
                ScenarioEntities = new List<Entity>();
        }
    }
}
