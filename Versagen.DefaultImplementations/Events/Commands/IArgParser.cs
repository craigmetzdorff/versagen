using System;

namespace Versagen.Events.Commands
{
    public interface IArgParser
    {
        Type TypeParsed { get; }
        object ParseThis(string token);
    }

    public interface IArgParser<out T> : IArgParser
    {
        new T ParseThis(string token);
    }
}
