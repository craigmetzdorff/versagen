using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Entity
{
    public class NumericStatModifier : StatModifierBase<int>
    {
        public NumericStatModifier(IStat<int> modifiedStat, string name, bool isRemovable) : base(modifiedStat, name, isRemovable)
        {
        }

        public override int Aggregate(int other) => Value + other;
    }
}
