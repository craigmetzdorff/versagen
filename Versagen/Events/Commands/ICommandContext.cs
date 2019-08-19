using System;
using System.IO;
using System.Threading.Tasks;
using Versagen.Entity;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.World;

namespace Versagen.Events.Commands
{

    public interface ICommandContext : IDisposable
    {
        IEventPipe Pipe { get; }
        Stream[] Attachments { get; }
        string Message { get; }
        string MessageRemainder { get; }
        IPlayer User { get; }
        IEntity ActingEntity { get; }
        ulong ScenarioID { get; }
        IVersaWriter OriginTerm { get; }
        IVersaWriter GMTerm { get; }
        IVersaWriter UserTerm { get; }
        /// <summary>
        /// Tell the queue that new events can now be started. This can be called to allow an EventQueue to allow more work to get done while a function cleans itself up.
        /// All IEventQueue implementations should be built to ensure that not calling this in a method will not cause function blocking.
        /// </summary>
        void AllowNewEvents();
    }
}