using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Versagen.Utils;
using Xunit.Abstractions;

namespace Versagen.DefaultImplementations.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void RollTheDice()
        {
            var roll1 = new Dice(5, 20, 0);
            var roll2 = new Dice(5, 20, 0);
            Debug.WriteLine(roll1.ToString());
            Debug.Print(string.Join('+', roll1) + "=");
            Debug.WriteLine(roll1.IndividualDice.Sum().ToString());
            Debug.Print(string.Join('+', roll1) + "=");
            Debug.WriteLine(roll1.IndividualDice.Sum().ToString());
            Debug.WriteLine(roll2.ToString());
            Assert.True(roll1 + roll2 == roll1.Result + roll2.Result);
            var rand = new Random();
            var sides = (ushort)rand.Next(ushort.MinValue, ushort.MaxValue);

        }
    }
}
