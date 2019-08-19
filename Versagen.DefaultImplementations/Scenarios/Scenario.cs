using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Events;
using Versagen.Events.Commands;
using Versagen.Locations;
using Versagen.PlayerSystem;

namespace Versagen.Scenarios
{
    public class Scenario :IScenario
    {
        IScenarioScope scScope { get; }
        private ICommandService comServ { get; }
        public bool IsCompleted { get; protected set; } = false;
        public VersaCommsID ScenarioID { get; }
        public Task ProcessEvent(IEvent e)
        {
            e.Services = scScope.GetScenarioServiceProvider(e.Services);
            return !tokenSource.IsCancellationRequested ? Pipe.ProcessEvent(e) : Task.CompletedTask;
        }

        public ImmutableArray<IPlayer> GMs { get; }

        

        public IEventPipe Pipe { get; }

        public CancellationTokenSource tokenSource { get; protected set; }

        public Scenario(VersaCommsID scenarioId, IServiceProvider provider)
        {
            ScenarioID = scenarioId;
            tokenSource = new CancellationTokenSource();
            scScope = provider.GetRequiredService<IScenarioScopeFactory>()
                .ConfigureScenarioServices(scenarioId);
            GMs = new ImmutableArray<IPlayer>();
            var tempProv = scScope.GetScenarioServiceProvider(provider);
            Pipe = tempProv.GetRequiredService<IEventPipe>();
            Pipe.SubscribePreprocess(new EventObjectFetcher(this));
            comServ = tempProv.GetRequiredService<ICommandService>();
            var repeater = tempProv.GetRequiredService<MessageRepeater>();
            Pipe.ObserveSynchronous.Subscribe(comServ);
            Pipe.ObserveFirst.Subscribe(repeater);
            tempProv.GetService<RootLocationHolder>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                tokenSource?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
