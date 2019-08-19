using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Versagen.Entity
{
    public interface IEntityStore<TEntity> :IEntityStore where TEntity : IEntity
    {
        new TEntity GetEntity(VersaCommsID id);

        TEntity ToSavedNPC(TEntity entity);

        TEntity ToScenarioNPC(TEntity entity);

        TEntity ToPlayerEntity(TEntity entity);

        bool UpdatePermanentEntity(TEntity newVersion);

        new IQueryable<TEntity> Entities { get; }
    }


    public interface IEntityStore
    {
        string MakeActUponPrefix(VersaCommsID id);

        IEntity GetEntity(VersaCommsID id);

        IEntity ToSavedNPC(IEntity entity);

        IEntity ToScenarioNPC(IEntity entity);

        IEntity ToPlayerEntity(IEntity entity);

        bool UpdatePermanentEntity(IEntity newVersion);

        IQueryable<IEntity> Entities { get; }
    }
}
