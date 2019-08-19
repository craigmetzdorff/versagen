using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    public class CommandService : ICommandService
    {
        private SubjectBase<CommandRunStateEventArgs> RunStateArgs { get; }
        public IDisposable Subscribe(IObserver<CommandRunStateEventArgs> observer) => RunStateArgs.Subscribe(observer);

        protected volatile ImmutableSortedDictionary<ICommandGroup, RefCountDisposable> CommandGroups;

        private readonly object _gate = new object();

        public void Dispose()
        {
            RunStateArgs.Dispose();
        }

        public void OnCompleted()
        {
            RunStateArgs.OnCompleted();
        }

        public void OnError(Exception error)
        {
            RunStateArgs.OnError(error);
        }

        public async void OnNext(IEvent value)
        {
            if (!(value is IMessageEvent me)) return;
            if (value.IsSystemMessage || value.IgnoreThis) return;
            if (!TryFindCommand(me, out var matchCom, out var matchStr)) return;
            var outs = await RunSupportedCommand(me, matchCom, matchStr);
            if (outs.Item1)
            {
                    
                //command was supported and ran successfully; do whatever.
            }
            else
                throw new NotSupportedException(
                    $"Unsupported command type encountered: {matchCom.GetType().FullName}");
        }

        public IEnumerator<ICommandGroup> GetEnumerator()
        {
            return Groups.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<ICommandGroup> Groups => CommandGroups.Keys.Append(DefaultGroup);
        public IEnumerable<IVersaCommand> AllCommands => Groups.SelectMany(x => x).Distinct();
        public string GlobalPrefix { get; }
        public ICommandGroup DefaultGroup { get; }

        public IDisposable AddCommandGroup(ICommandGroup group)
        {
            lock (_gate)
            {
                if (!CommandGroups.ContainsKey(group))
                    return CommandGroups[group].GetDisposable();
                var newRefDisposable = new RefCountDisposable(Disposable.Create(() => RemoveCommandGroup(group, true)));
                for (;;)
                {
                    if (ImmutableInterlocked.Update(ref CommandGroups, (x,y) => x.Add(y, newRefDisposable), group))
                        break;
                }
                return CommandGroups[group].GetDisposable();
            }
        }

        /// <summary>
        /// Helper method to allow awaiting a disposable. This way we can ensure if calling from outside a disposable we await everything currently using the actual group.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="fromDisposable"></param>
        /// <returns></returns>
        private bool RemoveCommandGroup(ICommandGroup group, bool fromDisposable)
        {
            //TODO: Is this necessary?
            if (!fromDisposable)
            {
                RefCountDisposable holdIt;
                lock (_gate)
                    holdIt = CommandGroups.GetValueOrDefault(group);
                
                if (holdIt == default) return false;
                if (!holdIt.IsDisposed)
                    holdIt.Dispose();
                return true;
            }
            lock (_gate)
            {
                if (!CommandGroups.ContainsKey(group))
                    return false;
                for (; ; )
                {
                    if (ImmutableInterlocked.Update(ref CommandGroups, (x, y) => x.Remove(y), group))
                        break;
                }
                return true;
            }
        }

        public bool RemoveCommandGroup(ICommandGroup @group) => RemoveCommandGroup(@group, false);

        public virtual bool TryFindCommand(IMessageEvent e, out IVersaCommand command, out string matchedCommandText)
        {
            
            foreach (var comG in e.EventSpecificCommands.Concat(Groups))
            {
                if (!comG.TryFindCommand(GlobalPrefix, e, out command, out matchedCommandText)) continue;
                RunStateArgs.OnNext(new CommandRunStateEventArgs(e, command, null, ECommandState.FoundCommand));
                return true;
            }
            command = default;
            matchedCommandText = default;
            return false;
        }


        public virtual async Task<(bool, Task<(bool, IConditionalRule, string)>)> RunSupportedCommand(
            IMessageEvent @event, IVersaCommand command, string matchedCommandLine)
        {
            switch (command)
            {
                case IVersaCommand<CommandContext> basicCommand:
                    RunStateArgs.OnNext(new CommandRunStateEventArgs(@event, command, null, ECommandState.BeforeContextConstruction));
                    var factory = @event.Services
                        .GetRequiredService<ICommandContextFactory<CommandContext>>();
                    var (context, additionalConditionFuncs) = await factory.ConfigureContextAsync(@event, basicCommand, matchedCommandLine).ConfigureAwait(false);
                    return (true, RunCommand(@event, context, basicCommand,  @event.Services, additionalConditionFuncs));
                default:
                    return (false, default);
            }
        }

        

        public async Task<(bool, IConditionalRule, string)> RunCommand(IEvent e, CommandContext context, IVersaCommand<CommandContext> command,
            IServiceProvider provider,
            params Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>[] additionalChecks)
        {
            using (var checkTokenSource = new CancellationTokenSource())
            {
                RunStateArgs.OnNext(new CommandRunStateEventArgs(e, command, context, ECommandState.Preconditions));
                var (allClear, failedRule, failureReason) = additionalChecks.Concat(command.PrepRuleTasks(context, provider))
                    .AsParallel()
                    .WithCancellation(checkTokenSource.Token)
                    .Select(t => t.Invoke()).Select(t => t.Result)
                    .SkipWhile(c => c.passed)
                    .Take(1)
                    .DefaultIfEmpty((true, default, default))
                    .Single();
                try
                {
                    checkTokenSource.Cancel();
                }
                //IMPORTANT: DO NOT USE CHECKTOKENSOURCE BEYOND THIS POINT.
                catch
                {
                    // ignored
                }

                if (!allClear) return (false, failedRule, failureReason);
                //Deadlock intentional; we would WANT only one to run at a time.
                try
                {
                    await command.Run(context, provider);
                    RunStateArgs.OnNext(new CommandRunStateEventArgs(e, command, context, ECommandState.PostCommand));
                }
                catch (Exception ex)
                {
                    RunStateArgs.OnNext(new CommandRunStateEventArgs(e, command, context, ECommandState.PostCommand, ex));
                }
                return (true, default, default);
            }
        }

        public CommandService(string globalPrefix, IEnumerable<IVersaCommand> defaultCommands) : this(globalPrefix)
        {
            if (defaultCommands.Select(DefaultGroup.TryAdd).SkipWhile(x => x).Any())
                throw new Exception("Error populating default command group at construction time. Please debug and inspect for errors."); 
        }

        public CommandService(string globalPrefix)
        {
            GlobalPrefix = globalPrefix;
            RunStateArgs = new Subject<CommandRunStateEventArgs>();
            CommandGroups = ImmutableSortedDictionary<ICommandGroup, RefCountDisposable>.Empty.WithComparers(new CommandExecOrderer());
            DefaultGroup = new CommandGroup("","/commands");
        }
    }
}
