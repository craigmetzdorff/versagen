using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Versagen;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Scenarios;

namespace Versagen.ASPNET
{



    public static class VersaASPConfigExtensions
    {
        public static VersagenServiceConfig AddASPSupport(this VersagenServiceConfig config)
        {
            
            return config;
        }

        //TODO: Remove this later.
        [Obsolete("Uses old debug methods and is not suitable for new versions of Versagen.", true)]
        public static VersagenServiceConfig ASPBasicSupport(this VersagenServiceConfig collection, Func<IServiceProvider, IVersaWriter> broadcastChatBuilder)
        {
            collection.VersagenServices.AddSingleton<IScenarioEventDistributor<MessageEvent>, VersaScenarioDistributor>();
                collection.VersagenServices
                .TryAddSingleton<ICommandContextFactory<CommandContext,CommandContext.Builder,IMessageEvent,VersaCommand>> (
                    provider =>
                        new ASPDebugContextBuilder(provider, broadcastChatBuilder(provider))
                );
                collection.VersagenServices.TryAddSingleton<AuthTranslator<string>, DEBUGTextIDAuthenticator>();
            return collection;
        }
    }
}
