using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Input;
using Versagen.Entity;
using Versagen.PlayerSystem;
using Versagen.Scenarios;
using Versagen.Utils;

namespace Versagen.Events.Commands
{




    public static class StatManiuplation
    {
        public class StatChangeLinqDefiner<TCommand, TContext, TStat, TValue>
        {

        }


        public class StatChangeDelegate<TValue> : IDisposable
        {
            private readonly Func<IModifyStat<TValue>, ICommandContext, Task> _manipulateFunc;

            private IEntityStore store;
            public async Task ChangeStat(ICommandContext context)
            {
                var target = store.Entities.FirstOrDefault(x => context.MessageRemainder.Trim().Equals(x.Name));
                if (target == default)
                {
                    await context.OriginTerm.WriteLineAsync("But that target doesn't exist!");
                    return;
                }

                var maxHealthTarget = (IStat<int>)target.Stats["MaxHealth"];
                if (target.GetStatTotal<int>("MaxHealth") <= 0)
                {
                    await context.OriginTerm.WriteLineAsync("Don't beat up a defenseless corpse!");
                    return;
                }
                IModifyStat<int> damageStat = default;
                if (!target.Stats.TryGetValue("LostHealth", out var DamageStatUncast))
                {

                    damageStat = new NumericStatModifier(maxHealthTarget, "LostHealth", false);
                    target.Stats.Add(damageStat.Name, damageStat);
                }
                else
                {
                    damageStat = (IModifyStat<int>)DamageStatUncast;
                }

                await context.OriginTerm.WriteLineAsync($"{context.ActingEntity.Name} strikes {target.Name} with all their strength!");
                var strength = context.ActingEntity.GetStatTotal<int>("Attack") + new Dice(1, 6, 0) - target.GetStatTotal<int>("Defense");
                damageStat.Value -= strength;

                if (target.GetStatTotal<int>("MaxHealth") <= 0)
                {
                    await context.OriginTerm.WriteLineAsync("Your enemy falls to the ground, unquestionably dead. VICTORYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY!");
                }
                else
                {
                    await context.OriginTerm.WriteLineAsync(
                        $"{target.GetStatTotal<int>("MaxHealth")}/{maxHealthTarget} hitpoints remain!");
                }
                await context.Pipe.ChainEvent(new MessageEvent
                {
                    Player = new UnionType<VersaCommsID, IPlayer>(),
                    IsSystemMessage = true,
                    Entity = new UnionType<VersaCommsID, IEntity>(target),
                    Scenario = new UnionType<VersaCommsID, IScenario>(context.ScenarioID),
                    FullMessage = $"{target.Id}:ATTACKED"
                }).ConfigureAwait(true);
            }
            public StatChangeDelegate(IEntityStore store)
            {
                this.store = store;
            }

            public void Dispose()
            {
            }
        }

        public static ICommandBuilder<TContext, TCommand> DoStatChange<TCommand, TContext, TValue>(this ICommandBuilder<TContext, TCommand> builder,
            string baseStatName,
            string Description,
            Func<IEntityStore, string, IEntity> targetSelector,
            Func<IModifyStat<TValue>, IEntity, ICommandContext, TValue> manipulator
            )
            where TCommand : IVersaCommand<TContext> where TContext : ICommandContext
        { 

            return builder;
        }


        
    }
}
