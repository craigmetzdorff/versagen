using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.RulesOfNature;

namespace Versagen.SampleConsoleOne
{
    public class ConsoleContextBuilder :ICommandContextFactory<CommandContext,CommandContext.Builder,IEvent,VersaCommand>
    {

        public IO.IVersaWriter originWriter = new VersaConsoleWriter("Unused", ConsoleColor.DarkRed);

        public IO.IVersaWriter userWriter = new VersaConsoleWriter("Unused", ConsoleColor.Blue);

        public IO.IVersaWriter gmWriter = new VersaConsoleWriter("Unused", ConsoleColor.DarkGreen);

        public IServiceProvider provider;

        public string prefix;

        public (CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(IEvent @event, VersaCommand command)
        {
            var starter = ConfigureBuilder(@event);
            return starter;
        }

        public Task<(CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(IEvent @event, VersaCommand command)
        {
            return Task.FromResult(ConfigureBuilder(@event, command));
        }

        public Task<CommandContext.Builder> FetchParameterInfo(CommandContext.Builder builder, VersaCommand command)
        {
            return Task.FromResult(builder);
        }

        public (CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(IEvent @event)
        {
            var builder = new CommandContext.Builder
            {
                Message = @event.FullMessage,
                OriginTerm = originWriter,
                UserTerm = userWriter,
                GMTerm = gmWriter
            };
            return (builder, new Func<Task<(bool, IConditionalRule, string)>>[] { });
        }

        public Task<(CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(IEvent @event)
        {
            return Task.FromResult(ConfigureBuilder(@event));
        }

        public ConsoleContextBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public (CommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureContext(IEvent @event)
        {
            throw new NotImplementedException();
        }

        public (CommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs) ConfigureContext(IEvent @event, IVersaCommand<CommandContext> command,
                string matchedCommandLine)
        {
            throw new NotImplementedException();
        }

        public (CommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureContext(IEvent @event, IVersaCommand command)
        {
            throw new NotImplementedException();
        }

        public Task<(CommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureContextAsync(IEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task<(CommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs)> ConfigureContextAsync(IEvent @event, IVersaCommand<CommandContext> command,
            string matchedCommandLine)
        {
            throw new NotImplementedException();
        }

        public Task<(CommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureContextAsync(IEvent @event, IVersaCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
