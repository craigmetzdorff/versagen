using System.Buffers.Text;
using System.Collections.Generic;
using Versagen.Entity;
using Versagen.Items;
using Versagen.PlayerSystem;

namespace Versagen.Data
{
    public class Player : stats
    {
        public int[] BasePlayerStats = playerStats;
        

        ulong ID { get; set; }

    }
}
