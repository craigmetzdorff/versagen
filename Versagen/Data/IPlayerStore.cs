using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Versagen.Entity;
using Versagen.PlayerSystem;

namespace Versagen.Data
{
    public interface IPlayerStore<T> : IPlayerStore where T:IPlayer
    {
        new T GetPlayer(VersaCommsID id);
        new Task<T> GetPlayerAsync(VersaCommsID id);
        Task<(bool success, VersaCommsID ID, string FailureReason)> UpdatePlayerAsync(T user);
        //TODO: Implement this elsewhere instead, on a similar player system.
        new IQueryable<T> Players { get; }
    }


    public interface IPlayerStore
    {
        IPlayer GetPlayer(VersaCommsID id);
        Task<IPlayer> GetPlayerAsync(VersaCommsID id);
        Task<(bool success, VersaCommsID ID, string FailureReason)> UpdatePlayerAsync(IPlayer user);
        //TODO: Implement this elsewhere instead, on a similar player system.
        //Task<IQueryable<IEntity>> GetAssignedEntities(IPlayer user);
        IQueryable<IPlayer> Players { get; }
    }
}
