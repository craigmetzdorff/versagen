using System;
using System.ComponentModel;

namespace Versagen.Entity
{
    public interface IStat : INotifyPropertyChanged
    {
        string Name { get; }
        string Modifies { get; }
    }

    public interface IStat<T>: IStat, IComparable<T>
    {
        T Value { get; set; }
        int CompareTo(IStat<T> other);
        bool Equals(T value);
        bool Equals(IStat<T> other);
    }

    public interface IModifyStat<T> : IStat<T>
    {
        bool IsRemovable { get; }
        T Aggregate(T other);
    }
}
