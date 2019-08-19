using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Versagen.Items;
using Versagen.PlayerSystem;

namespace Versagen.ASPNET.Identity
{
    public class IdentityPlayer : IdentityUser, IPlayer
    {
        public VersaCommsID VersaID { get; internal set; }
        public EVersaPerms Permissions { get; set; }
        public IDictionary<int, int> Currencies { get; set; }
        public IList<VersaCommsID> OwnedCharacters { get; set; }
        /// <summary>
        /// TODO: Reconsider for the future? Might be too difficult for this bit. Also might even be unnecessary.
        /// </summary>
        public List<IItem> PlayerInventory { get; set; }
        public IdentityPlayer()
        {
            VersaID = VersaCommsID.FromEnum(EVersaCommIDType.User, 0);
            Currencies = new Dictionary<int, int>();
            OwnedCharacters = new List<VersaCommsID>();
            PlayerInventory = new List<IItem>();
        }


        public IdentityPlayer(VersaCommsID newID) : this()
        {
            if (newID.IdType != EVersaCommIDType.User)
                throw new ArgumentOutOfRangeException(nameof(newID), "Object was not provided a User ID value.");
        }
    }
    public class IdentityPlayer<TKey> : Microsoft.AspNetCore.Identity.IdentityUser<TKey>, IPlayer where TKey:IEquatable<TKey>
    {
        public VersaCommsID VersaID { get; internal set; }
        public EVersaPerms Permissions { get; set; }
        public IDictionary<int, int> Currencies { get; set; }
        public IList<VersaCommsID> OwnedCharacters { get; set; }
        /// <summary>
        /// TODO: Reconsider for the future? Might be too difficult for this bit. Also might even be unnecessary.
        /// </summary>
        public List<IItem> PlayerInventory { get; set; }
        public IdentityPlayer()
        {
            Currencies = new Dictionary<int, int>();
            OwnedCharacters = new List<VersaCommsID>();
            PlayerInventory = new List<IItem>();
        }


        public IdentityPlayer(VersaCommsID newID) : this()
        {
            if (newID.IdType != EVersaCommIDType.User)
                throw new ArgumentOutOfRangeException(nameof(newID), "Object was not provided a User ID value.");
        }
    }
}
