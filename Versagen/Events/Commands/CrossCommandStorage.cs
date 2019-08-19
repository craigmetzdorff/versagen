using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Versagen.Events.Commands
{
    public class CrossCommandStorage :ICrossCommandStorage
    {
        private ConcurrentDictionary<Type, ImmutableList<object>> _servicesWithin { get; }

        private ConcurrentDictionary<string, string> _stringData { get; }

        public bool AddService(Type key, object obj)
        {
            bool wasNew = default;
            // ReSharper disable once AssignmentInConditionalExpression
            _servicesWithin.AddOrUpdate(key, ImmutableList<object>.Empty.Add(obj), (_, il) => (wasNew = !il.Contains(obj)) ? il : il.Add(obj));
            return wasNew;
        }

        public bool AddStringData(string key, string data) => _stringData.TryAdd(key, data);

        public void Dispose()
        {

        }

        public object GetService(Type serviceType)
        {
            IEnumerable<object> AppendObjs(IEnumerable<object> inObjs) => inObjs.Concat(_servicesWithin[typeof(object)].Where(c => c.GetType().IsSubclassOf(serviceType)));
            var typesOut = _servicesWithin.AsQueryable().Where(kvp => kvp.Key == serviceType).Select(kvp => kvp.Value);
            return !typesOut.Any() ?
                serviceType.IsSubclassOf(typeof(IEnumerable)) ?
                    AppendObjs(typesOut) :
                    AppendObjs(typesOut).FirstOrDefault() :
                serviceType.IsSubclassOf(typeof(IEnumerable)) ?
                    AppendObjs(typesOut.AsEnumerable()) :
                    typesOut.FirstOrDefault();
        }

        public bool HasStringFlag(string str) => _stringData.ContainsKey(str);

        public bool RemoveObject(object obj)
        {
            var testObj = _servicesWithin.SingleOrDefault(kvp => kvp.Value == obj);
            if (testObj.Equals(default(KeyValuePair<Type, ImmutableList<object>>)))
                return false;
            bool wasNew = default;
            //Below was intentional
            // ReSharper disable once AssignmentInConditionalExpression
            _servicesWithin.AddOrUpdate(testObj.Key, ImmutableList<object>.Empty,
                (_, il) => (wasNew = !il.Contains(obj)) ? il : il.Add(obj));
            return wasNew;
        }

        public bool RemoveServices(Type key, out IEnumerable<object> obj)
        {
            if (_servicesWithin.TryRemove(key, out var objs))
            {
                obj = objs.AsEnumerable();
                return true;
            };
            obj = null;
            return false;
        }

        public bool RemoveStringData(string key, out string data) => _stringData.TryRemove(key, out data);

        public bool RemoveStringFlag(string str) => _stringData.TryRemove(str, out _);

        public bool TryAddObject(object obj)
        {
            return !_servicesWithin.Values.Contains(obj) && AddService(typeof(object), obj);
        }

        public bool TryAddService(Type serviceType, object obj) =>
            _servicesWithin.TryAdd(serviceType, ImmutableList<object>.Empty.Add(obj));

        public bool TryAddStringData(string key, string data)
            => _stringData.TryAdd(key, data);

        public bool TryAddStringFlag(string str) => _stringData.TryAdd(str, "");

        public CrossCommandStorage(ConcurrentDictionary<string, string> stringData, ConcurrentDictionary<Type, ImmutableList<object>> servicesWithin)
        {
            _stringData = stringData;
            _servicesWithin = servicesWithin;
        }
    }
}
