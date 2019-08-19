using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Versagen.Entity;
using Versagen.Events.Commands;
using Versagen.Rules;

namespace Versagen.Events.Actions
{
    /// <inheritdoc />
    /// <summary>
    /// Provides actions that occur in response to other actions occurring. Distinct from commands in which they typically are not invoked, but chime off of some other option.
    /// </summary>
    /// <typeparam name="TCommandContext"></typeparam>
    public interface IReaction<in TEvent> : IReaction where TEvent:IEvent
    {
        
    }


    public interface IReaction: IObserver<IEvent>, IObserver<CommandRunStateEventArgs>
    {
        string Tag { get; }
    }




}
