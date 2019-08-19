using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Versagen;

namespace Versagen.Tests
{
    public class BuildDeps
    {
        protected class DebugList : List<string> { }

        /// <summary>
        /// Test no longer needed.
        /// </summary>
        [Fact]
        public void TestConfig()
        {
            var one = VersaCommsID.FromEnum(EVersaCommIDType.User, ulong.MinValue);
            var two = VersaCommsID.FromEnum(EVersaCommIDType.User, ulong.MaxValue);
            Assert.True(true);
        }
    }
}
