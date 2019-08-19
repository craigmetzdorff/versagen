using System;
using System.IO;
using Versagen.Entity;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.World;

namespace Versagen.Events.Commands
{
    public interface ICommandContextBuilder<T> where T:ICommandContext
    {
        IEventPipe Pipe { get; set; }
        ulong ScenarioID { get; set; }
        IPlayer User { get; set; }
        Stream[] Attachments { get; set; }
        IVersaCommand<T> Command { get; set; }
        string CommandString { get; set; }
        string Message { get; set; }
        string MessageWithoutCommand { get; set; }
        IVersaWriter OriginTerm { get; set; }
        IVersaWriter UserTerm { get; set; }
        IVersaWriter GMTerm { get; set; }
        bool InitialAsyncState { get; set; }
        Action SignalAsyncOkay { get; set; }
        IEntity ActingEntity { get; set; }
        T Build();
    }
}