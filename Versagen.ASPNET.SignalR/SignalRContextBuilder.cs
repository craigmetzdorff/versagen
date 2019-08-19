using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.Rules;
using Microsoft.Extensions.DependencyInjection;
using Versagen.PlayerSystem;

namespace Versagen.ASPNET.SignalR
{
    [Obsolete]
    public class SignalRContextBuilder<THub> : ICommandContextFactory<CommandContext, CommandContext.Builder, IMessageEvent, VersaCommand> where THub:Hub
    {

        IVersaWriter broadcaster;
        IList<IPlayer> players;
        private string functionName;
        private IServiceProvider backupProvider;

        public (CommandContext.Builder builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs) ConfigureBuilder(IMessageEvent @event, VersaCommand command)
        {
            //TODO:the serviceprovider attached to the event may get disposed. Must ensure that a working backup can be returned.

            var services = @event.Services;
            IHubContext<THub> context;
            try
            {
                 context = services.GetRequiredService<IHubContext<THub>>();
            }
            catch(ObjectDisposedException)
            {
                services = backupProvider;
                context = services.GetRequiredService<IHubContext<THub>>();
            }
            
            var authenticator = services.GetRequiredService<AuthTranslator<string>>();
            IVersaWriter useTerm;
            try
            {
                var tsk1 = authenticator.GetExternalID(@event.GetPlayerID());
                tsk1.Wait();
                useTerm = new VersaSignalRWriter(@event.GetPlayerID(),
                    context.Clients.User(tsk1.Result), functionName, services.GetRequiredService<HtmlEncoder>());
            }
            catch
            {
                try
                {
                    var tsk1 = authenticator.GetExternalID(@event.GetPlayerID());
                    tsk1.Wait();
                    useTerm = new VersaSignalRWriter(@event.GetPlayerID(),
                        context.Clients.Client(tsk1.Result), functionName,
                        services.GetRequiredService<HtmlEncoder>());
                }
                catch
                {
                    useTerm = broadcaster;
                }
            }

            IPlayer playerHandlerFunction(VersaCommsID id)
            {
                IPlayer player;
                if (players.Any(t => t.VersaID == @event.GetPlayerID()))
                    player = players.First(t => @event.GetPlayerID() == t.VersaID);
                else
                {
                    player = new Player(@event.GetPlayerID());
                    players.Add(player);
                }

                return player;
            }


            
            var b = new CommandContext.Builder
            {
                Message = @event.FullMessage,
                GMTerm = broadcaster, //TODO: fix this later on to ensure that the system checks if it's in a scenario.
                UserTerm = useTerm,
                OriginTerm = 
                    @event.Terminal.As(new VersaCommsID(0)) == default ?
                        new VersaSignalRWriter(@event.Terminal.As<VersaCommsID>(default), context.Clients.User(new Func<string>(() =>
                        {
                            var tsk = authenticator.GetExternalID(@event.Terminal.As<VersaCommsID>(default));
                            tsk.Wait();
                            return tsk.Result;
                        }).Invoke()), functionName, services.GetRequiredService<HtmlEncoder>()):
                broadcaster,
                ScenarioID = broadcaster.DestinationID,
                Command = command,
                User = @event.GetPlayer(playerHandlerFunction)
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

        public SignalRContextBuilder(IServiceProvider backupProvider, string functionName, IVersaWriter broadcaster)
        {
            this.backupProvider = backupProvider;
            this.functionName = functionName;
            this.broadcaster = broadcaster;
            players = new List<IPlayer>();

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
