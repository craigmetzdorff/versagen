using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Versagen.Events
{
    public class TaskEventRunner<C, T, A, B> : IEventRunner
        where C : Commands.CommandManager<A>
        where T : Commands.IVersaCommand<A>
        where A : Commands.ICommandContext
        where B : Commands.ICommandContextBuilder<A>
    {
        public IEventWaiter EWaiter { get; }
        public IEventQueue<C,T,A,B> EQueue { get; }
        public bool UsesTasks => true;

        TaskScheduler _scheduler { get; } = TaskScheduler.Default;

        CancellationTokenSource _loopTokenSource { get; set; }
        
        CancellationTokenSource _internalSource { get; set; } = new CancellationTokenSource();

        ConcurrentDictionary<int, Task> RunningTasks { get; } = new ConcurrentDictionary<int, Task>();

        Task loopTask { get; set; }

        public bool isLooping { get; private set; } = false;

        Task AppendAwaitCompletion(Task theTask)
        {
            if (!theTask.IsCompleted)
            {
                var updatedTask = theTask.ContinueWith(t => RunningTasks.Remove(t.Id, out _));
                return RunningTasks.AddOrUpdate(theTask.Id, updatedTask, (_,t)=> updatedTask);
            }
            return theTask;
        }

        public Task AwaitAllTasks(CancellationToken token = default)
        {
            var oneTask = Task.WhenAll(RunningTasks.Values.ToArray());
            return Task.WhenAny(oneTask, Task.Delay(-1, token));
        }

        public Task AwaitAllTasks(TimeSpan span)
        {
            var oneTask = Task.WhenAll(RunningTasks.Values.ToArray());
            return Task.WhenAny(oneTask, Task.Delay(span));
        }

        public void StartEventLoop(CancellationToken token = default)
        {
            if (isLooping)
                return;
            isLooping = true;
            _loopTokenSource?.Dispose();
            _loopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _internalSource.Token);
            var loopToken = _loopTokenSource.Token;
            loopTask = Task.Run(() =>
            {
                while (isLooping)
                {
                    if (EWaiter.WaitForEvent(loopToken))
                    {
                        try
                        {
                            var returned = EQueue.StartEvent();
                            AppendAwaitCompletion(returned.CommandTask);
                        }
                        //TODO: DO SOMETHING WITH THIS!
                        catch { }
                    }
                }
            });
        }

        /// <summary>
        /// Return a linked CancelToken?
        /// </summary>
        /// <param name="token"></param>
        public void StopEventLoop(CancellationToken token = default)
        {
            isLooping = false;
            var clearOnDone = AwaitAllTasks().ContinueWith(_ => RunningTasks.Clear());
            RunningTasks.GetOrAdd(clearOnDone.Id, clearOnDone);
        }

        public void StopEventLoop(TimeSpan span)
        {
            isLooping = false;
        }

        public TaskEventRunner(IEventWaiter waiter, IEventQueue<C,T,A,B> queue)
        {
            this.EWaiter = waiter;
            this.EQueue = queue;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _internalSource.Cancel();
                    _internalSource.Dispose();
                    _loopTokenSource?.Dispose();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventRunner() {
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
