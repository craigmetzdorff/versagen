using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    /// <summary>
    /// A command that takes a particular kind of comtext object in order to run a function upon it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVersaCommand<in T> :IVersaCommand where T:ICommandContext
    {
        Task Run(T args, IServiceProvider provider);
    }

    /// <summary>
    /// <para>IMPORTANT: DO NOT INHERIT THIS INTERFACE DIRECTLY AS IT HAS NO IDEA HOW TO RUN ITS OWN DO METHOD.</para>
    /// <para>A non-generic version of <see cref="IVersaCommand{T}"/> that exists to ensure that commands using multiple types of contexts can exist in the same list and be pattern-matched upon later.</para>
    /// </summary>
    public interface IVersaCommand
    {
        string CommandLine { get; }
        string Name { get; }
        EVersaPerms RequiredPrivilegdes { get; }
        string Description { get; }
        IConditionalRule[] Rules { get; }
        ECommandRunSpace RunSpace { get; }
        //TODO: never implemented in CommandService
        bool RunSynchronously { get; }
        //TODO: keep?
        char SeparatorChar { get; }
        IVersaCommand WithCommandLinePrefix(string prependThis);
        IEnumerable<Func<Task<(bool passed, IConditionalRule rule, string failureReason)>>> PrepRuleTasks(
            ICommandContext context, IServiceProvider provider);
    }

}