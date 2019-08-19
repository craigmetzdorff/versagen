using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Versagen.Rules;
using System.Linq;
using System.Threading.Tasks;
using Versagen.Events.Commands;

namespace Versagen
{
    public class VersaDescription :IList<(string descPart, IConditionalRule[] displayConditions)>
    {
        public List<(string descPart, IConditionalRule[] displayConditions)> DescriptionParts { get; }

        public VersaDescription()
        {
            DescriptionParts = new List<(string descPart, IConditionalRule[] displayConditions)>();
        }

        public void AddDescriptionParts(string description, params IConditionalRule[] rules) => DescriptionParts.Add((description, rules));

        public string BuildDescription(ICommandContext context, IServiceProvider provider)
        {
            var builder = new StringBuilder();
            foreach (var (descPart, displayConditions) in DescriptionParts)
            {
                if (displayConditions == null || !displayConditions.Any() || (context != null && displayConditions.All(c => c.CheckRule(context, provider).GetAwaiter().GetResult().Item1)))
                    builder.AppendLine(descPart);
            }
            return builder.ToString();
        }

        IEnumerator<(string descPart, IConditionalRule[] displayConditions)> IEnumerable<(string descPart, IConditionalRule[] displayConditions)>.GetEnumerator() => DescriptionParts.GetEnumerator();
        public override string ToString() => BuildDescription(null, null);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) DescriptionParts).GetEnumerator();

        public void Add((string descPart, IConditionalRule[] displayConditions) item) => DescriptionParts.Add(item);

        void ICollection<(string descPart, IConditionalRule[] displayConditions)>.Clear() => DescriptionParts.Clear();

        bool ICollection<(string descPart, IConditionalRule[] displayConditions)>.Contains((string descPart, IConditionalRule[] displayConditions) item) => DescriptionParts.Contains(item);

        void ICollection<(string descPart, IConditionalRule[] displayConditions)>.CopyTo((string descPart, IConditionalRule[] displayConditions)[] array, int arrayIndex) => DescriptionParts.CopyTo(array, arrayIndex);

        bool ICollection<(string descPart, IConditionalRule[] displayConditions)>.Remove((string descPart, IConditionalRule[] displayConditions) item) => DescriptionParts.Remove(item);

        int ICollection<(string descPart, IConditionalRule[] displayConditions)>.Count => DescriptionParts.Count;

        bool ICollection<(string descPart, IConditionalRule[] displayConditions)>.IsReadOnly => ((ICollection<(string descPart, IConditionalRule[] displayConditions)>) DescriptionParts).IsReadOnly;

        int IList<(string descPart, IConditionalRule[] displayConditions)>.IndexOf((string descPart, IConditionalRule[] displayConditions) item) => DescriptionParts.IndexOf(item);

        void IList<(string descPart, IConditionalRule[] displayConditions)>.Insert(int index, (string descPart, IConditionalRule[] displayConditions) item) => DescriptionParts.Insert(index, item);

        void IList<(string descPart, IConditionalRule[] displayConditions)>.RemoveAt(int index) => DescriptionParts.RemoveAt(index);

        (string descPart, IConditionalRule[] displayConditions) IList<(string descPart, IConditionalRule[] displayConditions)>.this[int index]
        {
            get => DescriptionParts[index];
            set => DescriptionParts[index] = value;
        }
        public static implicit operator VersaDescription(string input) => new VersaDescription {(input, null)};

        public static VersaDescription operator +(VersaDescription one, VersaDescription two)
        {
            var newDesc = new VersaDescription();
            foreach (var c in one)
                newDesc.AddDescriptionParts(c.descPart, c.displayConditions);
            foreach (var c in two)
                newDesc.AddDescriptionParts(c.descPart, c.displayConditions);
            return newDesc;
        }
    }
}
