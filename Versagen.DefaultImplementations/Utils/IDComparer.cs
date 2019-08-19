using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Utils
{
    public class IDComparer<T> :IEqualityComparer<T>
    {
        private Func<T, VersaCommsID> Selector;
        public IDComparer(Func<T, VersaCommsID> selector)
        {
            Selector = selector;
        }

        public bool Equals(T x, T y) => Selector(x).Equals(Selector(y));

        public int GetHashCode(T obj)
        {
            return Selector(obj).GetHashCode();
        }
    }
}
