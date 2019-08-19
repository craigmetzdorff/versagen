using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.XML.Schemas;

namespace Versagen.Entity
{
    public static class StatExtensions
    {
        internal static T StatAggregateHelper<T>(IStat<T> baseStat, IEnumerable<IModifyStat<T>> aggregatorCandidates)
        {
            var modifyStats = aggregatorCandidates as IModifyStat<T>[] ?? aggregatorCandidates.ToArray();
            return !modifyStats.Any() ? baseStat.Value : modifyStats.Aggregate(baseStat.Value, (current, stat) => stat.Aggregate(current));
        }

        public static T GetStatTotal<T>(this IEnumerable<IStat> stats, IStat<T> baseStat)
        {
            var modifiers = stats.OfType<IModifyStat<T>>().Where(c => c.Modifies == baseStat.Name);
            return StatAggregateHelper(baseStat, modifiers);
        }

        public static T GetStatTotal<T>(this IEnumerable<IStat> stats, IStat baseStat)
        {
            var castedStat = baseStat as IStat<T> ?? throw new InvalidCastException($"Stat {baseStat.Name} is not of the expected type.");
            return GetStatTotal(stats, castedStat);
        }

        public static T GetStatTotal<T>(this IEnumerable<IStat> stats, string baseStatName)
        {
            var statArr = stats as IStat[] ?? stats.ToArray();
            var baseStat = statArr.SingleOrDefault(c => c.Name == baseStatName);
            return GetStatTotal<T>(statArr, baseStat);
        }

        public static T GetStatTotal<T>(this IDictionary<string, IStat> stats, string baseStatName)
        {
            var baseStat = stats[baseStatName];
            return GetStatTotal<T>(stats.Values, baseStat);
        }

        public static T GetStatTotal<T>(this IEntity entity, string baseStatName) =>
            GetStatTotal<T>(entity.Stats, baseStatName);

        public static bool TryGetStatTotal<T>(this IEntity entity, string baseStatName, out T total)
        {
            if (entity.Stats.TryGetValue(baseStatName, out var baseStat))
            {
                total = GetStatTotal<T>(entity.Stats.Values, baseStat);
                return true;
            }

            total = default;
            return false;
        }
    }
}
