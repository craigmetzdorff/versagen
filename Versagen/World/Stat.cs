using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Versagen.Annotations;

namespace Versagen.Entity
{
    public class Stat<T> : IStat<T> where T : IComparable
    {
        public string Name { get; }

        public string Modifies { get; }

        public T Value { get ; set; }

        public int CompareTo(IStat<T> other) => Value.CompareTo(other.Value);

        public int CompareTo(object obj) => throw new NotImplementedException();

        public int CompareTo(T other) => Value.CompareTo(other);

        public bool Equals(T value) => Value.Equals(value);

        public bool Equals(IStat<T> other) => Value.Equals(other.Value);
        
        public Stat(string Name, T InitialValue, string Modifies = null)
        {
            this.Name = Name;
            Value = InitialValue;
            this.Modifies = Modifies;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
