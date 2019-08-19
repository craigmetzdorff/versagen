using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Versagen.PlayerSystem;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Data;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;

namespace Versagen.ASPNET.Identity
{
    public class VersaUserManager<TVersaIdentity> : VersaUserManager<TVersaIdentity, string> where TVersaIdentity : IdentityUser, IPlayer
    {
        public VersaUserManager(IUserStore<TVersaIdentity> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TVersaIdentity> passwordHasher, IEnumerable<IUserValidator<TVersaIdentity>> userValidators, IEnumerable<IPasswordValidator<TVersaIdentity>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TVersaIdentity>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<TVersaIdentity> TranslatePlayerAsync(string id)
            => Store.FindByIdAsync(id, CancellationToken);
        public override Task<IPlayer> TranslateExternalUserAsync(string id)
            => Store.FindByIdAsync(id, CancellationToken).ContinueWith(t => t.Result as IPlayer);

        public override Task<VersaCommsID> GetIdentifier(string externalUser)
            => Store.FindByIdAsync(externalUser, CancellationToken).ContinueWith(t => t.Result.VersaID);
    }

    public partial class VersaUserManager<TVersaIdentity, TKey> : UserManager<TVersaIdentity>,
        IPlayerTranslator<TVersaIdentity, TKey, TVersaIdentity>,
        IPlayerStore<TVersaIdentity> where TVersaIdentity : Microsoft.AspNetCore.Identity.IdentityUser<TKey>, IPlayer
        where TKey:IEquatable<TKey>
    {
        private IServiceProvider _services;
        protected VersaCommsID RandomID()
        {
            var rand = new Random();
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            ulong resul = BitConverter.ToUInt64(buf, 0);
            return VersaCommsID.FromEnum(EVersaCommIDType.User, resul);
        }

        /// <summary>
        /// DANGEROUS but necessary since we don't want people to normally write to this.
        /// Copied a project and made it internal just so that people wouldn't go out of their way to use this. Figure out a way to not need this ASAP.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected TVersaIdentity ForceChangeID(TVersaIdentity user, VersaCommsID target)
        {
            Mono.Reflection.BackingFieldResolver.GetBackingField(user.GetType().GetProperty("VersaID"))
                .SetValue(user, target);
            return user;
        }

        protected virtual TVersaIdentity SetVersaID(TVersaIdentity user)
        {
            var newID = Users.Max(x => x.VersaID) + 1;
            if (user is IdentityPlayer<TKey> idPlayer)
            {
                idPlayer.VersaID = newID;
                user = idPlayer as TVersaIdentity;
            }
            else
                //AVOID GETTING TO THIS POINT, BUT IT'S HERE IF NEEDED
                user = ForceChangeID(user, newID);
            
            return user;
        }

        public override async Task<IdentityResult> CreateAsync(TVersaIdentity user, string password)
        {
            
            var result = await base.CreateAsync(
                SetVersaID(
                    user
                )
                , password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                _services.GetRequiredService<ILogger<VersaUserManager>>().LogError("Creating user failed!");
            }
            //if (!result.Succeeded) return result;
            //if (Users.Count(u => u.VersaID == user.VersaID) < 2) //Extremely inefficient but highly unlikely to fail.
            //    return result;
            //while (Users.Count(u => u.VersaID == user.VersaID) > 1)
            //    user = SetVersaID(user);
            //return await UpdateAsync(user);
            return result;
        }

        protected VersaUserManager(IUserStore<TVersaIdentity> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TVersaIdentity> passwordHasher, IEnumerable<IUserValidator<TVersaIdentity>> userValidators,
            IEnumerable<IPasswordValidator<TVersaIdentity>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TVersaIdentity>> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
                services, logger)
        {
            _services = services;
        }

        public virtual Task<TVersaIdentity> TranslatePlayerAsync(VersaCommsID id)
            => (Users.Where(u => u.VersaID == id)).ToAsyncEnumerable().SingleOrDefault();

        public virtual Task<TVersaIdentity> TranslatePlayerAsync(TVersaIdentity externalUser)
            => Task.FromResult(externalUser);
        
        public virtual Task<TVersaIdentity> TranslatePlayerAsync(TKey id)
            =>(Users.Where(u => u.Id.Equals(id))).ToAsyncEnumerable().SingleOrDefault();


        public TVersaIdentity GetPlayer(VersaCommsID id)
        {
            var playerAyn = TranslatePlayerAsync(id);
            playerAyn.Wait();
            return playerAyn.Result;
        }

        public Task<(bool success, VersaCommsID ID, string FailureReason)> UpdatePlayerAsync(TVersaIdentity user)
        {
            return UpdateAsync(user).ContinueWith((t) =>
            {
                return (success: t.Result.Succeeded,
                    ID: user.VersaID,
                    FailureReason: (t.Result.Succeeded
                        ? ""
                        : t.Result.Errors.Select(c => $"{c.Code}: {c.Description}").Aggregate((sum, next)=> sum+next)));
            });
        }
    }
}
