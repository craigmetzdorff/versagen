//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace Versagen.Events
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <remarks>Can't really just use a disposable since it will be awaited and thus still referenced. Need to use this pattern for the sake of ASP.NET contexts, for example, to keep the ServiceProvider alive for its necessary duration.</remarks>
//    public sealed class KeepEventAliveSlim : IKeepEventAlive
//    {
//        public class KeepEventAliveSlimDisposer :IDisposable
//        {
//            private KeepEventAliveSlim slim;

//            internal KeepEventAliveSlimDisposer(KeepEventAliveSlim slimIn)
//            {
//                slim = slimIn;
//            }

//            ~KeepEventAliveSlimDisposer()
//            {
//                Dispose();
//            }

//            public void Dispose()
//            {
//                if (slim.HoldOffOnCompletion)
//                    slim.HoldOffOnCompletion = false;
//                else
//                    slim.DoComplete();
//            }
//        }

//        public struct KeepEventAliveAwaiter :IAwaiter
//        {
//            private readonly KeepEventAliveSlim slim;

//            private Action onCompletedThing;

//            public void OnCompleted(Action continuation)
//            {
//                onCompletedThing = continuation;
//            }

//            public void UnsafeOnCompleted(Action continuation)
//            {
//                onCompletedThing = continuation;
//            }

//            public void GetResult()
//            {
//                if (!slim.IsComplete)
//                    slim.completeSource.Value.Task.GetAwaiter().GetResult();
//                onCompletedThing();
//            }

//            public bool IsCompleted => slim.IsComplete;
//        }

//        private Lazy<TaskCompletionSource<bool>> completeSource = new Lazy<TaskCompletionSource<bool>>();
//        public bool HoldOffOnCompletion { get; set; }
//        public bool IsComplete { get; private set; }
//        public void DoComplete()
//        {
//            IsComplete = true;
//            if (completeSource.IsValueCreated)
//                completeSource.Value.TrySetResult(true);
//        }

//        public void Dispose()
//        {
//            DoComplete();
//        }
//        public IDisposable GetDisposer() => new KeepEventAliveSlimDisposer(this);

//        public IAwaiter GetAwaiter()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
