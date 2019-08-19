using System.Collections.Generic;
using Versagen.Items;

namespace Versagen.PlayerSystem
{
    public interface IPlayer
    {
        VersaCommsID VersaID { get; }
        EVersaPerms Permissions { get; }
        IDictionary<int, int> Currencies { get; }
        IList<VersaCommsID> OwnedCharacters { get; }
        List<IItem> PlayerInventory { get; }
        string UserName { get; }

    }
}