using System;
using System.Collections.Generic;
using Versagen.Utils;
using Versagen.Entity;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Scenarios;
using Versagen.XML.Schemas;

namespace Versagen.Events
{
    /// <summary>
    /// The most basic form of Versagen communication, carrying info about where it originated and the ServiceProvider scoped to that instance.
    /// It's recommended to extend this class and add extra pattern matching clauses when hooking onto IEventPipe objects.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// If true, at the end of the observer's scope, no future Observers will be triggered.
        /// </summary>
        bool IgnoreThis { get; set; }
        bool IsSystemMessage { get; set; }
        IEventPipe SourcePipe { get; set; }
        IServiceProvider Services { get; set; }
        UnionType<VersaCommsID, IScenario> Scenario { get; set; }
        UnionType<VersaCommsID, IVersaWriter> Terminal { get; set; }

        UnionType<VersaCommsID, IPlayer> Player { get; set; }
        UnionType<VersaCommsID, IEntity> Entity { get; set; }
        IList<ICommandGroup> EventSpecificCommands { get; }

    }

    public static class EventHelpers
    {
        /// <summary>
        /// Retrieves the player object.
        /// </summary>
        /// <typeparam name="TPlayer"></typeparam>
        /// <param name="evnt"></param>
        /// <param name="GetPlayerFunc"></param>
        /// <returns></returns>
        /// TODO: use C# 8.0 switch case handling!
        public static TPlayer GetPlayer<TPlayer>(this IEvent evnt, Func<VersaCommsID, TPlayer> GetPlayerFunc) where TPlayer:IPlayer
        {
            switch (evnt.Player.Obj)
            {
                case TPlayer player:
                    return player;
                case VersaCommsID id:
                    var newPlayer = GetPlayerFunc(id);
                    evnt.Player = newPlayer;
                    return newPlayer;
                default:
                    return default;
            }
        }

        public static VersaCommsID GetPlayerID(this IEvent evnt)
        {
            switch (evnt.Player.Obj)
            {
                case IPlayer player: return player.VersaID;
                case VersaCommsID id: return id;
                default: return default;
            }
        }

        public static TEntity GetEntity<TEntity>(this IEvent evnt, Func<VersaCommsID, TEntity> getEntityFunc)  where TEntity:IEntity
        {
            switch (evnt.Entity.Obj)
            {
                case TEntity entity:
                    return entity;
                case VersaCommsID id:
                    var newEntity = getEntityFunc(id);
                    evnt.Entity = newEntity;
                    return newEntity;
                default:
                    return default;
            }
        }

        public static VersaCommsID GetEntityID(this IEvent evnt)
        {
            switch (evnt.Entity.Obj)
            {
                case IEntity entity: return entity.Id;
                case VersaCommsID id: return id;
                default: return default;
            }
        }

        public static TScenario GetScenario<TScenario>(this IEvent evnt,
            Func<VersaCommsID, TScenario> getScenarioFunction) where TScenario:IScenario
        {
            switch (evnt.Entity.Obj)
            {
                case TScenario entity:
                    return entity;
                case VersaCommsID id:
                    var newEntity = getScenarioFunction(id);
                    evnt.Scenario = newEntity;
                    return newEntity;
                default:
                    return default;
            }
        }

        public static VersaCommsID GetScenarioID(this IEvent evnt)
        {
            switch (evnt.Scenario.Obj)
            {
                case IScenario entity: return entity.ScenarioID;
                case VersaCommsID id: return id;
                default: return default;
            }
        }

    }


    
}
