using System;
using System.Threading.Tasks;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    public interface ICommandContextFactory<TContext,TBuilder, in TEvent, in TCommand> :ICommandContextFactory<TContext>
        where TContext:ICommandContext
        where TBuilder:ICommandContextBuilder<TContext>
        where TEvent:IMessageEvent
        where TCommand:IVersaCommand<TContext>
    {
        (TBuilder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(TEvent @event, TCommand command);
        Task<(TBuilder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(TEvent @event, TCommand command);

        Task<TBuilder> FetchParameterInfo(TBuilder builder, TCommand command);
        (TBuilder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(TEvent @event);
        Task<(TBuilder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(TEvent @event);

    }

    public interface ICommandContextFactory<TContext> where TContext : ICommandContext
    {
        (TContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureContext(IEvent @event);
        (TContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)
            ConfigureContext(IEvent @event, IVersaCommand<TContext> command, string matchedCommandLine);
        Task<(TContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureContextAsync(IEvent @event);
        Task<(TContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)>
            ConfigureContextAsync(IMessageEvent @event, IVersaCommand<TContext> command, string matchedCommandLine);

    }
}
