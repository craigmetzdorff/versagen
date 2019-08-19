using System;
using System.Diagnostics;
using Versagen.Structs;
using System.Threading.Tasks;
using Versagen.IO;

namespace Versagen.SampleConsoleOne
{
    public class VersaConsoleWriter :IVersaWriter
    {
        public void Dispose()
        {
            //notNeeded
        }

        public bool ColorSupport { get; } = false;
        public VersaCommsID DestinationID { get; set; }

        public Task WriteAsync(string message, Color color)
        {
            Console.Write(message);
            return Task.CompletedTask;
        }

        public Task WriteAsync(string message, ConsoleColor color)
        {
            var oldcolor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = oldcolor;
            return Task.CompletedTask;
        }

        public Task WriteAsync(string message)
        {
            Console.Write(message);
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(string message, Color color)
        {
            Console.WriteLine(message, color);
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(string message, ConsoleColor color)
        {
            var oldcolor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldcolor;
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        public VersaConsoleWriter(string Preamble, ConsoleColor PreambleColor)
        {

        }
    }
}