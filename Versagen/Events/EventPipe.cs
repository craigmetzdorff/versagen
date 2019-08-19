using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Versagen.Events
{
    /// <summary>
    /// 
    /// </summary>
    public class EventPipe : IEventPipe
    {
        private readonly TaskPoolScheduler _allScheduler;
        private readonly Subject<IEvent> _allSubject;
        private readonly Subject<(IEvent @event, bool doPreProcessing)> _incomingEventSubject;
        private readonly IScheduler _initialScheduler;
        private readonly TaskPoolScheduler _readerScheduler;
        private readonly Subject<IEvent> _readSubject;
        private readonly SynchronizationContext _syncContext;
        private readonly ConcurrentExclusiveSchedulerPair _tpschedPair;
        private readonly Subject<IEvent> _writeSubject;
        private readonly TaskFactory exFactory;
        private bool _isCompleted;

        private object _gate = new object();

        private ConcurrentDictionary<IObserver<IEvent, IEvent>, IDisposable> _preProcessing =
                            new ConcurrentDictionary<IObserver<IEvent, IEvent>, IDisposable>();
        public AsyncLock ExclusionLocker { get; } = new AsyncLock();
        public bool IsDisposed { get; protected set; }

        public CancellationToken MainToken => TokenSource.Token;

        //public Task ProcessEvent(IEvent evnt, IKeepEventAlive keepEventAlive)
        //{
        //    return Task.Run(()=> actionPrompts.Aggregate((one, two) => one + two);
        public IObservable<IEvent> ObserveConcurrent { get; }

        //}
        public IObservable<IEvent> ObserveFirst { get; }

        public IObservable<IEvent> ObserveSynchronous { get; }

        //private IEnumerable<Action<IEvent>> actionPrompts =>
        //    AlwaysObservers.Concat(ReaderObservers).Concat(WriterObservers);
        public CancellationTokenSource TokenSource { get; }

        public TaskScheduler ExclusiveScheduler => _tpschedPair.ExclusiveScheduler;

        //private SingleActionScheduler _writerScheduler { get; }
        public Task ChainEvent(IEvent evnt, bool doPreprocess = false)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(EventPipe));
            //Action<IEvent> InternalOnNextMaker(ConcurrentDictionary<IObserver<IEvent>, IDisposable> col, IObserver<IEvent> obs) => @event =>
            //{
            //    if (!TokenSource.IsCancellationRequested && col.ContainsKey(obs) && !@event.IgnoreThis)
            //        obs.OnNext(@event);
            //};
            //Action<IEvent> OnNextAggregation(ConcurrentDictionary<IObserver<IEvent>, IDisposable> dict) => dict.IsEmpty ? _ =>{}: dict.Select(o => InternalOnNextMaker(dict, o.Key)).Aggregate((a, b) => a + b);
            evnt.SourcePipe = this;
            //var procE = doPreprocess && !_preProcessing.IsEmpty ? _preProcessing.Keys.Aggregate(evnt, (e, o) => o.OnNext(e)) : evnt;
            //var allActs = OnNextAggregation(_observesall) + OnNextAggregation(_observesread) + OnNextAggregation(_observeswrite);

            //Note: These do not actually cause the events to run on the task. Instead, they are DISPATCHED to the underlying scheduler asynchronously.
            
            return Task.Run(() => _incomingEventSubject.OnNext((evnt, doPreprocess)));
            //return Task.Run(() =>
            //{
            //    _allSubject.OnNext(procE);
            //    _readSubject.OnNext(procE);
            //    _writeSubject.OnNext(procE);
            //}, TokenSource.Token);
        }

        //protected void DisposeObserverMethod((ConcurrentDictionary<IObserver<IEvent>, IDisposable> coll, IObserver<IEvent> observer) inTuple)
        //{
        //    var (coll, observer) = inTuple;
        //    coll.TryRemove(observer, out _);
        //}
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// Method remains since it has potentially different effects than <see cref="M:Versagen.Events.EventPipe.ChainEvent(Versagen.Events.IEvent,System.Boolean)" /> with preprocessing enabled, depending on circumstances.
        /// </summary>
        /// <param name="evnt"></param>
        /// <returns></returns>
        public virtual Task ProcessEvent(IEvent evnt) => ChainEvent(evnt, true);

        public void Complete()
        {
            if (_isCompleted) return;
            try
            {
                _incomingEventSubject.OnCompleted();
            }
            finally
            {
                _isCompleted = true;
            }
        }

        public Task RunExclusiveOperation(Action action, CancellationToken cancelToken)
        {
            void plzShim(object state)
            {
                ExclusionLocker.Wait((Action)state);
            }
            return exFactory.StartNew(plzShim, action, cancelToken);
        }

        public Task RunExclusiveOperation(Action action) => RunExclusiveOperation(action, CancellationToken.None);

        public IDisposable SubscribePreprocess(IObserver<IEvent, IEvent> observer) =>
                                    _preProcessing.GetOrAdd(observer,
                Disposable.Create(() => _preProcessing.TryRemove(observer, out _)));

        protected virtual void Dispose(bool disposing)
        {
            //Action OnCompletedAggregation(ConcurrentDictionary<IObserver<IEvent>, IDisposable> dict) => dict.Keys.Aggregate<IObserver<IEvent>, Action>(() => { }, (a, o) => o.OnCompleted + a);
            if (IsDisposed || !disposing) return;
            _tpschedPair.Complete();
            _tpschedPair.Completion.GetAwaiter().GetResult();
            IsDisposed = true;
            /*var finalAct = _preProcessing.Keys.Aggregate<IObserver<IEvent, IEvent>, Action>(() => { },
                               (a, o) => (() => o.OnCompleted()) + a)
                           + OnCompletedAggregation(_observesall)
                           + OnCompletedAggregation(_observesread)
                           + OnCompletedAggregation(_observeswrite);*/
            //finalAct();
            Complete();
            _allSubject.Dispose();
            _readSubject.Dispose();
            _writeSubject.Dispose();
            _incomingEventSubject.Dispose();
            if (_initialScheduler is IDisposable disp)
                disp.Dispose();
            //_writerScheduler.Dispose();
            TokenSource.Dispose();
        }

        //protected IDisposable innerSubscribe(ConcurrentDictionary<IObserver<IEvent>, IDisposable> obsList,
        //            IObserver<IEvent> observer) =>
        //    obsList.GetOrAdd(observer, Disposable.Create(() => obsList.TryRemove(observer, out _)));

        public EventPipe(IScheduler onReceiveScheduler)
        {
            _initialScheduler = onReceiveScheduler;
            TokenSource = new CancellationTokenSource();
            _tpschedPair = new ConcurrentExclusiveSchedulerPair();
            _allScheduler = new TaskPoolScheduler(new TaskFactory(TokenSource.Token));
            _readerScheduler = new TaskPoolScheduler(new TaskFactory(TokenSource.Token,
                TaskCreationOptions.PreferFairness, TaskContinuationOptions.None, _tpschedPair.ConcurrentScheduler));
            exFactory = new TaskFactory(MainToken, TaskCreationOptions.PreferFairness, TaskContinuationOptions.None, _tpschedPair.ExclusiveScheduler);
            _incomingEventSubject = new Subject<(IEvent @event, bool doPreProcessing)>();
            _allSubject = new Subject<IEvent>();
            _readSubject = new Subject<IEvent>();
            _writeSubject = new Subject<IEvent>();
            var holdSyntax = _incomingEventSubject.ObserveOn(_initialScheduler)
                .Select(evnt =>
                evnt.doPreProcessing && !_preProcessing.IsEmpty
                    ? _preProcessing.Keys.Aggregate(evnt.@event, (e, o) => o.OnNext(e))
                    : evnt.@event);
            holdSyntax.Subscribe(_allSubject.Checked());
            holdSyntax.Subscribe(_readSubject.Checked());
            holdSyntax.Subscribe(_writeSubject.Checked());
            ObserveFirst = _allSubject.ObserveOn(_allScheduler);
            ObserveConcurrent = _readSubject.ObserveOn(_readerScheduler);
            ObserveSynchronous = _writeSubject;
        }

        public EventPipe() : this(new EventLoopScheduler(threadStart => new Thread(threadStart)
        {
            Name = "EventPipeThread",
            IsBackground = true,
        }))
        { }
    }
}