using System;
using System.Threading.Tasks;

namespace Versagen.PlayerSystem
{
    //TODO: Type for external user and external user ID instead probably
    public interface IAuthTranslator<TExternalId>
    {
        /// <summary>
        /// Get the internal User ID to return.
        /// </summary>
        /// <param name="externalUser"></param>
        /// <returns></returns>
        Task<VersaCommsID> GetIdentifier(TExternalId externalUser);

        Task<TExternalId> GetExternalID(VersaCommsID identifier);
    }



    public abstract class AuthTranslator<TExternalID> : IAuthTranslator<TExternalID>
    {
        /// <summary>
        /// Get the internal User ID to return.
        /// </summary>
        /// <param name="externalUser"></param>
        /// <returns></returns>

        public abstract Task<VersaCommsID> GetIdentifier(TExternalID externalUser);

        public abstract Task<TExternalID> GetExternalID(VersaCommsID identifier);


        protected virtual VersaCommsID CreateNewUser(TExternalID externalUser)
        {
            //TODO: fire off other functions needed to create a new user, like creating tables for them in the database and such.
            var rand = new Random();
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            ulong resul = BitConverter.ToUInt64(buf, 0);
            return VersaCommsID.FromEnum(EVersaCommIDType.User, resul);
        }
    }
}
