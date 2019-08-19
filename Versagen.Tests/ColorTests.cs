using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Versagen.Structs;
using Xunit;

namespace Versagen.Tests
{
    public class ColorTests
    {

        [Fact]
        public void PrintStyles()
        {
            Debug.Print(Color.White.ToStringCssRGBA());
        }
    }
}
