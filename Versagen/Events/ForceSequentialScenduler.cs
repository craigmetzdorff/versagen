using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Versagen.Events
{
    /// <summary>
    /// Ensures that, no matter what, only one Event is running at a time.
    /// </summary>
    [Obsolete("Did not perform as expected. Not a viable scheduler.", true)]
    public class SingleActionScheduler : LocalScheduler, ICancelable
    {
        private SchedulerQueue<TimeSpan> _sQueue;

        private object EntryGate = new object();
        private object FinalGate = new object();

        private SemaphoreSlim slimGate = new SemaphoreSlim(1,1);

        private Task DeQueueLoop;

        AsyncManualResetEvent manEv = new AsyncManualResetEvent();

        private TaskScheduler ExclusiveScheduler;

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var si = new ScheduledItem<TimeSpan, TState>(Scheduler.Immediate, state, action, dueTime);
            lock (EntryGate)
            {
                _sQueue.Enqueue(si);
            }
            manEv.Set();
            return si;
        }

        //public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
        //{

        //    var si = new ScheduledItem<TimeSpan,TState>(Scheduler.Immediate, state, )
        //}

        private async Task DeQueueAll()
        {
            while (_sQueue.Count > 0)
            {
                ScheduledItem<TimeSpan> item;
                lock (EntryGate)
                {
                    item = _sQueue.Dequeue();
                }
                await slimGate.WaitAsync();
                using (Disposable.Create(() =>slimGate.Release()))
                {
                    if (item.IsCanceled) continue;
                    lock (FinalGate)
                        item.Invoke();
                }
            }
        }

        private Task DequeueLoopRunner() => new Task(async () =>
        {
            try
            {
                while (!IsDisposed)
                {
                    await DeQueueAll();
                    await manEv.WaitAsync();
                    manEv.Reset();
                }
            }
            finally
            {
                //If exception causes loop to crash, make another that can get awaited.
                if (!IsDisposed)
                    DeQueueLoop = DequeueLoopRunner();
                DeQueueLoop.Start(ExclusiveScheduler);
            }
        });

        public SingleActionScheduler(TaskScheduler exclusiveScheduler = null)
        {
            if (exclusiveScheduler == null)
                exclusiveScheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            ExclusiveScheduler = exclusiveScheduler;
            _sQueue = new SchedulerQueue<TimeSpan>();
            DeQueueLoop = DequeueLoopRunner();
            DeQueueLoop.Start(ExclusiveScheduler);
        }

        public void Dispose()
        {
            DeQueueLoop?.Dispose();
            manEv?.Dispose();
            IsDisposed = true;
            DeQueueAll().Wait();
        }

        public bool IsDisposed { get; private set; }
    }
}
