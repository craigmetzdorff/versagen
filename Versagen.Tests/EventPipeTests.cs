using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Versagen.Utils;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Entity;
using Versagen.Events;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace Versagen.Tests
{
    public class EventPipeTests
    {
        public class TestObserver : IObserver<IEvent>
        {
            private string mode { get; }
            private int identif;
            private IDisposable completeDispose;

            public void OnCompleted()
            {
                outp.WriteLine(identif + mode + " Completed.");
                completeDispose.Dispose();
            }

            public void OnError(Exception error)
            {
                outp.WriteLine("error from" + identif + mode + ": " + error);
            }

            public async void OnNext(IEvent value)
            {
                if (!(value is IMessageEvent me)) return;
                await Task.Yield();
                value.Services.GetRequiredService<List<int>>().Add(identif);
                outp.WriteLine(DateTime.Now + " " + identif + mode + " says " + me.FullMessage + " and the latest list item is " + value.Services.GetService<List<int>>().Last());
            }

            private ITestOutputHelper outp;

            public TestObserver(int id, ITestOutputHelper output,string mode, IDisposable DisposeAtEnd)
            {
                identif = id;
                outp = output;
                this.mode = mode;
                completeDispose = DisposeAtEnd;
            }
        }


        IEventPipe pipe;

        private ITestOutputHelper outp;

        private IServiceProvider testProv;

        [Fact]
        public async void AsyncDisposeTest()
        {
            pipe = new EventPipe();
            var endThing = new TaskCompletionSource<bool>();
            var disposer = new RefCountDisposable(Disposable.Create(() => endThing.SetResult(true)));
            for (var i = 0; i < 100; i++)
            {
                pipe.ObserveFirst.Subscribe(new TestObserver(i, outp, "first", disposer.GetDisposable()));
                pipe.ObserveConcurrent.Subscribe(new TestObserver(i, outp, "reader", disposer.GetDisposable()));
                pipe.ObserveSynchronous.Subscribe(new TestObserver(i,outp,"writer", disposer.GetDisposable()));
            }
            disposer.Dispose();
            //var testTask = new List<Task>();
            var totalDisposer = new RefCountDisposable(Disposable.Create(() => pipe.Complete()));
            var rand = new Random();
            Parallel.ForEach(Enumerable.Range(0, 99), async x =>
            {
                using (totalDisposer.GetDisposable())
                {
                    using (var scp = testProv.CreateScope())
                    {
                        var e = new MessageEvent
                        {
                            IgnoreThis = false,
                            Services = scp.ServiceProvider.CreateScope().ServiceProvider,
                            Scenario = VersaCommsID.FromEnum(EVersaCommIDType.Scenario, 0),
                            Player = new UnionType<VersaCommsID, IPlayer>(0),
                            Terminal = new UnionType<VersaCommsID, IVersaWriter>(0),
                            FullMessage = x.ToString(),
                            Entity = new UnionType<VersaCommsID, IEntity>(0),
                        };
                        await Task.Yield();
                        await pipe.ProcessEvent(e);
                    }
                }
            });
            
            //totalDisposer.Dispose();
            //scp.Dispose();
            //await Task.WhenAll(testTask);
            
            await Task.Delay(200);
            totalDisposer.Dispose();
            //await Task.Delay(200);
            await endThing.Task;
            //pipe.Dispose();
        }


        public EventPipeTests(ITestOutputHelper output)
        {
            outp = output;
            pipe = new EventPipe();
            var provCol = new ServiceCollection();
            provCol.AddScoped<List<int>>();
            testProv = provCol.BuildServiceProvider();
        }
    }
}
