using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    public class VersaArgsCommand : VersaCommandBase<ArgsCommandContext>
    {
        public class Builder : ICommandBuilder<ArgsCommandContext, VersaArgsCommand>
        {
            /// <summary>
            /// Check to see if a command is allowed in a given group.
            /// </summary>
            public Func<ArgsCommandContext, Task> Do { get; set; }
            public string Name { get; set; }
            public string CallLine { get; set; }
            public string Description { get; set; }

            public List<IConditionalRule> Predicates { get; set; }

            /// <summary>
            /// Determine where a given command can run.
            /// </summary>
            public ECommandRunSpace ChatContext { get; set; }
            public EVersaPerms RequiredPrivilegdes { get; set; } = EVersaPerms.Standard;
            /// <summary>
            /// If true, this will not run as a seperate task, but instead as part of a regular thread.
            /// </summary>
            /// <remarks>Should be true when changing settings in the SQL database.</remarks>
            public bool RunSynchronously { get; set; }

            /// <summary>
            /// Split command on this character. Always used to check if command is called, but ignored if ArgPattern is set.
            /// </summary>
            public char SplitOn { get; set; } = ' ';

            public object[] additionalConstructorArgs { get; set; }

            /// <summary>
            /// Allows assigning a pattern to look for for either validation or arguments.
            /// If empty, just split on separator character.
            /// </summary>
            public string ArgPattern { get; set; }

            public ImmutableArray<string>.Builder ParamNames { get; set; } = ImmutableArray<string>.Empty.ToBuilder();

            public ImmutableArray<string>.Builder ParamDescriptions { get; set; } = ImmutableArray<string>.Empty.ToBuilder();

            public ImmutableArray<RuntimeTypeHandle>.Builder ArgTypes { get; set; } = ImmutableArray<RuntimeTypeHandle>.Empty.ToBuilder();

            public ImmutableArray<Func<string, IServiceProvider, Task<object>>>.Builder OverrideFuncs { get; set; } = ImmutableArray<Func<string, IServiceProvider, Task<object>>>.Empty.ToBuilder();

            public ImmutableArray<Func<string, string>>.Builder IdentityModifierFuncs { get; set; } = ImmutableArray<Func<string, string>>.Empty.ToBuilder();
            public Type TansientClassNeeded { get; set; }
            public Func<IDisposable, Func<ArgsCommandContext, Task>> TransientClassMethod { get; set; }

            public Builder AddArgType(string paramName, string paramDescription, Type paramType, Func<string, IServiceProvider, Task<object>> overrideFunc = null)
            {
                ParamNames.Add(paramName);
                ParamDescriptions.Add(paramDescription);
                ArgTypes.Add(paramType.TypeHandle);
                // ReSharper disable once PossibleNullReferenceException
                OverrideFuncs.Add(overrideFunc.Invoke);
                IdentityModifierFuncs.Add(null);
                return this;
            }

            public Builder AddArgType<T>(string paramName, string paramDescription, Func<string, IServiceProvider, Task<T>> overrideFunc = null) =>
                AddArgType(paramName, paramDescription, typeof(T), (r, s) => overrideFunc.Invoke(r, s).ContinueWith(t => (object)t.Result));

            public Builder AddArgType<T>(string paramName, string paramDescription, Func<string, string> identityModifier = null)
            {
                ParamNames.Add(paramName);
                ParamDescriptions.Add(paramDescription);
                ArgTypes.Add(typeof(T).TypeHandle);
                OverrideFuncs.Add(null);
                IdentityModifierFuncs.Add(identityModifier);
                return this;
            }

            public IEnumerable<(string name, string description, RuntimeTypeHandle type, Func<string, IServiceProvider, Task<object>> overrideFunc, Func<string, string> identityModifierFunc)> EnumerateParams()
            {
                for (int i = 0; i < ParamNames.Count; i++)
                {
                    yield return (ParamNames[i], ParamDescriptions[i], ArgTypes[i], OverrideFuncs[i], IdentityModifierFuncs[i]);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="args">Parameter name, parameter description, parameter type, arguments</param>
            /// <returns></returns>
            public Builder AddArgTypes(params (string, string, Type, Func<string, IServiceProvider, Task<object>>)[] args)
            {
                foreach (var tuple in args) AddArgType(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
                return this;
            }

            public ICommandBuilder<ArgsCommandContext, VersaArgsCommand> UsingTransientClass<T>(Func<T, Func<ArgsCommandContext, Task>> methodToCall, params object[] additionalConstructorArgs) where T : IDisposable
            {
                throw new NotImplementedException();
            }

            public VersaArgsCommand Build() => new VersaArgsCommand(this);

            public ICommandBuilder<ArgsCommandContext, VersaArgsCommand> UsingTransientClass<T>(Func<T, Func<ArgsCommandContext, Task>> methodToCall) where T : IDisposable
            {
                throw new NotImplementedException();
            }
        }

        public bool UsesSeparation { get; }

        public string ArgPattern { get; }

        public string[] GroupsToMatch { get; }

        public ImmutableArray<string> ParamNames { get; }

        public ImmutableArray<string> ParamDescriptions { get; }

        public ImmutableArray<RuntimeTypeHandle> ArgTypes { get; }

        public ImmutableArray<Func<string, IServiceProvider, Task<object>>> OverrideFuncs { get; }

        public ImmutableArray<Func<string, string>> IdentityModifierFuncs { get; }

        public IEnumerable<(string name, string description, RuntimeTypeHandle type, Func<string, IServiceProvider, Task<object>> overrideFunc, Func<string, string> identityModifierFunc)> EnumerateParams()
        {
            for (int i = 0; i < ParamNames.Length; i++)
            {
                yield return (ParamNames[i], ParamDescriptions[i], ArgTypes[i], OverrideFuncs[i], IdentityModifierFuncs[i]);
            }
        }

        protected VersaArgsCommand(Builder b) : base(b)
        {
            if (b.ArgPattern == default)
            {
                try
                {
                    var theSingle = b.ArgTypes.SingleOrDefault(c => Type.GetTypeFromHandle(c).IsArray);
                    if (!theSingle.Equals(default) && !theSingle.Equals(b.ArgTypes.Last()))
                        throw new ArgumentException("When a regex is not used, only the last argument may be an array.");
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException("When a regex is not used, you may only have one array type in the arguments.");
                }
            }
            else
            {
                ArgPattern = b.ArgPattern;
                GroupsToMatch = new Regex(b.ArgPattern).GetGroupNames();
            }
            ParamNames = b.ParamNames.ToImmutable();
            ParamDescriptions = b.ParamDescriptions.ToImmutable();
            ArgTypes = b.ArgTypes.ToImmutable();
            OverrideFuncs = b.OverrideFuncs.ToImmutable();
            IdentityModifierFuncs = b.IdentityModifierFuncs.ToImmutable();
        }
    }
}
