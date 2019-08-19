using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Versagen.Events
{
    public interface IEventPreProcessor<T> :IEventPreprocessor where T : IEvent
    {
        new Task<TI> PreProcessAsync<TI>(TI newEvent) where TI:T;
        new TI PreProcess<TI>(TI newEvent) where TI : T;
    }

    public interface IEventPreprocessor
    {
        Task<T> PreProcessAsync<T>(T newEvent) where T : IEvent;

        T PreProcess<T>(T newEvent) where T : IEvent;
    }
}
