using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Versagen.Events
{
    //From https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-1-asyncmanualresetevent/
    public class AsyncManualResetEvent :IDisposable
    {
        private volatile TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() => m_tcs.Task;

        public void Set() => m_tcs.TrySetResult(true);

        public void Reset()
        {
            while (true)
            {
                var tcs = m_tcs;
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
            }
        }

        public void Dispose()
        {
            m_tcs.TrySetCanceled();
            m_tcs.Task.Dispose();
        }
    }
}
