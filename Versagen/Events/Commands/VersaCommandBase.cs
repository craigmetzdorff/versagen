using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    [Flags]
    public enum ECommandRunSpace
    {
        Disabled = 0,
        World = 1,
        Story = World << 1,
        Private = Story << 1,
        All = World | Story | Private
    }

    public enum EHandleRemainder
    {
        Discard = 0,
        AsString = 1,
        AsTokens = 2,
        AsType = 3,
        CustomFunction = 4
    }

    public abstract class VersaCommandBase<T> : IVersaCommand<T> where T:ICommandContext 
    {
        public string Name { get; }
        public string CommandLine { get; protected set; }
        public ECommandRunSpace RunSpace { get; }

        public char SeparatorChar { get; }

        public Func<T, IServiceProvider, Task> Do { get; }

        /// <summary>
        /// If true, this will not run as a seperate task, but instead as part of a regular thread.
        /// </summary>
        /// <remarks>Should be true when changing settings in the SQL database.</remarks>
        public bool RunSynchronously { get; }
        public EVersaPerms RequiredPrivilegdes { get; }

        public IConditionalRule[] Rules { get; }

        public string Description { get; }

        public virtual IVersaCommand WithCommandLinePrefix(string prependThis)
        {
            var copy = (VersaCommandBase<T>)MemberwiseClone();
            copy.CommandLine = prependThis + SeparatorChar + copy.CommandLine;
            return copy;
        }

        public IEnumerable<Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>> PrepRuleTasks(
            ICommandContext context, IServiceProvider provider)
        =>
            Rules.Select(r => new Func<Task<(bool, IConditionalRule, string)>>(async () =>
            {
                var (success, reason) = await r.CheckRule(context, provider).ConfigureAwait(false);
                return (success, r, reason);
            }));


        public virtual Task Run(T args, IServiceProvider provider)
        {
            if (!RunSynchronously) return Do.Invoke(args, provider);
            Do.Invoke(args, provider).Wait(); //TODO:Is this what I should actually do? Wait might not be best option.
            return Task.CompletedTask;

        }

        protected VersaCommandBase(ICommandBuilder<T,VersaCommandBase<T>> b)
        {
            Func<T, IServiceProvider, Task> DoBuilderVI()
            {
                //Store these in their own variables. Otherwise the entire builder will get moved over into the lambda.
                var neededType = b.TansientClassNeeded;
                var retMethod = b.TransientClassMethod;
                var constArgs = b.additionalConstructorArgs;
                //We want to set all public properties currently at default values
                //TODO: Add an attribute check to tell method to ignore certain classes.
                var PropertiesToCheck = neededType.GetProperties().Where(p => p.SetMethod?.IsPublic ?? false);
                //Then only store this method so that there won't have to be an entire other structure to take care of it.
                return (context, provider) =>
                {
                    //should it be GetServiceOrCreateInstance instead?
                    IDisposable transient = (IDisposable)Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(provider, neededType, constArgs);
                    //Attempt to auto-populate members with public setters. This should only be used with members that are OPTIONAL for the command system to run.
                    foreach (var p in PropertiesToCheck)
                    {
                        if (p.GetValue(transient) !=
                            (p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null)) continue;
                        try
                        {
                            p.SetValue(transient, provider.GetService(p.PropertyType));
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    try { return retMethod.Invoke(transient).Invoke(context).ContinueWith(_ => transient?.Dispose()); }
                    catch (Exception ex)
                    {
                        try
                        {
                            return Task.FromException(ex);
                        }
                        finally
                        {
                            try
                            {
                                transient.Dispose();
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(b.Name) ||
                string.IsNullOrWhiteSpace(b.Description) ||
                (!b.CallLine.Any()) ||
                string.IsNullOrWhiteSpace(b.CallLine))
                throw new ArgumentNullException("Name","ActAsCommands must be named, have a defined description, and may not be called on a blank or whitespace post!");
            CommandLine = b.CallLine;
            Description = b.Description;
            if (b.TansientClassNeeded != default)
            {
                Do = DoBuilderVI();
            }
            else if (b.Do != default) Do = (c, _) => b.Do(c);
            else throw new ArgumentNullException("Do", "ActAsCommands must be configured via either setting \"Do\" or calling \"UsingTransientClass\"");
            Name = b.Name;
            Rules = b.Predicates != null ? b.Predicates.ToArray() : new IConditionalRule[0];
            RequiredPrivilegdes = b.RequiredPrivilegdes;
            RunSpace = b.ChatContext;
            RunSynchronously = b.RunSynchronously;
        }

        /// <summary>
        /// Masks a given VersaCommand with a context other than ICommandContext as something as generic as possible.
        /// This is private because I technically shouldn't be doing this.
        /// </summary>
        private struct InterfaceNegotiator:IVersaCommand<ICommandContext>
        {
            public VersaCommandBase<T> baseCom { get; }
            public string CommandLine => baseCom.CommandLine;

            public string Name => baseCom.Name;

            public EVersaPerms RequiredPrivilegdes => baseCom.RequiredPrivilegdes;

            public IConditionalRule[] Rules => baseCom.Rules;

            public ECommandRunSpace RunSpace => baseCom.RunSpace;

            public bool RunSynchronously => baseCom.RunSynchronously;

            public char SeparatorChar => baseCom.SeparatorChar;

            public string Description => baseCom.Description;

            private (bool isRightType, T contextTyped) CheckContextOkay(ICommandContext inCon)
            {
                if (inCon is T outContext)
                    return (true, outContext);
                return (false, default);
            }

            public IVersaCommand WithCommandLinePrefix(string prependThis)
            {
                return this;
            }

            public IEnumerable<Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>> PrepRuleTasks(
                ICommandContext context, IServiceProvider provider)
            =>
                context is T ? baseCom.PrepRuleTasks(context, provider): throw new InvalidCastException("WRONG CONTEXT TYPE");
            

            public Task Run(ICommandContext args, IServiceProvider provider)
            {
                var (isRightType, contextTyped) = CheckContextOkay(args);
                if (isRightType)
                    return baseCom.Run(contextTyped, provider);
                throw new InvalidCastException("WRONG CONTEXT TYPE");
            }

            public Task Run(ICommandContext args, IServiceProvider provider, TaskScheduler scheduler)
            {
                return Run(args, provider);
            }

            internal InterfaceNegotiator(VersaCommandBase<T> versCommand)
            {
                baseCom = versCommand;
            }
        }

        /// <summary>
        /// WARNING: THIS IS DANGEROUS TERRITORY. USE VERY SPARINGLY.
        /// V E R Y
        /// YOU WILL ESSENTIALLY BREAK TYPE SAFETY LIKE THIS.
        /// When a command has been cast using the <see cref="WithICommandContext"></see> function, you can recover the original class using this and casting.
        /// </summary>
        /// <param name="hiding"></param>
        /// <returns></returns>
        public static (bool isHidden, VersaCommandBase<T> casted) GetFromInterface(IVersaCommand<ICommandContext> hiding)
        {
            if (hiding is InterfaceNegotiator negotiatorRevealed)
            {
                return (true, negotiatorRevealed.baseCom);
            }
            return (false, default);
        }

        /// <summary>
        /// WARNING: THIS IS DANGEROUS TERRITORY. USE VERY SPARINGLY.
        /// V E R Y
        /// YOU WILL ESSENTIALLY BREAK TYPE SAFETY LIKE THIS.
        /// Get a version of this command that casts to <see cref="IVersaCommand{ICommandContext}"/> for extremely generic methods that need it.
        /// The regular version of the command can be retrieved later via <see cref="GetFromInterface(IVersaCommand{ICommandContext})"/>
        /// </summary>
        /// <returns></returns>
        public IVersaCommand<ICommandContext> WithICommandContext()
        {
            return new InterfaceNegotiator(this);
        }
    }
}
