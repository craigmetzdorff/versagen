using System;
using Versagen.Events;

namespace Versagen
{
    public class VersaIO<C, Q, E, U, T> where C: IVersaCommunication<U,T> where Q: EventArgs, T where E: IEvent
    {

        public void DoInput(C newInput)
        {

            
        }

        protected VersaIO(IServiceProvider services)
        {
            //EQmanager = services.GetRequiredService<IEventQueueManager>();
        }
    }
}
