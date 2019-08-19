namespace Versagen.Logging
{
    public enum EDebugSeverity : byte
    {
        Trace,
        Info,
        Warning,
        Error,
        Critical,
        Armageddon
    }

    /// <summary>
    /// <para>Logs the different </para>
    /// </summary>
    public interface IVersaLogListener
    {
        void Write(object msg, EDebugSeverity severity);
        void WriteLine(object msg, EDebugSeverity severity);
    }
}
