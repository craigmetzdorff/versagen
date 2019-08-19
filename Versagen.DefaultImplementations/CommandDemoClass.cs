using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Versagen.Entity;
using Versagen.Events.Commands;
using Versagen.Locations;

namespace Versagen
{
    public class CommandDemoClass :IDisposable
    {

        public RootLocationHolder root { get; }

        public IServiceProvider provider { get; set; }

        public static void AddDemoCommandsForService(ICommandGroup group)
        {
            group.TryAdd(
                    new VersaCommand.Builder()
                        {
                            Name = "lookcom",
                            CallLine = "look",
                            Description = "Have a look around.",
                            
                        }.UsingTransientClass<CommandDemoClass>(e => e.DoALook).Build()
                
            );
        }

        public Task DoALook(ICommandContext context)
        {
            if (context.MessageRemainder.EndsWith("at mirror"))
            {
                context.UserTerm.WriteLineAsync("You stare into the mirror in front of you.");
                context.UserTerm.WriteLineAsync(context.ActingEntity != null
                        ? (provider != null
                            ? context.ActingEntity.GetDefaultDescription().BuildDescription(context, provider)
                            : context.ActingEntity.GetDefaultDescription().BuildDescription(null, null)):
                    "You can't really see yourself well for some reason. After a minute or so you give up.");
            }
            else
            {
                context.UserTerm.WriteLineAsync(provider != null
                    ? context.ActingEntity.CurrentLocation.GetDefaultDescription().BuildDescription(context, provider)
                    : context.ActingEntity.CurrentLocation.GetDefaultDescription().BuildDescription(null, null));
            }
            return Task.CompletedTask;
        }


        private IEntityStore store { get; }

        







        public CommandDemoClass(IEntityStore<Entity.Entity> store, RootLocationHolder rootLocation)
        {
            this.store = store;
            this.root = rootLocation;
        }

        public void Dispose()
        {
        }
    }
}
