using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Versagen.Events;

namespace Versagen.Scenarios
{
    public interface IScenario :IDisposable
    {
        bool IsCompleted { get; }
        VersaCommsID ScenarioID { get; }
        Task ProcessEvent(IEvent e);
    }
}
