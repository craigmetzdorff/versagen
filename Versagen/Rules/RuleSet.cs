using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Versagen.Rules
{
    public class RuleSet<T> : IRuleSet<T>
        where T:IRule
    {
        internal class RuleSetInternalEqualityComparer : IEqualityComparer<KeyValuePair<string,T>>, IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
                => x.Tag == y.Tag;

            public bool Equals(KeyValuePair<string, T> x, KeyValuePair<string, T> y)
                => x.Key == y.Key;

            public int GetHashCode(T obj)
                => obj.GetHashCode();

            public int GetHashCode(KeyValuePair<string, T> obj)
                => obj.GetHashCode();
        }
        System.Collections.Concurrent.ConcurrentDictionary<string, T> _rules;
        public int Count => _rules.Count();
        public bool IsReadOnly => false;
        public bool Add(T item)
        {
            if (!_rules.ContainsKey(item.Tag))
            {
                _rules.GetOrAdd(item.Tag, item);
                return true;
            }
            return false;
        }
        public void Clear()
            =>
                _rules.Clear();
        public bool Contains(T item)
            =>
                _rules.ContainsKey(item.Tag);
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            var count = _rules.Count;
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
                throw new ArgumentException($"The Array provided is too small to hold all elements starting from index {arrayIndex}.",nameof(arrayIndex));
            int numCopied = 0;
            var rules = _rules.Values.ToArray();
            for (int i = 0; i < count && numCopied < count; i++)
            {
                    array[arrayIndex + numCopied] = rules[i];
                    numCopied++;
                
            }
        }
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                _rules.TryRemove(item.Tag, out _);
            }
        }
        public IEnumerator<T> GetEnumerator()
            =>
                _rules.Values.GetEnumerator();
        public T GetRule(string Tag)
        {
            return _rules[Tag];
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rules.GetEnumerator();
        }
        void ICollection<T>.Add(T item)
        {
            if (!_rules.ContainsKey(item.Tag))
            {
                _rules.GetOrAdd(item.Tag, item);
            }
        }
        IEnumerable<TRule> GetRulesByType<TRule>() where TRule:T
        {
            return _rules.Values.Where(rule => rule.GetType().IsAssignableFrom(typeof(TRule))).Cast<TRule>();
        }
        public TRule GetRule<TRule>(string Tag) where TRule : T
            => (TRule)_rules[Tag];
        public void IntersectWith(IEnumerable<T> other)
        {
            var comp = new RuleSetInternalEqualityComparer();
            foreach (var deletR in _rules.Except(other.Select(r => new KeyValuePair<string, T>(r.Tag, r)), comp))
            {
                _rules.TryRemove(deletR.Key, out _);
            }
        }
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            int counted = 0;
            foreach (var laRule in other)
            {
                if (_rules.ContainsKey(laRule.Tag))
                    counted++;
            }
            return counted > _rules.Count;
        }
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int counted = 0;
            var rCount = _rules.Count;
            foreach (var laRule in other)
            {
                if (!_rules.ContainsKey(laRule.Tag))
                    return false;
                if (++counted >= rCount)
                    return false;
            }
            return true;
        }
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            int counted = 0;
            foreach (var laRule in other)
            {
                if (_rules.ContainsKey(laRule.Tag))
                    counted++;
            }
            return counted >= _rules.Count;
        }
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            int counted = 0;
            var rCount = _rules.Count;
            foreach (var laRule in other)
            {
                if (!_rules.ContainsKey(laRule.Tag))
                    return false;
                if (++counted > rCount)
                    return false;
            }
            return true;
        }
        public bool Overlaps(IEnumerable<T> other)
            => other.Any(r => _rules.ContainsKey(r.Tag));
        public bool SetEquals(IEnumerable<T> other)
        {
            int counted = 0;
            var rCount = _rules.Count;
            foreach (var laRule in other)
            {
                if (!_rules.ContainsKey(laRule.Tag))
                    return false;
                if (++counted > rCount)
                    return false;
            }
            return counted == rCount;
        }
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var comp = new RuleSetInternalEqualityComparer();
            foreach (var item in other.Distinct(comp))
            {
                if (!_rules.TryRemove(item.Tag, out _))
                    _rules.GetOrAdd(item.Tag, item);
            }
        }
        public void UnionWith(IEnumerable<T> other)
        {
            var comp = new RuleSetInternalEqualityComparer();
            foreach (var deletR in other.Select(r => new KeyValuePair<string, T>(r.Tag, r)).Except(_rules, comp))
            {
                _rules.TryRemove(deletR.Key, out _);
            }
        }
        public bool Remove(T item)
            =>
                _rules.TryRemove(item.Tag, out _);
        IEnumerable<TRule> IRuleSet<T>.GetRulesByType<TRule>()
        {
            return _rules.Values.Where(rule => rule.GetType().IsAssignableFrom(typeof(TRule))).Cast<TRule>();
        }
        public RuleSet(IEnumerable<T> other) : this()
        {
            foreach (var r in other) Add(r);
        }
        public RuleSet()
        {
            _rules = new System.Collections.Concurrent.ConcurrentDictionary<string, T>();
        }
    }
}
