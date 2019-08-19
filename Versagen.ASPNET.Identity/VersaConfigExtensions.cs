using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Versagen.Data;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;

namespace Versagen.ASPNET.Identity
{
    public static class VersaConfigExtensions
    {
        public class VersagenIdentityConfigIntermediate
        {
            internal IdentityBuilder IdentityBuilder;
            public VersagenServiceConfig VersagenConfig;

            public VersagenServiceConfig ExtraIdentityOptions(Action<IdentityBuilder> builder)
            {
                builder.Invoke(IdentityBuilder);
                return VersagenConfig;
            }

            internal VersagenIdentityConfigIntermediate(){}
        }

        /// <summary>
        /// TODO: Integrate Versagen's own auth system into this.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static VersagenIdentityConfigIntermediate AddIdentityPlayer<TUser,TRole>(this VersagenServiceConfig collection) 
            where TUser:IdentityUser,IPlayer
            where TRole:IdentityRole
        {
            VersaUserManager<TUser> FetchFunc(IServiceProvider p) => p.GetRequiredService<VersaUserManager<TUser>>();

            var idBuild = collection.VersagenServices.AddIdentity<TUser, TRole>()
                .AddUserManager<VersaUserManager<TUser>>();

            //TODO: Test if actually necessary to do all this or not.
            collection.VersagenServices.AddTransient<IAuthTranslator<string>>(FetchFunc)
                .AddTransient<IPlayerTranslator<string, TUser>>(FetchFunc)
                .AddTransient<IPlayerTranslator<TUser, string, TUser>>(FetchFunc)
                .AddTransient<IPlayerStore>(FetchFunc)
                .AddTransient<IPlayerStore<TUser>>(FetchFunc);

            var retVal = new VersagenIdentityConfigIntermediate
            {
                IdentityBuilder = idBuild, VersagenConfig = collection
            };
            return retVal;
        }
    }
}
