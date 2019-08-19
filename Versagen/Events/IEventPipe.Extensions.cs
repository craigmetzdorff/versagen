using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Versagen.Events
{
    public static class EventPipeExtensions
    {
        /// <summary>
        /// Creates a disposable object that will lock the Queue for the duration of an event, disallowing any new tasks to be run beneath it.
        /// TODO: Is it enough to just use ExclusionLocker and be done with it? Probably.
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static IDisposable BeginExclusiveOperation(this IEventPipe pipe)
        {
            //Prevent deadlock because we are already running exclusively.
            if (TaskScheduler.Current.Id == pipe.ExclusiveScheduler.Id)
            {
                return Disposable.Empty;
            }
            void runShim(object state)
            {
                var casthadle =(Tuple<ManualResetEventSlim, CancellationToken>)state;
                casthadle.Item1.Set();
                casthadle.Item2.WaitHandle.WaitOne();
            }
            var src = new CancellationTokenSource();
            ManualResetEventSlim oneSlim = new ManualResetEventSlim();
            Tuple<ManualResetEventSlim, CancellationToken> thing = new Tuple<ManualResetEventSlim, CancellationToken>(oneSlim, src.Token);
            Task.Run(()=>
                 new Task(runShim, thing).RunSynchronously(pipe.ExclusiveScheduler), pipe.MainToken);
            //pipe.ExclusionLocker.Wait(()=> new TaskFactory(pipe.ExclusiveScheduler).StartNew(() =>
            //{
            //    oneSlim.Set();
            //    src.Token.WaitHandle.WaitOne();
            //}));
            oneSlim.Wait(pipe.MainToken);
            return Disposable.Create(src, s =>
            {
                s.Cancel(false);
                s.Dispose();
            });
        }

        public static IDisposable Lock(this TaskScheduler scheduler, CancellationToken cancelWaiToken)
        {
            //Prevent deadlock because we are already running exclusively.
            if (TaskScheduler.Current.Id == scheduler.Id)
            {
                return Disposable.Empty;
            }
            void runShim(object state)
            {
                var casthadle = (Tuple<ManualResetEventSlim, CancellationToken>)state;
                casthadle.Item1.Set();
                casthadle.Item2.WaitHandle.WaitOne();
            }
            var src = new CancellationTokenSource();
            ManualResetEventSlim oneSlim = new ManualResetEventSlim();
            Tuple<ManualResetEventSlim, CancellationToken> thing = new Tuple<ManualResetEventSlim, CancellationToken>(oneSlim, src.Token);
            Task.Run(() =>
                new Task(runShim, thing).RunSynchronously(scheduler), cancelWaiToken);
            //pipe.ExclusionLocker.Wait(()=> new TaskFactory(pipe.ExclusiveScheduler).StartNew(() =>
            //{
            //    oneSlim.Set();
            //    src.Token.WaitHandle.WaitOne();
            //}));
            oneSlim.Wait(cancelWaiToken);
            return Disposable.Create(src, s =>
            {
                s.Cancel(false);
                s.Dispose();
            });
        }

        public static async Task<IDisposable> LockAsync(this TaskScheduler scheduler, CancellationToken cancelWaiToken)
        {
            //Prevent deadlock because we are already running exclusively.
            if (TaskScheduler.Current.Id == scheduler.Id)
            {
                return Disposable.Empty;
            }
            void runShim(object state)
            {
                var casthadle = (Tuple<TaskCompletionSource<bool>, CancellationToken>)state;
                casthadle.Item1.TrySetResult(true);
                casthadle.Item2.WaitHandle.WaitOne();
            }
            var src = new CancellationTokenSource();
            var oneSlim= new TaskCompletionSource<bool>();
            var thing = new Tuple<TaskCompletionSource<bool>, CancellationToken>(oneSlim, src.Token);
            Task.Run(() =>
                new Task(runShim, thing,TaskCreationOptions.LongRunning).RunSynchronously(scheduler), cancelWaiToken);
            //pipe.ExclusionLocker.Wait(()=> new TaskFactory(pipe.ExclusiveScheduler).StartNew(() =>
            //{
            //    oneSlim.Set();
            //    src.Token.WaitHandle.WaitOne();
            //}));
            await oneSlim.Task;
            return Disposable.Create(src, s =>
            {
                s.Cancel(false);
                s.Dispose();
            });
        }
    }
}
