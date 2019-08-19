using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Versagen.Structs;

namespace Versagen.IO
{

    public interface IVersaGroupWriter : IVersaWriter
    {
        VersaCommsID[] Subscribed { get; }
        (int count, Exception exception) Subscribe(params VersaCommsID[] Ids);
        (int count, Exception exception) UnSubscribe(params VersaCommsID[] Ids);
    }

    //TODO: Message category system; ie. use strings to indicate types of messages (and perhaps an enum with more common types that translates the names into string values)
    public interface IVersaWriter : IDisposable, IDeepCloneable<IVersaWriter>
    {

        bool ColorSupport { get; }


        
        VersaCommsID DestinationID { get; }
        /// <summary>
        /// Creates a clone of this writer using the specified preamble.
        /// </summary>
        /// <param name="newPreamble"></param>
        /// <returns></returns>
        IVersaWriter WithPreamble(string newPreamble);
        string DefaultPreamble { get; set; }
        Task WritePreambleAsync();
        Task WritePreambleAsync(string PreambleContents);
        Task WriteAsync(string message, Color color);
        Task WriteAsync(string message);
        Task WriteLineAsync(string message, Color color);
        Task WriteLineAsync(string message);

    }
}
