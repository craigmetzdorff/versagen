using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jolt.Utils;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Entity;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Scenarios;
using Versagen.Utils;

namespace Versagen.SampleConsoleOne
{
    public class VersaHastyImplementation
    {

        public class ConsoleEvent : IEvent
        {
            public bool IgnoreThis { get; set; }
            public IEventPipe SourcePipe { get; set; }
            public IServiceProvider Services { get; set; }
            public UnionType<VersaCommsID, IScenario> Scenario { get; set; }
            public UnionType<VersaCommsID, IVersaWriter> Terminal { get; set; }
            public string FullMessage { get; set; }
            public UnionType<VersaCommsID, IPlayer> Player { get; set; }
            public UnionType<VersaCommsID, IEntity> Entity { get; set; }
        }

        public List<int> constTestAgainst = new List<int>();


        public IServiceProvider provider;

        public interface IInjectedTester
            {
                bool Injected { get; set; }
            }

            public class InjectThisClass : IInjectedTester
            {
                public static bool injected = false;

                public bool Injected { get => injected; set => injected = value; }
            }

            public class CommandTestMeOut : IDisposable
            {

                public static bool DisposedTest = false;

                public IInjectedTester InjectedTester { get; set; }

                public int countServices { get; set; }

                public Task TestInjectedWorked(ICommandContext context)
                {
                    InjectedTester.Injected = true;
                    return context.OriginTerm.WriteLineAsync($"I'VE BEEN INJECTED. Count: {countServices}");
                    
                }

                public CommandTestMeOut(List<int> theInts)
                {
                    countServices = theInts.Count;
                }

                #region IDisposable Support
                private bool disposedValue = false; // To detect redundant calls

                protected virtual void Dispose(bool disposing)
                {
                    if (!disposedValue)
                    {
                        if (disposing)
                        {
                            DisposedTest = true;
                        }

                        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                        // TODO: set large fields to null.

                        disposedValue = true;
                    }
                }

                // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
                // ~CommandTestMeOut() {
                //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                //   Dispose(false);
                // }

                // This code added to correctly implement the disposable pattern.
                public void Dispose()
                {
                    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                    Dispose(true);
                    // TODO: uncomment the following line if the finalizer is overridden above.
                    // GC.SuppressFinalize(this);
                }
                #endregion
            }

            bool didDebug = false;
            public readonly EventQueue<CommandManager<CommandContext>, VersaCommand, CommandContext, CommandContext.Builder> EQ;

            const int CountTestTestAgainst = 3;

            public VersaHastyImplementation()
            {

                for (var i = 0; i < CountTestTestAgainst; i++)
                    constTestAgainst.Add(CountTestTestAgainst);
                provider = new ServiceCollection().AddSingleton(constTestAgainst)
                    .AddTransient<IInjectedTester, InjectThisClass>().BuildServiceProvider();
            EQ = new Versagen.Events.EventQueue<CommandManager<CommandContext>, VersaCommand, CommandContext, CommandContext.Builder>(
                new CommandManager<CommandContext>("Test commands", "!")
                {
                    new VersaCommand.Builder
                    {
                        Name = "Test Command",
                        CallLine = "testMe",
                        Description = "This code will fail at runtime without a defined description for each command.",
                        Predicates = new List<RulesOfNature.IConditionalRule>(),
                        Do = c => c.OriginTerm.WriteLineAsync("testMe recognized!"),
                    }.Build(),
                    new VersaCommand.Builder
                    {
                        Name = "Test Command2",
                        CallLine = "ignoreMe",
                        Description = "This code will fail at runtime without a defined description for each command.",
                        Predicates = new List<RulesOfNature.IConditionalRule>(){
                            new RulesOfNature.LambdaRule("DebugFalse", "debugIsFalse", (c,s) => Task.FromResult((!didDebug, "didDebug is already true" )))
                        },
                        Do = c => Task.Run(() => didDebug = true),
                    }.Build(),
                    new VersaCommand.Builder
                    {
                        Name = "Echo",
                        CallLine = "echo",
                        Description = "This code will fail at runtime without a defined description for each command.",
                        Predicates = new List<RulesOfNature.IConditionalRule>(){
                        },
                        Do =  c =>
                            c.OriginTerm.WriteLineAsync(c.MessageRemainder),
                    }.Build(),
                    new VersaCommand.Builder
                    {
                        Name = "Injection Test",
                        CallLine = "injectMe",
                        Description = "Tests how this works with a generated class instead.",
                        Predicates = new List<RulesOfNature.IConditionalRule>(),
                    }.UsingTransientClass<CommandTestMeOut>(c => c.TestInjectedWorked).Build(),
                    new VersaCommand.Builder
                    {
                        Name = "Test Command2",
                        CallLine = "roll",
                        Description = "This code will fail at runtime without a defined description for each command.",
                        Predicates = new List<RulesOfNature.IConditionalRule>(),
                        Do = c => c.OriginTerm.WriteLineAsync(Dice.Parse(c.MessageRemainder).ToString()),
                    }.Build(),

                }, new ConsoleContextBuilder(provider));
            }
        }
    }
