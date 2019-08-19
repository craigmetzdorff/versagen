using System;
using System.Collections.Generic;
using System.Text;
using Versagen.Entity;

namespace Versagen.Events.Commands
{
    public enum ECommandState : byte
    {
        FoundCommand,
        BeforeContextConstruction,
        Preconditions,
        PostCommand
    }

    public class CommandRunStateEventArgs
    {
        public IEvent Event { get; }
        public IVersaCommand Command { get; }
        public ICommandContext Context { get; }
        public ECommandState State { get; }
        public Exception CommandException { get; }
        public bool HasException => CommandException != default;
        public CommandRunStateEventArgs(IEvent @event, IVersaCommand command, ICommandContext context,
            ECommandState state, Exception commandException = default)
        {
            Event = @event;
            Command = command;
            Context = context;
            State = state;
            CommandException = commandException;
        }
    }
}
