using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Versagen.IO;
using Versagen.Structs;

namespace Versagen.ASPNET
{
    public class LambdaWriter : IVersaWriter
    {
        public void Dispose()
        {
            
        }

        Func<string, Color?, Task> MessageFunc { get; }

        public bool ColorSupport { get; set; } = true;
        public VersaCommsID DestinationID { get; set; } = uint.MaxValue;
        public IVersaWriter WithPreamble(string newPreamble)
        {
            throw new NotImplementedException();
        }

        public string DefaultPreamble { get; set; }

        public Task WritePreambleAsync()
        {
            throw new NotImplementedException();
        }

        public Task WritePreambleAsync(string PreambleContents)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string message, Color color)
            => MessageFunc(message, color);

        public Task WriteAsync(string message)
            =>
                MessageFunc(message, null);

        public Task WriteLineAsync(string message, Color color)
            =>
                MessageFunc(message + Environment.NewLine, color);

        public Task WriteLineAsync(string message)
            => MessageFunc(message + Environment.NewLine, null);


        public LambdaWriter(Func<string, Color?, Task> messagePredicate)
        {
            MessageFunc = messagePredicate;
        }

        public IVersaWriter DeepClone(bool requireNewRef = false)
        {
            throw new NotImplementedException();
        }

        object IDeepCloneable.DeepClone(bool requireNewRef)
        {
            return DeepClone(requireNewRef);
        }
    }
}
