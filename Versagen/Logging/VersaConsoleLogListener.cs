using System;

namespace Versagen.Logging
{
    public class VersaConsoleLogListener :IVersaLogListener
    {
        public void Write(object msg, EDebugSeverity severity)
        {
            Console.Write(msg);
        }

        public void WriteLine(object msg, EDebugSeverity severity)
        {
            Console.WriteLine(msg);
        }
    }
}
