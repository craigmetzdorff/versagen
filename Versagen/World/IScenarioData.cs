using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.World
{
    /// <summary>
    /// Describes the basic state of the Scenario to the outside for purposes such as authentication, etc.
    /// Might need more later, but not sure. Should be accessible from the IScenarioManager later on.
    /// </summary>
    public interface IScenarioData
    {
        VersaCommsID ID { get; }
        string ScenarioName { get; }
        VersaCommsID[] PlayerChars { get; }
        VersaCommsID GameMaster { get; }
    }
}
