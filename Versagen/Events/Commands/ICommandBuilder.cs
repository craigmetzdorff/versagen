using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    enum VersaComCallMethods
    {
        Lambda,
        MethodInfo,
        MethodInfoAndClass
    }

    /// <summary>
    /// TODO: Change this so that it takes the ICommandContext as invariant, or at least takes a context SOMEHOW
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public interface ICommandBuilder<C, out V> where C:ICommandContext where V : IVersaCommand<C>
    {
        string CallLine { get; set; }
        ECommandRunSpace ChatContext { get; set; }
        string Description { get; set; }
        Func<C, Task> Do { get; set; }
        string Name { get; set; }
        List<IConditionalRule> Predicates { get; set; }
        EVersaPerms RequiredPrivilegdes { get; set; }
        bool RunSynchronously { get; set; }
        char SplitOn { get; set; }
        object[] additionalConstructorArgs { get; set; }
        Type TansientClassNeeded { get; set; }
        Func<IDisposable, Func<C, Task>> TransientClassMethod { get; set; }
        ICommandBuilder<C, V> UsingTransientClass<T>(Func<T, Func<C, Task>> methodToCall, params object[] additionalConstructorArgs) where T : IDisposable;

        V Build();
    }
}