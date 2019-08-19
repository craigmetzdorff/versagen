using System;
using Jolt.Utils;
using Versagen.IO;
using Versagen.Scenarios;

namespace Versagen.SampleConsoleOne
{
    class Program
    {
        static void Main(string[] args)
        {
            var hastyTest = new VersaHastyImplementationExtra();
            hastyTest.Runner.StartEventLoop();
            while (true)
            {
                var input = Console.ReadLine();
                hastyTest.AddEvent(new VersaHastyImplementationExtra.ConsoleEvent
                {
                    FullMessage = input,
                    Services = hastyTest.provider,
                    Scenario = new UnionType<VersaCommsID, IScenario>(0),
                    Terminal = new UnionType<VersaCommsID, IVersaWriter>(0)
                });
            }
        }
    }
}
