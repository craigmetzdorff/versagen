using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Versagen.IO;
using Versagen.Structs;
using Microsoft.AspNetCore.SignalR;

namespace Versagen.ASPNET.SignalR
{
    public class VersaSignalRWriter : IVersaWriter
    {
        public void Dispose()
        {
            if (midstatement)
                WriteLineAsync("");
        }

        HtmlEncoder Encoder { get; }

        protected IClientProxy proxy { get; }
        protected string functionName { get; }

        public bool ColorSupport { get; set; }

        public VersaCommsID DestinationID { get; set; }
        public IVersaWriter WithPreamble(string newPreamble)
        {
            var clone = (VersaSignalRWriter)MemberwiseClone();
            clone.DefaultPreamble = newPreamble;
            return clone;
        }

        public string DefaultPreamble { get; set; } = "Versagen: ";

        public Task WritePreambleAsync() => WritePreambleAsync(DefaultPreamble);

        public Task WritePreambleAsync(string contents)
        {
            midstatement = true;
            return proxy.SendAsync(functionName, Encoder.Encode(contents), null, false);
        }

        bool midstatement;

        private async Task WriteDelegate(string message, Color? color, bool withNewLine)
        {
            if (!midstatement)
            {
                await WritePreambleAsync().ConfigureAwait(false);
            }
            

            var splits = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var split in splits.SkipLast(1))
                await proxy.SendAsync(functionName, Encoder.Encode(split), color?.ToStringCssRGBA(), true)
                    .ConfigureAwait(false);
            await proxy.SendAsync(functionName, Encoder.Encode(splits[splits.Length-1]), color?.ToStringCssRGBA(), withNewLine).ConfigureAwait(false);
            midstatement = !withNewLine;
        }

        public Task WriteAsync(string message, Color color) => WriteDelegate(message, color, false);

        public Task WriteAsync(string message) => WriteDelegate(message, null, false);

        public Task WriteLineAsync(string message, Color color) => WriteDelegate(message, color, true);

        public Task WriteLineAsync(string message) => WriteDelegate(message, null, true);

        public VersaSignalRWriter(VersaCommsID destID, IClientProxy sender, string functionName, HtmlEncoder encoder)
        {
            DestinationID = destID;
            proxy = sender;
            this.functionName = functionName;
            Encoder = encoder;
        }

        public IVersaWriter DeepClone(bool requireNewRef = false)
        {
            return (VersaSignalRWriter) MemberwiseClone();
        }

        object IDeepCloneable.DeepClone(bool requireNewRef)
        {
            return DeepClone(requireNewRef);
        }
    }
}
