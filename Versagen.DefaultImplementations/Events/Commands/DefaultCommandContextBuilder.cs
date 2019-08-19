using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Versagen.Rules;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Data;
using Versagen.Entity;
using Versagen.IO;
using Versagen.Scenarios;

namespace Versagen.Events.Commands
{
    public class DefaultCommandContextBuilderFactory<A,B,E,C> :ICommandContextFactory<A,B,E,C> where A : ICommandContext
        where B : ICommandContextBuilder<A>, new()
        where E : IMessageEvent
        where C : IVersaCommand<A> 
    {
        public (B builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(E @event, C command) => ConfigureBuilderAsync(@event, command).GetAwaiter().GetResult();

        public async Task<(B builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(E @event, C command)
        {
            var intermediate = await ConfigureBuilderAsync(@event);
            intermediate.builder.Command = command;
            return intermediate;
        }

        public Task<B> FetchParameterInfo(B builder, C command)
        {
            throw new NotImplementedException();
        }

        public (B builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(E @event) => ConfigureBuilderAsync(@event).GetAwaiter().GetResult();

        public async Task<(B builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(E @event)
        {
            var writeDir = @event.Services.GetRequiredService<IVersaWriterDirectory>();
            var builder = new B();
            builder.Pipe = @event.SourcePipe;
            if (@event.Terminal.As<VersaCommsID>(default) != default)
                builder.OriginTerm = writeDir.GetWriter(@event.Terminal.As<VersaCommsID>(default));
            else if (@event.Terminal.Obj is IVersaWriter ot)
                builder.OriginTerm = ot;
            builder.UserTerm = writeDir.GetWriter(@event.GetPlayerID());
            builder.ScenarioID = @event.Scenario.As<VersaCommsID>(default);
            builder.Message = @event.FullMessage;
            if (@event.Player.Obj != null)
                builder.User = await Task.Run(()=> @event.GetPlayer(id => @event.Services.GetRequiredService<IPlayerStore>().GetPlayerAsync(id).Result));
            if (@event.Entity.Obj != null)
                builder.ActingEntity = @event.GetEntity(id => @event.Services.GetRequiredService<IEntityStore<Entity.Entity>>().Entities.First(c => c.Id == id));
            return (builder, new Func<Task<(bool, IConditionalRule, string)>>[0]);
        }

        public (A context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureContext(IEvent @event)
        {
            var intermediate = ConfigureBuilder((E)@event);
            return (intermediate.builder.Build(), intermediate.additionalConditionFuncs);
        }

        public (A context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs) ConfigureContext(IEvent @event, IVersaCommand<A> command,
                string matchedCommandLine)
        {
            throw new NotImplementedException();
        }

        public Task<(A context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureContextAsync(IEvent @event)
        {
            throw new NotImplementedException();
        }

        public async
            Task<(A context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
                additionalConditionFuncs)> ConfigureContextAsync(IMessageEvent @event, IVersaCommand<A> command,
                string matchedCommandLine)
        {
            var writeDir = @event.Services.GetRequiredService<IVersaWriterDirectory>();
            var builder = new B();
            builder.Pipe = @event.SourcePipe;
            if (@event.Terminal.As<VersaCommsID>(default) != default)
                builder.OriginTerm = writeDir.GetWriter(@event.Terminal.As<VersaCommsID>(default));
            else if (@event.Terminal.Obj is IVersaWriter ot)
                builder.OriginTerm = ot;
            builder.CommandString = matchedCommandLine;
            builder.UserTerm = writeDir.GetWriter(@event.GetPlayerID());
            builder.ScenarioID = @event.Scenario.As<VersaCommsID>(default);
            builder.Message = @event.FullMessage;
            if (@event.Player.Obj != null)
                builder.User = await Task.Run(() => @event.GetPlayer(id => @event.Services.GetRequiredService<IPlayerStore>().GetPlayerAsync(id).Result));
            if (@event.Entity.Obj != null)
                builder.ActingEntity = @event.GetEntity(id => @event.Services.GetRequiredService<IEntityStore<Entity.Entity>>().Entities.First(c => c.Id == id));
            builder.Command = command;
            return (builder.Build(), new Func<Task<(bool, IConditionalRule, string)>>[0]);
        }
    }
}
