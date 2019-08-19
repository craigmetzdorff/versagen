using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Versagen.Events
{
    public interface IKeepEventAlive : IAwaitable, IDisposable
    {
        /// <summary>
        /// If true, keep event alive after passing it back into the queue. Otherwise signal completion to all awaiters. Should be reset to false after every failed disposal attempt.
        /// </summary>
        bool HoldOffOnCompletion { get; set; }
        bool IsComplete { get; }
        void DoComplete();
    }
}
