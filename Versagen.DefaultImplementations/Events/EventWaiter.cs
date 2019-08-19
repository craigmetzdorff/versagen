using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Versagen.Events.Commands;

namespace Versagen.Events
{
    public class EventWaiter<C, T, A, B> : IEventWaiter
        where C : CommandManager<A>
        where T : IVersaCommand<A>
        where A : ICommandContext
        where B : ICommandContextBuilder<A>
    {
        IEventQueue<C,T,A,B> EventsStuff { get; }
        ManualResetEvent ResetEvent { get; }
        CancellationTokenSource _internalSource { get; }
        private List<CancellationTokenSource> otherSources { get; }
        TaskCompletionSource<bool> _asyncInternalSource { get; set; }

        private readonly Semaphore _manageAsyncRace = new Semaphore(1, 1);

        public (IEvent @event, bool foundCommand, bool commandSuccessful, string failureReason) WaitAndRunEvent(CancellationToken token = default)
        {
            var eventStuff = EventsStuff.DoEvent();
            if (eventStuff.@event != default)
                return eventStuff;
            WaitForEvent(token);
            return !token.IsCancellationRequested ? EventsStuff.DoEvent() : (null, false, false, default);
        }

        public async Task<(IEvent @event, bool foundCommand, bool commandSuccessful, string failureReason)> WaitAndRunEventAsync(CancellationToken token = default)
        {
            var eventStuff = await EventsStuff.DoEventAsync();
            if (eventStuff.@event != default)
                return eventStuff;
            await WaitForEventAsync(token);
            if (!token.IsCancellationRequested)
                return await EventsStuff.DoEventAsync();
            return (null, false, false, default);
        }

        public bool WaitForEvent(CancellationToken token = default)
        {
            if (EventsStuff.HasEvent())
                return true;
            ShouldRemove = false;
            EventsStuff.Subscribe(this);
            if (!EventsStuff.HasEvent())
                WaitHandle.WaitAny(new[] { _internalSource.Token.WaitHandle, token.WaitHandle, ResetEvent });
            ResetEvent.Reset();
            ShouldRemove = true;
            return EventsStuff.HasEvent();
        }

        public bool WaitForEvent(TimeSpan span)
        {
            if (EventsStuff.HasEvent())
                return true;
            ShouldRemove = false;
            EventsStuff.Subscribe(this);
            if (!EventsStuff.HasEvent())
                WaitHandle.WaitAny(new[] { _internalSource.Token.WaitHandle, ResetEvent }, span);
            ResetEvent.Reset();
            ShouldRemove = true;
            return EventsStuff.HasEvent();
        }

        public Task<bool> WaitForEventAsync(CancellationToken token = default)
        {
            Task<bool> getFinalTask(CancellationToken inToken)
            {
                return Task.WhenAny(_asyncInternalSource.Task, Task.Delay(-1, inToken))
                    // ReSharper disable once MethodSupportsCancellation
                    .ContinueWith(_ =>
                    {
                        _asyncInternalSource?.TrySetCanceled();
                        ShouldRemove = true;
                        return EventsStuff.HasEvent();
                    });
            }

            if (EventsStuff.HasEvent()) return Task.FromResult(true);
            _manageAsyncRace.WaitOne();
            if (_asyncInternalSource != null)
            {
                var tok = CancellationTokenSource.CreateLinkedTokenSource(
                    System.Linq.Enumerable.Last(otherSources).Token, token);
                otherSources.Add(tok);
                _manageAsyncRace.Release();
                return getFinalTask(tok.Token);
            }
            _asyncInternalSource = new TaskCompletionSource<bool>();
            _manageAsyncRace.Release();
            ShouldRemove = false;
            EventsStuff.Subscribe(this);
            var source = CancellationTokenSource.CreateLinkedTokenSource(
                _internalSource.Token, token);
            otherSources.Add(source);
            if (!EventsStuff.HasEvent())
                return getFinalTask(source.Token);
            _asyncInternalSource.SetResult(true);
            otherSources.ForEach(o => o.Dispose());
            return Task.FromResult(true);
        }
        
        public Task<bool> WaitForEventAsync(TimeSpan span)
        {
            Task<bool> getFinalTask()
            {
                return Task.WhenAny(_asyncInternalSource.Task, Task.Delay(span))
                    .ContinueWith(_ =>
                    {
                        _asyncInternalSource?.TrySetCanceled();
                        ShouldRemove = true;
                        return EventsStuff.HasEvent();
                    }, _internalSource.Token);
            }
            if (EventsStuff.HasEvent()) return Task.FromResult(true);
            _manageAsyncRace.WaitOne();
            if (_asyncInternalSource != null)
            {
                _manageAsyncRace.Release();
                return getFinalTask();
            }
            _asyncInternalSource = new TaskCompletionSource<bool>();
            _manageAsyncRace.Release();
            ShouldRemove = false;
            EventsStuff.Subscribe(this);
            if (!EventsStuff.HasEvent())
                return getFinalTask();
            _asyncInternalSource.SetResult(true);
            otherSources.ForEach(o => o.Dispose());
            return Task.FromResult(true);
        }

        void IObserver<IEvent>.OnCompleted()
        {
            _internalSource.Cancel();
            Dispose();
        }

        void IObserver<IEvent>.OnError(Exception error)
        {
            throw error;
        }

        void IObserver<IEvent>.OnNext(IEvent value)
        {
            OnNextAsync(value).Wait();
        }

        private bool _innerShouldRemove;
        public bool ShouldRemove{
            get => _innerShouldRemove || disposedvalue;
            private set => _innerShouldRemove = value;
        }

        public EEventObserverExecStage ExecuteStage => EEventObserverExecStage.NewEvent;

        public Task OnNextAsync(IEvent next, EEventObserverExecStage execStage = default)
        {
            ResetEvent.Set();
            _asyncInternalSource?.SetResult(true);
            _asyncInternalSource = null;
            otherSources.ForEach(o => o.Dispose());
            otherSources.Clear();
            ShouldRemove = true;
            return Task.CompletedTask;
        }

        public EventWaiter(IEventQueue<C, T, A, B> theQueue)
        {
            this.EventsStuff = theQueue;
            ResetEvent = new ManualResetEvent(false);
            _internalSource = new CancellationTokenSource();
            otherSources = new List<CancellationTokenSource>();
        }

        private bool disposedvalue = false;
        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedvalue)
            {
                if (disposing)
                {
                    _internalSource.Cancel();
                    otherSources.ForEach(o => o.Dispose());
                    otherSources.Clear();
                    _internalSource.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedvalue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventWaiter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
