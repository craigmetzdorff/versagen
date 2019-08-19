using System.Collections.Concurrent;
using System.Collections.Generic;
using Versagen.Events;
using Versagen.PlayerSystem;

namespace Versagen.World
{
    /// <summary>
    /// Manages the different sections of the world. This DOES include the actual "World" instance.
    /// </summary>
    public interface IScenarioManager
    {
        ConcurrentDictionary<ulong, IWorldTree> Scenarios { get; }
        IWorldTree WorldInstance { get; }
        /// <summary>
        /// TODO: change these params to something that's actually relevant.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IWorldTree CreateScenario(IEvent context);
        IList<IPlayer> Players { get; }
        IList<ulong> GameMasters { get; }
    }
}
