using System;
using System.IO;
using Versagen.Entity;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.World;

namespace Versagen.Events.Commands
{
    public class CommandContext : ICommandContext
    {
        public class Builder : ICommandContextBuilder<CommandContext>
        {
            public IVersaCommand<CommandContext> Command { get; set; }
            public string CommandString { get; set; }
            public string Message { get; set; }
            public Stream[] Attachments { get; set; }
            public IEventPipe Pipe { get; set; }
            public ulong ScenarioID { get; set; }
            public IPlayer User { get; set; }
            public IWorldTree WorldData { get; set; }
            public string MessageWithoutCommand { get; set; }
            public IVersaWriter OriginTerm { get; set; }
            public IVersaWriter UserTerm { get; set; }
            public IVersaWriter GMTerm { get; set; }
            public bool InitialAsyncState { get; set; }
            public Action SignalAsyncOkay { get; set; }
            public IEntity ActingEntity { get; set; }

            public CommandContext Build() => new CommandContext(this);
        }

        private IVersaCommand<CommandContext> Command { get; }
        public IEntity ActingEntity { get; }
        public ulong ScenarioID { get; }
        public IPlayer User { get; }
        public IWorldTree WorldData { get; }
        public IVersaWriter OriginTerm { get; }
        public IVersaWriter UserTerm { get; }
        Action AllowEventAsyncAction { get; }
        public bool NewEventsAllowed { get; }
        public void AllowNewEvents() => AllowEventAsyncAction();
        public IVersaWriter GMTerm { get; }
        public string Message { get; }
        public IEventPipe Pipe { get; }
        public Stream[] Attachments { get; }
        public string MessageRemainder { get; }

        protected CommandContext(ICommandContextBuilder<CommandContext> b)
        {
            Message = b.Message;
            MessageRemainder = b.Message.Substring(b.CommandString.Length).TrimStart();
            Command = b.Command;
            OriginTerm = b.OriginTerm;
            GMTerm = b.GMTerm;
            UserTerm = b.UserTerm;
            User = b.User;
            AllowEventAsyncAction = b.SignalAsyncOkay;
            NewEventsAllowed = b.InitialAsyncState;
            ActingEntity = b.ActingEntity;
            Pipe = b.Pipe;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CommandContext() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
