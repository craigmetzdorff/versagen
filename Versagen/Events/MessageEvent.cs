using System;
using System.Collections.Generic;
using System.Text;
using Versagen.Utils;
using Versagen.Entity;
using Versagen.Events.Commands;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Scenarios;

namespace Versagen.Events
{
    public class MessageEvent :IMessageEvent
    {
        public bool IgnoreThis { get; set; }
        public bool IsSystemMessage { get; set; }
        public IEventPipe SourcePipe { get; set; }
        public IServiceProvider Services { get; set; }
        public UnionType<VersaCommsID, IScenario> Scenario { get; set; }
        public UnionType<VersaCommsID, IVersaWriter> Terminal { get; set; }
        public string FullMessage { get; set; }
        public UnionType<VersaCommsID, IPlayer> Player { get; set; }
        public UnionType<VersaCommsID, IEntity> Entity { get; set; }
        public IList<ICommandGroup> EventSpecificCommands { get; set; } = new List<ICommandGroup>();
    }
}
