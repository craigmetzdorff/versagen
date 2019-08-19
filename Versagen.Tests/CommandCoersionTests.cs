using System;
using System.Collections.Generic;
using Xunit;
using Versagen.Events.Commands;
using System.Threading.Tasks;
using Versagen.Events;

namespace Versagen.Tests
{
    public class CommandCoersionTests
    {
        bool didDebug = false;
        readonly EventQueue<CommandManager<CommandContext>, IVersaCommand<CommandContext>, CommandContext, CommandContext.Builder> EQ;


        /// <summary>
        /// TODO: FINISH THIS
        /// </summary>
        public async void TestRunMassConversion()
        {
            var even = new CommandManagerTests.DebugEvent("!testMe");
            var odd = new CommandManagerTests.DebugEvent("!ignoreMe");
            Assert.True(await EQ.AddEvent(even));
            Assert.True(await EQ.AddEvent(odd));
            Assert.True(await EQ.AddEvent(odd));
            Assert.True(await EQ.AddEvent(even));
            var (_, foundCommand, commandSuccessful, _, commandTask) = EQ.StartEvent();
            await commandTask;
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(didDebug);
            (_, foundCommand, commandSuccessful, _, commandTask) = EQ.StartEvent();
            await commandTask;
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.False(didDebug);
            (_, foundCommand, commandSuccessful, _, commandTask) = EQ.StartEvent();
            await commandTask;
            Assert.True(foundCommand);
            Assert.False(commandSuccessful);
            Assert.False(didDebug);
            (_, foundCommand, commandSuccessful, _, commandTask) = EQ.StartEvent();
            await commandTask;
            Assert.True(foundCommand);
            Assert.True(commandSuccessful);
            Assert.True(didDebug);
        }


        internal void DoImplicitConversion(VersaCommandBase<CommandContext> command)
        {
            command.Run(new CommandContext.Builder().Build(), default(IServiceProvider)).Wait();
        }

        internal void DoubleCastCrais(VersaCommand command)
        {
            command.Run(new CommandContext.Builder().Build(), default(IServiceProvider)).Wait();
        }

        internal void AHoop(IVersaCommand<ICommandContext> command)
        {
            command.Run(new CommandContext.Builder().Build(), default(IServiceProvider)).Wait();
            Assert.True(iRan);
            iRan = false;
            DoImplicitConversion(VersaCommandBase<CommandContext>.GetFromInterface(command).casted);
            Assert.True(iRan);
            iRan = false;
            DoubleCastCrais((VersaCommand)VersaCommandBase<CommandContext>.GetFromInterface(command).casted);
            Assert.True(iRan);
        }

        bool iRan = false;

        [Fact]
        public void TestImplicitConversion()
        {
            
            var commanded = new VersaCommand.Builder()
            {
                Name = "TestMe",
                CallLine = "testMe",
                Description = "Let's see if I convert right.",
                Predicates = new List<RulesOfNature.IConditionalRule>(),
                Do = c => Task.Run(() => iRan = true)
            }.Build();

            AHoop(commanded.WithICommandContext());

        }



    } 
}
