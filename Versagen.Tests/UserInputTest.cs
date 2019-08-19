using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Versagen.Events;
using Versagen.Events.Commands;
using Xunit;
using Xunit.Sdk;

namespace Versagen.Tests
{
    public class UserInputTest
    {
        ConcurrentDictionary<IVersaCommand<CommandContext>, string> testDictionary =  new ConcurrentDictionary<IVersaCommand<CommandContext>, string>();

        IVersaCommand<CommandContext> command;
        private object e;

        public void addCommand(IVersaCommand<CommandContext> command, string commandMatchString)
        {
            if (!testDictionary.TryGetValue(command, out commandMatchString))
            {
                testDictionary.TryAdd(command, commandMatchString);
            }
        }

        public void updateCommand(IVersaCommand<CommandContext> command, string commandMatchString,
            string newCommandMatchString)
        {
            if (testDictionary.TryGetValue(command, out commandMatchString))
            {
                testDictionary.TryUpdate(command, commandMatchString, newCommandMatchString);
            }
        }

        public bool removeCommand1(IVersaCommand<CommandContext> command, string commandMatchString)
        {
            if (testDictionary.TryGetValue(command, out commandMatchString))
            {
                testDictionary.TryRemove(command, out commandMatchString);
                return true;
            }

            return false;
        }
        public bool TryFindCommand1(IEvent e, IVersaCommand<CommandContext> command, out string commandMatchString)
        {
            bool x = testDictionary.TryGetValue(command, out commandMatchString);
            testDictionary.GetOrAdd(command, commandMatchString);
            return x;
        }

        //[Fact]
        public void testMethods()
        {
            addCommand(command, "test");
            addCommand(command, "abc");
            updateCommand(command, "test", "newTest");
            Assert.True(removeCommand(command, "newTest"));
            Assert.True(TryFindCommand(e, command, "newTest"));
        }

        private bool TryFindCommand(object e, IVersaCommand<CommandContext> command, string v)
        {
            bool x = testDictionary.TryGetValue(command, out v);
            testDictionary.GetOrAdd(command, v);
            return x;
        }
    
        private bool removeCommand(IVersaCommand<CommandContext> command, string v)
        {
            if (testDictionary.TryGetValue(command, out v))
            {
                testDictionary.TryRemove(command, out v);
                return true;
            }
            return false;
        }

        public static void main(String[] args)
        {
            UserInputTest myTest = new UserInputTest();
            myTest.testMethods();
        }
    }
}

