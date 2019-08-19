using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.IO;

namespace Versagen.Utils
{
    /// <summary>
    /// A class for dice rolling. Pretty-prints the results.
    /// </summary>
    // ReSharper disable once CommentTypo
    // ReSharper disable once InheritdocConsiderUsage
    public class Dice : IComparable<int>, IEnumerable<int>, IComparable<Dice>, IEquatable<int>, IEquatable<Dice>, IFormattable
    {
        public static readonly string CommandReadPattern = @"(?:roll\()?(\d+)d(\d+)((?:\+|-)\d+)?\)?";
        public ushort SidesPerDie { get; }

        public ushort NumberDice { get; }

        public int Modifier { get; }

        //TODO:Immutable array here or...? Performance vs. integrity.
        public IEnumerable<int> IndividualDice { get; }

        public int Result { get; }

        public int CompareTo(int other)
        {
            return Result - other;
        }

        public int CompareTo(Dice other)
        {
            return Result - other.Result;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return IndividualDice.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return IndividualDice.GetEnumerator();
        }

        public string ToEquationString()
        {
            using (var b = new StringWriter())
            {
                b.Write($"({IndividualDice.First()}");
                foreach (var num in IndividualDice.Skip(1))
                    b.Write($"+{num}");
                b.Write($"){(Modifier < 0 ? '-' : '+')}{Math.Abs(Modifier)}={Result}");
                return b.ToString();
            }
        }
        public string ToCommandString() => $"roll({NumberDice}d{SidesPerDie}{(Modifier < 0 ? '-' : '+')}{Math.Abs(Modifier)})";
        public override string ToString() => ToEquationString();
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "G";
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;
            switch (format.ToUpperInvariant()[0])
            {
                case 'G':
                case 'E':
                    return ToEquationString();
                case 'C':
                    return ToCommandString();
                case 'R':
                    return Result.ToString();
                case 'M':
                    return Modifier.ToString();
                //TODO: Do I really need this here?
                //case 'I':
                //    var splitted = format.Split(":");
                //    switch (splitted.Length)
                //    {
                //        case 1:
                //            using (var b = new StringWriter())
                //            {
                //                b.Write($"({IndividualDice.First()}");
                //                foreach (var num in IndividualDice.Skip(1))
                //                    b.Write($"+{num}");
                //                return b.ToString();
                //            }
                //    }
                //    break;
                default:
                    throw new FormatException("Format not supported for dice printing");
            }
        }

        public bool Equals(int other)
        {
            return Result == other;
        }
        public bool Equals(Dice other)
        {
            return Result == other.Result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Keep rolling for as long as desired.
        /// WARNING: Calling <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> or similar methods will cause an out-of-memory exception and infinite loop.
        /// </summary>
        /// <param name="sides"></param>
        /// <returns></returns>
        public static IEnumerable<int> InfiniteRoll(ushort sides)
        {
            var rand = new Random();
            var max = 100 * (sides + 1);
            for (;;)
                yield return (int)Math.Floor((double)rand.Next(100, max) / 100);
        }

        /// <summary>
        /// Roll a ser number of dice and return them as an array.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="sides"></param>
        /// <returns></returns>
        public static int[] Roll(ushort count, ushort sides)
        {

            var rand = new Random();
            var max = 100 * (sides + 1);
            var rolls = new int[count];
            for (var i = 0; i < count; i++)
                rolls[i] = (int)Math.Floor((double)rand.Next(100, max) / 100);
            return rolls;
        }
        public static int Roll(ushort sides)
        {
            var rand = new Random();
            var max = 100 * (sides + 1);
            return (int)Math.Floor((double)rand.Next(100, max) / 100);
        }
        public static Dice CoinFlip(ushort count = 1) => new Dice(count, 2, 0);
        public static IEnumerable<bool> CoinFlipAsHeads(ushort count = 1)
        {
            var max = 100 * (2 + 1);
            var dieList = new bool[count];
            Random rand = new Random();
            for (int i = 0; i < count; i++)
                dieList[i] = Math.Floor((double)rand.Next(1, max) / 100) == 1;
            return dieList;
        }
        public static IEnumerable<string> CoinFlipPrintResults(ushort count = 1) => CoinFlipAsHeads(count).Select(c => c ? "Heads!" : "Tails!");

        public static Dice Parse(string commandStr)
        {
            var match = System.Text.RegularExpressions.Regex.Match(commandStr, CommandReadPattern);
            if (!match.Success)
                throw new FormatException("Could not parse Dice roll from string.");
            if (ushort.TryParse(match.Groups[1].Value, out var count))
            {
                if (ushort.TryParse(match.Groups[2].Value, out var sides))
                    return new Dice(count, sides, match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0);
                throw new FormatException("Could not parse a number of sides from this string!");
            }
            throw new FormatException("Could not parse a number of dice from this string!");
        }

        /// <summary>
        /// Convert to int in order to work with the result as a value itself using comparison operators.
        /// </summary>
        /// <param name="roll">The roll being worked on.</param>
        public static implicit operator int(Dice roll) => roll.Result;
        
        private Dice(Dice parent, int result, bool addModifier)
        {
            SidesPerDie = parent.SidesPerDie;
            NumberDice = 1;
            Modifier = addModifier? parent.Modifier : 0;
            Result = result+Modifier;
            IndividualDice = new int[] {result};
        }
        public Dice(ushort count, ushort sides, int modifier)
        {
            SidesPerDie = sides;
            NumberDice = count;
            Modifier = modifier;
            IndividualDice = Roll(count, sides);
            Result = IndividualDice.Sum() + modifier;
        }
    }
}
