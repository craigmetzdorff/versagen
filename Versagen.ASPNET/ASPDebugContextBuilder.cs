using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Rules;
using Versagen.World;

namespace Versagen.ASPNET
{
    public class ASPDebugContextBuilder : ICommandContextFactory<CommandContext,CommandContext.Builder,IMessageEvent,VersaCommand>
    {

        

        protected IVersaWriter broadcaster;

        protected IServiceProvider Provider;

        public (CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs) ConfigureBuilder(IMessageEvent @event, VersaCommand command)
        {
            var b = new CommandContext.Builder
            {
                Message = @event.FullMessage,
                GMTerm = broadcaster,
                UserTerm = broadcaster,
                OriginTerm = broadcaster,
                ScenarioID = broadcaster.DestinationID,
                Command = command
            };
            return (b, new Func<Task<(bool, IConditionalRule, string)>>[] { });
        }

        public Task<(CommandContext.Builder builder,
                Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)>
            ConfigureBuilderAsync(IMessageEvent @event, VersaCommand command)
        {
            var b = new CommandContext.Builder
            {
                Message = @event.FullMessage,
                GMTerm = broadcaster,
                UserTerm = broadcaster,
                OriginTerm = broadcaster,
                ScenarioID = broadcaster.DestinationID,
                Command = command
            };
            return Task.FromResult((b, new Func<Task<(bool, IConditionalRule, string)>>[] { }));
        }

        public Task<CommandContext.Builder> FetchParameterInfo(CommandContext.Builder builder, VersaCommand command)
        {
            throw new NotImplementedException();
        }

        public (CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs) ConfigureBuilder(IMessageEvent @event)
        {
            var b = new CommandContext.Builder
            {
                Message = @event.FullMessage,
                GMTerm = broadcaster,
                UserTerm = broadcaster,
                OriginTerm = broadcaster,
                ScenarioID = broadcaster.DestinationID,
            };
            return (b, new Func<Task<(bool, IConditionalRule, string)>>[] { });
        }

        public Task<(CommandContext.Builder builder,
                Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)>
            ConfigureBuilderAsync(IMessageEvent @event)
        {
            var b = new CommandContext.Builder
            {
                Message = @event.FullMessage,
                GMTerm = broadcaster,
                UserTerm = broadcaster,
                OriginTerm = broadcaster,
                ScenarioID = broadcaster.DestinationID,
            };
            return Task.FromResult((b, new Func<Task<(bool, IConditionalRule, string)>>[] { }));
        }


        public ASPDebugContextBuilder(IServiceProvider provider, IVersaWriter broadcaster)
        {
            this.broadcaster = broadcaster;
            Provider = provider;
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
            additionalConditionFuncs)> ConfigureContextAsync(IMessageEvent @event,
            IVersaCommand<CommandContext> command,
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
