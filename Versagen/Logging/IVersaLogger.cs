using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Logging
{
    public interface IVersaLogger:IVersaLogListener
    {
        EDebugSeverity MinimumSeverity { get; set; }
    }
}
