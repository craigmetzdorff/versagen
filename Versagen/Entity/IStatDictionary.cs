using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;

namespace Versagen.Entity
{
    public interface IStatDictionary:IDictionary<string, IStat>, INotifyCollectionChanged, IObservable<NotifyCollectionChangedEventArgs>
    {
        void Add(IStat stats);
        bool TryAdd(IStat stats);
        bool TryRemove(IStat stats);
    }

    public class StatDictionary : ObservableBase<NotifyCollectionChangedEventArgs>, IStatDictionary
    {
        private Dictionary<string, IStat> _stats;
        public IEnumerator<KeyValuePair<string, IStat>> GetEnumerator()
        {
            return _stats.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _stats).GetEnumerator();
        }

        void ICollection<KeyValuePair<string, IStat>>.Add(KeyValuePair<string, IStat> item)
        {
            ((ICollection<KeyValuePair<string, IStat>>) _stats).Add(item);
        }

        public void Clear()
        {
            _stats.Clear();
        }

        bool ICollection<KeyValuePair<string, IStat>>.Contains(KeyValuePair<string, IStat> item)
        {
            return ((ICollection<KeyValuePair<string, IStat>>)_stats).Contains(item);
        }

        void ICollection<KeyValuePair<string, IStat>>.CopyTo(KeyValuePair<string, IStat>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, IStat>>)_stats).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, IStat>>.Remove(KeyValuePair<string, IStat> item)
        {
            return ((ICollection<KeyValuePair<string, IStat>>)_stats).Remove(item);
        }

        public int Count => _stats.Count;
        bool ICollection<KeyValuePair<string, IStat>>.IsReadOnly => ((ICollection<KeyValuePair<string, IStat>>)_stats).IsReadOnly;

        void IDictionary<string, IStat>.Add(string key, IStat value)
        {
            _stats.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _stats.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _stats.Remove(key);
        }

        public bool TryGetValue(string key, out IStat value)
        {
            return _stats.TryGetValue(key, out value);
        }

        public IStat this[string key]
        {
            get => _stats[key];
            set => _stats[key] = value;
        }

        public ICollection<string> Keys => _stats.Keys;

        public ICollection<IStat> Values => _stats.Values;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private IObservable<EventPattern<NotifyCollectionChangedEventArgs>> _notifier;

        public StatDictionary()
        {
            _stats = new Dictionary<string, IStat>();
            //TODO: Generate each time instead?
            _notifier = Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    a => CollectionChanged += a, a => CollectionChanged -= a);
        }

        public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer)
        {
            throw new NotImplementedException();
        }

        protected override IDisposable SubscribeCore(IObserver<NotifyCollectionChangedEventArgs> observer)
        {
            throw new NotImplementedException();
        }

        public void Add(IStat stats)
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(IStat stats)
        {
            throw new NotImplementedException();
        }

        public bool TryRemove(IStat stats)
        {
            throw new NotImplementedException();
        }
    }
}
