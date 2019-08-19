using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Versagen.Data;
using Versagen.PlayerSystem;

namespace Versagen.ASPNET.Identity
{
    public partial class VersaUserManager<TVersaIdentity, TKey>
    {
        public virtual Task<VersaCommsID> GetIdentifier(TKey externalUser)
            => (Users.Where(u => u.Id.Equals(externalUser)).Select(u => u.VersaID)).ToAsyncEnumerable()
                .SingleOrDefault();

        public Task<TKey> GetExternalID(VersaCommsID identifier) => Users
            .Where(user => user.VersaID == identifier)
            .Select(user => user.Id)
            .ToAsyncEnumerable().SingleOrDefault();

        /// <summary>
        /// This still needs to be here, since other methods of authentication may not have this luxury.
        /// </summary>
        /// <param name="externalUser"></param>
        /// <returns></returns>
        public Task<IPlayer> TranslateExternalUserAsync(TVersaIdentity externalUser)
            => Task.FromResult<IPlayer>(externalUser);

        public virtual Task<IPlayer> TranslateExternalUserAsync(TKey id)
            => (Users.Where(u => u.Id.Equals(id))).OfType<IPlayer>().ToAsyncEnumerable().SingleOrDefault();

        public virtual Task<TVersaIdentity> TranslatePlayerAsync(IPlayer player)
        {
            if (player is TVersaIdentity realType)
                return Task.FromResult(realType);
            return TranslatePlayerAsync(player.VersaID);
        }

        IPlayer IPlayerStore.GetPlayer(VersaCommsID id)
        {
            return GetPlayer(id);
        }

        Task<TVersaIdentity> IPlayerStore<TVersaIdentity>.GetPlayerAsync(VersaCommsID id)
        {
            return TranslatePlayerAsync(id);
        }

        IQueryable<TVersaIdentity> IPlayerStore<TVersaIdentity>.Players => Users;

        async Task<IPlayer> IPlayerStore.GetPlayerAsync(VersaCommsID id)
        {
            return await TranslatePlayerAsync(id);
        }

        public Task<(bool success, VersaCommsID ID, string FailureReason)> UpdatePlayerAsync(IPlayer user)
        {
            return TranslatePlayerAsync(user).ContinueWith(t => UpdateAsync(t.Result)).ContinueWith((t) =>
            {
                return (success: t.Result.Result.Succeeded,
                    ID: user.VersaID,
                    FailureReason: (t.Result.Result.Succeeded
                        ? ""
                        : t.Result.Result.Errors.Select(c => $"{c.Code}: {c.Description}")
                            .Aggregate((sum, next) => sum + next)));
            });
        }

        IQueryable<IPlayer> IPlayerStore.Players => Users;
    }
}
