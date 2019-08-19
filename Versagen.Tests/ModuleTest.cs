using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Versagen.Tests
{
    public class ModuleTest
    {
        public class InnerTester
        {
            public Task SomeTask() => Task.CompletedTask;
        }

        public void testMethodsEqual()
        {
            Expression<Func<InnerTester, Func<Task>>> testIn = (it) => it.SomeTask;
        }
    }
}
