using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Versagen.Events.Commands;
using Versagen.Rules;

namespace Versagen.Events.Actions
{
    public class LambdaEventReaction : ObserverBase<IEvent>, IReaction
    {
        private readonly Func<IEvent, bool> _runAt;

        private readonly Func<IEvent, Task> _runThis;

        private volatile IDisposable _subscriberThing = default;

        public LambdaEventReaction(string tag, Func<IEvent, bool> runPredicate, Func<IEvent, Task> reactFunc)
        {
            Tag = tag;
            _runAt = runPredicate;
            _runThis = reactFunc;
        }

        public void SingleSubscribe(IObservable<IEvent> observable)
        {
            var cloned = (LambdaEventReaction) MemberwiseClone();
            cloned._subscriberThing = observable.Subscribe(this);
        }


        public string Tag { get; }

        protected override void OnErrorCore(Exception error)
        {
            throw new NotImplementedException();
        }

        protected override void OnCompletedCore()
        {
            _subscriberThing?.Dispose();
        }

        protected override async void OnNextCore(IEvent value)
        {
            if (!value.IsSystemMessage || value.IgnoreThis) return;
            if (!_runAt(value)) return;
            await _runThis(value).ConfigureAwait(false);
            _subscriberThing?.Dispose();
        }

        void IObserver<CommandRunStateEventArgs>.OnError(Exception error)
        {
            OnError(error);
        }

        public virtual void OnNext(CommandRunStateEventArgs value)
        {
            //OnNext(value.Event); //TODO: Add Func call here.
        }

        void IObserver<CommandRunStateEventArgs>.OnCompleted()
        {
            OnCompleted();
        }
    }
}
