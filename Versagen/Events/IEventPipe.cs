using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using Versagen.XML.Schemas;

namespace Versagen.Events
{
    /// <summary>
    /// An observable pipe to look for events to handle.
    /// </summary>
    public interface IEventPipe : ICancelable
    {
        /// <summary>
        /// These will run for events that need processing, before all other events. These should be low-overhead and will be executed sequentially.
        /// </summary>
        IDisposable SubscribePreprocess(IObserver<IEvent, IEvent> preprocessor);
        /// <summary>
        /// These will run at all times regardless of whether any writers are running.
        /// </summary>
        IObservable<IEvent> ObserveFirst { get; }
        IObservable<IEvent> ObserveConcurrent { get; }
        IObservable<IEvent> ObserveSynchronous { get; }

        void Complete();

        Task RunExclusiveOperation(Action action);

        Task RunExclusiveOperation(Action action, CancellationToken token);


        CancellationToken MainToken { get; }

        AsyncLock ExclusionLocker { get; }

        /// <summary>
        /// A <see cref="TaskScheduler"/> that will, when used for a task, cause the <see cref="ObserveConcurrent"/> observer to stop running tasks in its pool until the Write operation completes.
        /// </summary>
        TaskScheduler ExclusiveScheduler { get; }

        /// <summary>
        /// Intended to only be called from within another event handler.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="doPreprocess"></param>
        /// <returns></returns>
        Task ChainEvent(IEvent evnt, bool doPreprocess = false);

        /// <summary>
        /// Process an event on this queue.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="keepAliveMachine"></param>
        /// <remarks>Those making handlers for the IEvent system should take care to ensure that they acknowledge the IKeepEventAlive system, as without its correct implementation calls may be unawaitable or incompleteable.</remarks>
        /// <returns>An awaitable object that concludes after event resources are no longer required.</returns>
        Task ProcessEvent(IEvent evnt);
    }
}
