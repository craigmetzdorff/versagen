using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Versagen.PlayerSystem
{
    public interface IPlayerTranslator<TExternalId, TExternalUser> : IAuthTranslator<TExternalId>
    {
        Task<IPlayer> TranslateExternalUserAsync(TExternalUser externalUser);
        Task<IPlayer> TranslateExternalUserAsync(TExternalId id);
        Task<TExternalUser> TranslatePlayerAsync(IPlayer player);
        Task<TExternalUser> TranslatePlayerAsync(VersaCommsID id);
    }

    public interface IPlayerTranslator<TInternalUser, TExternalId, TExternalUser> : IPlayerTranslator<TExternalId, TExternalUser> where TInternalUser : IPlayer
    {
        Task<TInternalUser> TranslatePlayerAsync(TExternalUser externalUser);
        Task<TInternalUser> TranslatePlayerAsync(TExternalId id);
    }

}
