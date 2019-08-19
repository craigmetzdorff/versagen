using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    /// <summary>
    /// TODO: Complete and test this framework. Was not completed in time due to time constraints.
    /// </summary>
    /// <typeparam name="TB"></typeparam>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TC"></typeparam>
    public class ArgsContextBuilderFactory<TB, TE, TC> : ICommandContextFactory<ArgsCommandContext, TB, TE, TC>
        where TB : ArgsCommandContext.Builder, new()
        where TE : IMessageEvent
        where TC : VersaArgsCommand
    {

        IServiceProvider provider { get; }

        public Dictionary<RuntimeTypeHandle, Func<string, object>> SyncParamFetchers { get; }
            = new Dictionary<RuntimeTypeHandle, Func<string, object>>().AddByValueTupleEnum(
            (typeof(int).TypeHandle, (str) => int.Parse(str)),
            (typeof(ulong).TypeHandle, (str) => ulong.Parse(str))
        );


        public Dictionary<RuntimeTypeHandle, (bool, Func<string, Task<object>>)> AsyncParamFetchers { get; } = new Dictionary<RuntimeTypeHandle, (bool, Func<string, Task<object>>)>();

        public HashSet<RuntimeTypeHandle> IsAsyncTable { get; } = new HashSet<RuntimeTypeHandle>();

        public (TB builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(TE @event, TC command)
        {
            var builderStartp1 = ConfigureBuilder(@event);
            return (FetchParameterInfo(builderStartp1.builder, command), builderStartp1.additionalConditionFuncs);
        }

        private TB FetchParameterInfo(TB builder, TC command)
        {
            using (var paramEnum = command.EnumerateParams().GetEnumerator())
            {
                MatchCollection regMachine = Regex.Matches(builder.Message.Substring(builder.CommandString.Length), command.ArgPattern);

                var regMatch = regMachine[0];
                while (paramEnum.MoveNext())
                {
                    var cur = paramEnum.Current;
                    var type = Type.GetTypeFromHandle(cur.type);
                    if (command.GroupsToMatch.Any(c => c == cur.name))
                    {
                        if (type.IsArray)
                        {
                            var theGroupEnum = regMachine[0].Groups[cur.name].Captures.Select(c => c.Value);
                            var convertCallback = SyncParamFetchers[type.GetElementType().TypeHandle];
                            builder.Args.Add(cur.name,
                                (string.Join('\n', theGroupEnum), theGroupEnum.Select(c => convertCallback.Invoke(c)))
                            );
                        }
                        else
                        {
                            var theGroupEnum = regMatch.Groups[cur.name].Value;
                            var convertCallback = SyncParamFetchers[cur.type];
                            builder.Args.Add(cur.name, (theGroupEnum, convertCallback.Invoke(theGroupEnum)));
                        }
                    }
                    else
                    {
                        regMatch = regMatch.NextMatch();
                        var returnedVal = SyncParamFetchers[cur.type].Invoke(regMatch.Value);
                        builder.Args.Add(cur.name, (regMatch.Value, returnedVal));
                    }
                }
            };

            
            return builder;
        }

        public (TB builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureBuilder(TE @event)
        {
            var builder = new TB
            {
                Message = @event.FullMessage
            };
            return (builder, new Func<Task<(bool, IConditionalRule, string)>>[] { });
        }

        public Task<(TB builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(TE @event, TC command)
        {
            throw new NotImplementedException();
        }

        Task<TB> ICommandContextFactory<ArgsCommandContext, TB, TE, TC>.FetchParameterInfo(TB builder, TC command)
        {
            throw new NotImplementedException();
        }

        public Task<(TB builder, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureBuilderAsync(TE @event)
        {
            throw new NotImplementedException();
        }

        public void AddArgProcessor<T>(Func<string, IServiceProvider, T> parseFunction, bool useLazyInit = true)
        {
            if (useLazyInit)
                SyncParamFetchers.Add(typeof(T).TypeHandle, (s) => new Lazy<T>(() => parseFunction.Invoke(s, provider)));
            else
                SyncParamFetchers.Add(typeof(T).TypeHandle, (s) => {
                    var val = parseFunction.Invoke(s, provider);
                    return new Lazy<T>(val);
                });
        }

        public void AddArgTask<T>(Func<string, IServiceProvider, Task<T>> parseFunction, bool startTaskOnBuild = true)
        {
            var handle = typeof(T).TypeHandle;
            AsyncParamFetchers.Add(handle, (startTaskOnBuild, (s)
                => new Task<object>(() => parseFunction.Invoke(s, provider))));
        }

        public (ArgsCommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureContext(IEvent @event)
        {
            throw new NotImplementedException();
        }

        public (ArgsCommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[]
            additionalConditionFuncs) ConfigureContext(IEvent @event, IVersaCommand<ArgsCommandContext> command,
                string matchedCommandLine)
        {
            throw new NotImplementedException();
        }

        public (ArgsCommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs) ConfigureContext(IEvent @event, IVersaCommand command)
        {
            throw new NotImplementedException();
        }

        public Task<(ArgsCommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureContextAsync(IEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task<(ArgsCommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>
            [] additionalConditionFuncs)> ConfigureContextAsync(IMessageEvent @event,
            IVersaCommand<ArgsCommandContext> command, string matchedCommandLine)
        {
            throw new NotImplementedException();
        }

        public Task<(ArgsCommandContext context, Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalConditionFuncs)> ConfigureContextAsync(IEvent @event, IVersaCommand command)
        {
            throw new NotImplementedException();
        }
    }

    internal static class ContextBuilderFactoryHelperExtensions
    {
        public static Dictionary<K, V> AddByValueTupleEnum<K, V>(this Dictionary<K, V> dict, params (K, V)[] kVpairs)
        {
            foreach (var pair in kVpairs)
            {
                dict.Add(pair.Item1, pair.Item2);
            }

            return dict;
        }
    }
}
