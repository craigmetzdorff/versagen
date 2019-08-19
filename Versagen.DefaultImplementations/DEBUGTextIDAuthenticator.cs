using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Versagen.PlayerSystem;

namespace Versagen
{
    public class DEBUGTextIDAuthenticator : AuthTranslator<string>
    {
        protected ConcurrentDictionary<string, VersaCommsID> PlayerIDs { get; }

        protected ConcurrentDictionary<VersaCommsID, string> OuterTranslation { get; }

        //TODO: This is probably bad practice and should require something else be in place, or be auto-generated as a scoped variable.
        public override Task<VersaCommsID> GetIdentifier(string externalUser)
        =>
            Task.FromResult(PlayerIDs.GetOrAdd(externalUser, _ => CreateNewUser(externalUser)));

        public override Task<string> GetExternalID(VersaCommsID identifier)
        {
            if (OuterTranslation.TryGetValue(identifier, out var retID))
                return Task.FromResult(retID);
            return Task.FromResult("");
        }

        protected override VersaCommsID CreateNewUser(string externalUser)
        {
            var newID = base.CreateNewUser(externalUser);
            OuterTranslation.TryAdd(newID, externalUser);
            return newID;
        }

        public DEBUGTextIDAuthenticator()
        {
            PlayerIDs = new ConcurrentDictionary<string, VersaCommsID>();
            OuterTranslation = new ConcurrentDictionary<VersaCommsID, string>();
        }



    }
}
