using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    public class VersaCommand : VersaCommandBase<CommandContext>
    {
        public class Builder : ICommandBuilder<CommandContext, VersaCommand>
        {
            /// <summary>
            /// Check to see if a command is allowed in a given group.
            /// </summary>
            public Func<CommandContext, Task> Do { get; set; }
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
            public Type TansientClassNeeded { get; set; }
            public Func<IDisposable, Func<CommandContext, Task>> TransientClassMethod { get; set; }

            public VersaCommand Build() => new VersaCommand(this);

            public ICommandBuilder<CommandContext, VersaCommand> UsingTransientClass<T>(Func<T, Func<CommandContext, Task>> methodToCall, params object[] additionalConstructorArgs) where T : IDisposable
            {
                TansientClassNeeded = typeof(T);
                TransientClassMethod = (o) => methodToCall.Invoke((T)o);
                this.additionalConstructorArgs = additionalConstructorArgs;
                return this;
            }
        }

        internal object FindCommand(Func<object, object> p)
        {
            throw new NotImplementedException();
        }

        protected VersaCommand(Builder b) : base(b) { }
    }
}

