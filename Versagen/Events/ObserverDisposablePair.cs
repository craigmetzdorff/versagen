using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Versagen.XML.Schemas;
using System.Reactive;
using System.Reactive.Disposables;

namespace Versagen.Events
{
    /// <summary>
    /// An observer argument meant to be passed to an observer to ensure it knows how to unsubscribe itself at all times. Useful when a single subscriber may need to subscribe to multiple objects.
    /// </summary>
    public class ObserverDisposablePair
    {
        public IEvent Event { get; }
        public IDisposable Unsubscriber { get; }


        public ObserverDisposablePair(IEvent @event, IDisposable ownDisposer)
        {
            Event = @event;
            Unsubscriber = ownDisposer;
        }

        public ObserverDisposablePair(IEvent evnt)
        {
            Event = @evnt;
            Unsubscriber = Disposable.Empty;
        }

        public void Deconstruct(out IEvent evnt, out IDisposable unsubber)
        {
            evnt = Event;
            unsubber = Unsubscriber;
        }
        public static implicit operator ObserverDisposablePair((IEvent, IDisposable) vtup) => new ObserverDisposablePair(vtup.Item1,vtup.Item2);
    }
}
