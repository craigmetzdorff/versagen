using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Versagen.IO;
using Versagen.PlayerSystem;
using Versagen.World;

namespace Versagen.Events.Commands
{
    public class ArgsCommandContext : CommandContext
    {

        public new class Builder: CommandContext.Builder, ICommandContextBuilder<ArgsCommandContext>
        {
            private IVersaCommand<ArgsCommandContext> command;

            /// <summary>
            /// Param name, argument text, argument value.
            /// </summary>
            public ImmutableDictionary<string, (string, object)>.Builder Args { get; set; } =
                ImmutableDictionary<string, (string, object)>.Empty.ToBuilder();

            public IVersaCommand<ArgsCommandContext> Command
            {
                get => command;
                set
                {
                    command = value;
                    
                } }
            public new ArgsCommandContext Build() => new ArgsCommandContext(this);
        }

        protected ImmutableDictionary<string, (string, Task<object>)> _asyncArgsDict { get; }

        protected ImmutableDictionary<string, (string, Lazy<object>)> _syncArgsDict { get; }

        public (string, object) this[string name]
        {
            get
            {
                return _syncArgsDict[name];
            }
        }



        public T ArgValue<T>(string name)
        {
            
            if (_syncArgsDict.ContainsKey(name))
            {
                if (_syncArgsDict[name].Item2 is Lazy<T> valOut)
                    return valOut.Value;
                else
                    throw new InvalidCastException("This parameter is not of that type!");
            }
            else throw new KeyNotFoundException("This parameter type is not present in the context!");
        }

        public Task<T> ArgValueAsync<T>(string name)
        {
            if (_asyncArgsDict.ContainsKey(name))
            {
                if (_asyncArgsDict[name].Item2 is Task<T> valOut)
                    return valOut;
                else
                    throw new InvalidCastException("This parameter is not of that type!");
            }
            else throw new KeyNotFoundException("This parameter type is not present in the context!");
        }

        protected ArgsCommandContext(Builder b) : base((ICommandContextBuilder<CommandContext>)b)
        {
            _asyncArgsDict = b.Args.Where(c => c.Value.Item2 is Task<object>)
                .Select(c => KeyValuePair.Create(c.Key, (c.Value.Item1, (Task<object>)c.Value.Item2)))
                .ToImmutableDictionary();
            _syncArgsDict = b.Args.Where(c => !_asyncArgsDict.ContainsKey(c.Key))
                .Select(c => KeyValuePair.Create(c.Key, (c.Value.Item1, 
                
                (c.Value.Item2 is Lazy<object> lazy) ? lazy: new Lazy<object>(c.Value.Item2)
                
                )
                
                ))
                .ToImmutableDictionary();
        }
    }
}
