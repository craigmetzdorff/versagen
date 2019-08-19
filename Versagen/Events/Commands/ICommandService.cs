using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Events.Commands
{
    /// <summary>
    /// Will separately handle execution of commands, ensuring that other command awaiters are not signaled if an event has been performed.
    /// </summary>
    public interface ICommandService :IDisposable, IObservable<CommandRunStateEventArgs>, IObserver<IEvent>, IEnumerable<ICommandGroup>
    {
        IEnumerable<ICommandGroup> Groups { get; }
        IEnumerable<IVersaCommand> AllCommands { get; }
        string GlobalPrefix { get; }
        /// <summary>
        /// For group-less commands.
        /// </summary>
        ICommandGroup DefaultGroup { get; }
        IDisposable AddCommandGroup(ICommandGroup group);
        bool RemoveCommandGroup(ICommandGroup group);
        bool TryFindCommand(IMessageEvent e, out IVersaCommand command, out string matchedCommandText);
    }
}
