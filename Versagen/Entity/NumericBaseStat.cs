using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Versagen.Annotations;

namespace Versagen.Entity
{
    public struct NumericBaseStat :IStat<int>
    {
        public NumericBaseStat(string name, int value = 0)
        {
            PropertyChanged = new PropertyChangedEventHandler((sender, args) => {});
            Name = name;
            Modifies = "";
            Value = value;
        }

        public int Value { get; set; }
        public int CompareTo(IStat<int> other) => CompareTo(other.Value);

        public bool Equals(int value) => Value.Equals(value);

        public bool Equals(IStat<int> other) => Equals(other.Value);

        public int CompareTo(int other) => Value.CompareTo(other);

        public string Name { get; }
        public string Modifies { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Value.ToString();
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
