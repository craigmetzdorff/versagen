using System;
using Xunit;
using System.Threading.Tasks;
using Versagen.Events;
using Versagen.Events.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
/*
namespace Versagen.Tests
{
    public class EventQueueTests
    {
        public interface IInjectedTester
        {
            bool Injected { get; set; }
        }

        public class InjectThisClass :IInjectedTester
        {
            public static bool injected = false;

            public bool Injected { get => injected; set => injected = value; }
        }

        public class CommandTestMeOut:IDisposable
        {

            public static bool DisposedTest = false;

            public IInjectedTester InjectedTester { get; set; }

            public int countServices { get; set; }

            public Task TestInjectedWorked(ICommandContext context)
            {
                Assert.Equal(countTestTestAgainst, countServices);
                InjectedTester.Injected = true;
                return Task.CompletedTask;
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
        readonly EventQueue<CommandManager<CommandContext>, VersaCommand, CommandContext, CommandContext.Builder> EQ;

        const int countTestTestAgainst = 3;
        [Fact]
        public async void SingleLambdaCommand()
        {
            var even = new CommandManagerTests.DebugEvent("!testMe");
            Assert.True(await EQ.AddEvent(even));
            var (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(didDebug);
            didDebug = false;
        }

        [Fact]
        public async void CheckPrecondition()
        {
            didDebug = true;
            var odd = new CommandManagerTests.DebugEvent("!ignoreMe");
            Assert.True(await EQ.AddEvent(odd));
            Assert.True(await EQ.AddEvent(odd));
            var (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.False(didDebug);
            (foundEvent, foundCommand, commandSuccessful, _) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.False(commandSuccessful);
            Assert.False(didDebug);
            didDebug = false;
        }

        [Fact]
        public async void CheckServiceInjection()
        {
            var neither = new CommandManagerTests.DebugEvent("!injectMe");
            Assert.True(EQ.AddEvent(neither).Result);
            var (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(InjectThisClass.injected);
            Assert.True(CommandTestMeOut.DisposedTest);
            didDebug = false;
        }

        [Fact]
        public async void MultiStep()
        {
            var even = new CommandManagerTests.DebugEvent("!testMe");
            var odd = new CommandManagerTests.DebugEvent("!ignoreMe");
            var neither = new CommandManagerTests.DebugEvent("!injectMe");
            Assert.True(await EQ.AddEvent(even));
            Assert.True(await EQ.AddEvent(odd));
            Assert.True(await EQ.AddEvent(odd));
            Assert.True(await EQ.AddEvent(even));
            Assert.True(await EQ.AddEvent(neither));
            var (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(didDebug);
            (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.False(didDebug);
            (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.False(commandSuccessful);
            Assert.False(didDebug);
            (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(didDebug);
            (foundEvent, foundCommand, commandSuccessful, failureReason) = EQ.DoEvent();
            Assert.NotNull(foundEvent);
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(InjectThisClass.injected);
            Assert.True(CommandTestMeOut.DisposedTest);
        }

        public EventQueueTests()
        {
            List<int> ConstTestAgainst = new List<int>();
            for (int i = 0; i < countTestTestAgainst; i++)
                ConstTestAgainst.Add(countTestTestAgainst);
            var provider = new ServiceCollection().AddSingleton(ConstTestAgainst)
                .AddTransient<IInjectedTester, InjectThisClass>().BuildServiceProvider();
            
                

            EQ = new EventQueue<CommandManager<CommandContext>, VersaCommand, CommandContext, CommandContext.Builder>(
            new CommandManager<CommandContext>("Test commands", "!")
            {
                new VersaCommand.Builder()
                {
                    Name = "Test Command",
                    CallLine = "testMe",
                    Description = "This code will fail at runtime without a defined description for each command.",
                    Predicates = new System.Collections.Generic.List<RulesOfNature.IConditionalRule>(),
                    Do = (c) => Task.Run(() => didDebug = true),
                }.Build(),
                new VersaCommand.Builder()
                {
                    Name = "Test Command2",
                    CallLine = "ignoreMe",
                    Description = "This code will fail at runtime without a defined description for each command.",
                    Predicates = new System.Collections.Generic.List<RulesOfNature.IConditionalRule>(){
                        new RulesOfNature.LambdaRule("DebugFalse", "debugIsFalse", (c,s) => Task.FromResult((didDebug, "didDebug is already false" )))
                    },
                    Do = (c) => Task.Run(() => didDebug = false),
                }.Build(),
                new VersaCommand.Builder()
                {
                    Name = "Injection Test",
                    CallLine = "injectMe",
                    Description = "Tests how this works with a generated class instead.",
                    Predicates = new List<RulesOfNature.IConditionalRule>(),

                }.UsingTransientClass<CommandTestMeOut>(c => c.TestInjectedWorked).Build()

            });
        }
    } 
} */
