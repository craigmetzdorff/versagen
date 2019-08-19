using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Events
{
    public interface IMessageEvent :IEvent
    {
        string FullMessage { get; set; }
    }
}
