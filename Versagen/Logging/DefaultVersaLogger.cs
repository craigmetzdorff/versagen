using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Versagen.Logging
{
    public class DefaultVersaLogger : IVersaLogger
    {
        public EDebugSeverity MinimumSeverity { get; set; }
        private readonly Action<object, EDebugSeverity> _printAction;
        private readonly Action<object, EDebugSeverity> _printlnAction;

        public void Write(object msg, EDebugSeverity severity)
        {
            if (severity >= MinimumSeverity)
                _printAction(msg, severity);
        }

        public void WriteLine(object msg, EDebugSeverity severity)
        {
            _printlnAction(msg, severity);
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            _printlnAction(e.Exception, EDebugSeverity.Error);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _printlnAction("Unhandled exception! " + e.ExceptionObject,
                e.IsTerminating ? EDebugSeverity.Armageddon : EDebugSeverity.Critical);
        }

        /// <summary>
        /// This can work because it won't be registered with this interface.
        /// </summary>
        /// <param name="loggers"></param>
        public DefaultVersaLogger(IEnumerable<IVersaLogListener> loggers)
        {
            var versaLogListeners = loggers as IVersaLogListener[] ?? loggers.ToArray();
            if (versaLogListeners.Any())
            {
                _printAction = versaLogListeners.Skip(1).Aggregate<IVersaLogListener, Action<object, EDebugSeverity>>(versaLogListeners.First().Write, (a, b) => a + b.Write);
                _printlnAction = versaLogListeners.Skip(1).Aggregate<IVersaLogListener, Action<object, EDebugSeverity>>(versaLogListeners.First().WriteLine, (a, b) => a + b.WriteLine);
            }
            else
            {
                _printAction = (a,b) => { };
                _printlnAction = (a,b) => { };
            }

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
    }
}
