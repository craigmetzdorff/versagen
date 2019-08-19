using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.XML.Schemas;

namespace Versagen
{
    public interface IDeepCloneable<T> :IDeepCloneable
    {
        new T DeepClone(bool requireNewRef = false);
    }

    public interface IDeepCloneable
    {
        object DeepClone(bool requireNewRef = false);
    }


    public static class CloneHelper
    {
        public static void DeepCloneInPlace<TDeepCloneable>(this IList<TDeepCloneable> cloneables)
        where TDeepCloneable:IDeepCloneable<TDeepCloneable>
        {
            lock (cloneables)
                for (var i = 0; i < cloneables.Count; i++)
                {
                    cloneables[i] = cloneables[i].DeepClone();
                }
        }

        public static void SelectiveDeepCloneInPlace<T>(this IList<T> target)
        {
            lock (target)
                for (var i = 0; i < target.Count; i++)
                {
                    if (target[i] is IDeepCloneable<T> item)
                    {
                        target[i] = item.DeepClone();
                    }
                }
        }

        public static IEnumerable<TDeepCloneable> DeepClone<TDeepCloneable>(this IEnumerable<TDeepCloneable> deepCloneables)
            where TDeepCloneable : IDeepCloneable<TDeepCloneable>
            => deepCloneables.Select(c => c.DeepClone());
    }
}
