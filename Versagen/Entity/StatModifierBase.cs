using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Versagen.Annotations;

namespace Versagen.Entity
{
    public abstract class StatModifierBase<T> :IModifyStat<T> where T:IComparable<T>
    {
        private T _value;

        protected StatModifierBase(IStat<T> modifiedStat, string name, bool isRemovable)
        {
            _modifiedStat = modifiedStat;
            Name = name;
            IsRemovable = isRemovable;
        }

        private IStat<T> _modifiedStat { get; }

        public T Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged();}
        }

        public virtual int CompareTo(IStat<T> other)
        {
            return Value.CompareTo(other.Value);
        }

        public virtual bool Equals(T value)
        {
            return Value.Equals(value);
        }

        public virtual bool Equals(IStat<T> other)
        {
            return Value.Equals(other.Value);
        }

        public virtual int CompareTo(T other)
        {
            return Value.CompareTo(other);
        }
        public string Name { get; }
        public string Modifies => _modifiedStat.Name;
        public bool IsRemovable { get; }
        public abstract T Aggregate(T other);
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
