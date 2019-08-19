using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.SignalR;
using Versagen.IO;

namespace Versagen.ASPNET.SignalR
{
    public class SignalRVersaGroupWriter<THub> :VersaSignalRWriter, IVersaGroupWriter where THub:Hub
    {
        private SignalRWriterDirectory<THub> directory;

        public SignalRVersaGroupWriter(VersaCommsID destID, SignalRWriterDirectory<THub> dir, IClientProxy sender, string functionName, HtmlEncoder encoder) : base(destID, sender, functionName, encoder)
        {
            this.directory = dir;
        }

        public VersaCommsID[] Subscribed { get; }
        public (int count, Exception exception) Subscribe(params VersaCommsID[] Ids)
        {
            var retCount = 0;
            foreach (var i in Ids)
            {
                directory.AddToSignalRConnection(base.DestinationID, i).Wait();
            }

            return (Ids.Length, null);
        }

        public (int count, Exception exception) UnSubscribe(params VersaCommsID[] Ids)
        {
            throw new NotImplementedException();
        }
    }
}
