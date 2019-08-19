using System.Diagnostics;

namespace Versagen.Logging
{
    public class VersaDebugLogListener :IVersaLogListener
    {
        public void Write(object msg, EDebugSeverity severity)
        {
            Debug.Write(msg);
        }
        public void WriteLine(object msg, EDebugSeverity severity)
        {
            Debug.WriteLine(msg);
        }
    }
}
