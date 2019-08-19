using System;
using System.Collections.Generic;
using System.Text;
using Versagen.Items;

// Class for player attributes, including attack, defense, speed and experience points
namespace Versagen.PlayerSystem
{
    //TODO: rid Versa
    [Obsolete("Use the IPlayer interface instead.")]
    public class Player : IPlayer
    {
        public VersaCommsID VersaID { get; }
        public EVersaPerms Permissions { get; set; }
        public IDictionary<int, int> Currencies { get; set; }
        public IList<VersaCommsID> OwnedCharacters { get; set; }

        public Player(VersaCommsID ID)
        {
            this.VersaID = ID;
        }
      
        /* IAuthorizer is injected (Dependency injection)
      public int level = 1;
      public int toLevelUp = 10;
      public int exp = 0;
      public int attack = 5;
      public int defense = 3;
      public int speed = 10; */

        public List<IItem> PlayerInventory { get; set; }
        public string UserName { get; set; }

        public List<int> BasePlayerStats => throw new NotImplementedException();

        public bool setIDIfUnset(VersaCommsID id)
        {
            throw new NotImplementedException();
        }
    }
}
