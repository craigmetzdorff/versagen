using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Versagen.Utils;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Data;
using Versagen.Entity;
using Versagen.IO;
using Versagen.Locations;
using Versagen.Scenarios;
using System.Diagnostics;
using System.Linq;

namespace Versagen.Events
{
    public class EventObjectFetcher : IObserver<IEvent, IEvent>
    {
        private readonly IScenario _defaultScenario;

        public virtual IEvent GetObjectsFromIDs(IEvent value)
        {
            var writers = value.Services.GetRequiredService<IVersaWriterDirectory>();
            if (value.Entity.Obj is VersaCommsID)
                value.GetEntity(value.Services.GetRequiredService<IEntityStore>().GetEntity);
            if (value.Player.Obj is VersaCommsID)
                value.GetPlayer(value.Services.GetRequiredService<IPlayerStore>().GetPlayer);
            if (value.Terminal.Obj is VersaCommsID termId)
                value.Terminal = new UnionType<VersaCommsID, IVersaWriter>(writers.GetWriter(termId));
            if (value.Scenario.Obj is VersaCommsID)
                value.Scenario = new UnionType<VersaCommsID, IScenario>(_defaultScenario);
            return value;
        }

        public virtual IEvent HookReactions(IEvent value)
        {
            value.GetEntity<IEntity>(default).Reactions.ForEach(r => value.SourcePipe.ObserveFirst.Subscribe(r));

            return value;
        }

        public virtual IEvent PopulateEventCommands(IEvent value)
        {
            if (!(value is IMessageEvent me)) return value; //Because the command service won't run without a message command anyway.
            Debug.Write("Checking " + me.FullMessage);
            if (value.Entity.Obj == default)
                return value;
            var ent = value.GetEntity(value.Services.GetRequiredService<IEntityStore>().GetEntity);
            value.EventSpecificCommands.Add(ent.CurrentLocation.LocalizedCommands);
            value.EventSpecificCommands.Add(ent.ActAsCommands);
            return value;
        }

        public virtual IEvent OnNext(IEvent value) => PopulateEventCommands(GetObjectsFromIDs(value));

        public virtual IEvent OnError(Exception exception) => throw new AggregateException(exception, new NotImplementedException());

        public virtual IEvent OnCompleted()
        {
            return null;
        }

        public EventObjectFetcher(IScenario scenario)
        {
            _defaultScenario = scenario;
        }
    }
}
