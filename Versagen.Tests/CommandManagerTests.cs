using System;
using Xunit;
using System.Threading.Tasks;
using Jolt.Utils;
using Versagen.Entity;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Scenarios;

namespace Versagen.Tests
{
    public class CommandManagerTests
    {
        public class DebugEvent : IEvent
        {
            public bool IgnoreThis { get; set; }
            public IEventPipe SourcePipe { get; set; }
            public IServiceProvider Services { get; set; }
            public UnionType<VersaCommsID, IScenario> Scenario { get; set; }
            public UnionType<VersaCommsID, IVersaWriter> Terminal { get; set; }
            public string FullMessage { get; set; }
            public UnionType<VersaCommsID, IPlayer> Player { get; set; }


            public UnionType<VersaCommsID, IEntity> Entity { get; set; }

            public DebugEvent(string FullMessage)
            {
                this.FullMessage = FullMessage;
            }
        }

        bool firstTestBool = false;

        CommandManager<CommandContext> BasicManager { get; }


        [Fact]
        public void CanFindAndExecuteCommands()
        {
            bool didDebug = false;

            System.Collections.Generic.List<CommandManager<ICommandContext>> managers = new System.Collections.Generic.List<CommandManager<ICommandContext>>();

            var cManager = new CommandManager<CommandContext>("Test commands", "!")
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
                    Predicates = new System.Collections.Generic.List<RulesOfNature.IConditionalRule>(),
                    Do = (c) => Task.Run(() => didDebug = false),
                }.Build()

            };
            cManager.FindCommand(new DebugEvent("!testMe")).Item1.Run(new CommandContext.Builder().Build(),default(IServiceProvider)).Wait();

            Assert.True(didDebug);
        }


        public CommandManagerTests()
        {

        }
    }
}
