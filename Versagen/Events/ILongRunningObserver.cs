using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Events
{
    public interface ILongRunningObserver<in T> : IObserver<T>
    {
        
    }
}
