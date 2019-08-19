using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen
{
    /// <summary>
    /// For objects that can be cloned and merged later on for saving.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMergeable<T> : IDeepCloneable<T>
    {
        T Union(T other);
        void Merge(T other);
    }
}
