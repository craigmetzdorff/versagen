using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Versagen.Entity;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.Locations;
using Versagen.Logging;
using Versagen.Scenarios;
using Versagen.Structs;
using Versagen.Utils;

namespace Versagen
{
    public static class DefaultVersagenServiceConfigExtensions
    {
        public static VersagenServiceConfig UseDefaults(this VersagenServiceConfig config)
        {
            config.VersagenServices
                .AddTransient<ICommandContextFactory<CommandContext, CommandContext.Builder, IMessageEvent,
                        IVersaCommand<CommandContext>>,
                    DefaultCommandContextBuilderFactory<CommandContext, CommandContext.Builder, IMessageEvent,
                        IVersaCommand<CommandContext>>>();
            config.VersagenServices
                .AddTransient<ICommandContextFactory<CommandContext>, DefaultCommandContextBuilderFactory<CommandContext
                    ,
                    CommandContext.Builder, IMessageEvent, IVersaCommand<CommandContext>>>();
            config.ScenarioServices.TryAddSingleton<RootLocationHolder, RootLocationHolder<Locations.Location>>();
            bool hadNoEnts;
            if (
                ((hadNoEnts = config.ScenarioServices.All(c => c.ServiceType != typeof(IEntityStore))) ||
                              config.ScenarioServices.First(c => c.ServiceType == typeof(IEntityStore))
                                  .ImplementationType == typeof(EntityStore)) && ((hadNoEnts = hadNoEnts &&
                     config.VersagenServices.All(c => c.ServiceType != typeof(IEntityStore)))
                    || config.VersagenServices.First(c => c.ServiceType == typeof(IEntityStore)).ImplementationType == typeof(EntityStore)))
            {
                var defaultStore = new EntityStore.DefaultEntityStorePermanentBacking();
                config.ScenarioServices.AddSingleton(defaultStore);
                config.VersagenServices.AddSingleton(defaultStore);
                config.ScenarioServices.AddSingleton(p =>
                    new EntityStore(p.GetRequiredService<EntityStore.DefaultEntityStorePermanentBacking>()
                        .PermanentEntities, true));
                config.VersagenServices.AddSingleton(p =>
                    new EntityStore(p.GetRequiredService<EntityStore.DefaultEntityStorePermanentBacking>()
                        .PermanentEntities, false));
                config.ScenarioServices.AddTransient<IEntityStore<Entity.Entity>>(p =>
                    p.GetRequiredService<EntityStore>());
                config.VersagenServices.AddTransient<IEntityStore<Entity.Entity>>(p =>
                    p.GetRequiredService<EntityStore>());
                if (hadNoEnts)
                {
                    config.ScenarioServices.AddTransient<IEntityStore>(p => p.GetRequiredService<EntityStore>());
                    config.VersagenServices.AddTransient<IEntityStore>(p => p.GetRequiredService<EntityStore>());
                }
            }
            config.VersagenServices.AddSingleton<IScenarioEventDistributor<MessageEvent>, VersaScenarioDistributor>();
            config.SetupCommandService<CommandService>("!");
            config.ScenarioServices.AddSingleton<IEventPipe, EventPipe>();
            config.ScenarioServices.AddSingleton<RootLocationHolder<Locations.Location>>();
            config.ScenarioServices.AddTransient<RootLocationHolder>(p =>
                p.GetRequiredService<RootLocationHolder<Locations.Location>>());
            config.TryConfigureScenarioServiceScopeFactory<DefaultScenarioScopeFactory>();
            config.VersagenServices.AddTransient<IVersaLogger>(p => Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<DefaultVersaLogger>(p,
#if DEBUG
                EDebugSeverity.Info
#else
                EDebugSeverity.Error
#endif
            ));
            config.VersagenServices.AddTransient<IVersaLogListener, VersaConsoleLogListener>();
            config.VersagenServices.AddTransient<IVersaLogListener, VersaDebugLogListener>();
            config.VersagenServices.AddTransient<MessageRepeater>();
            return config;
        }
    }
}
