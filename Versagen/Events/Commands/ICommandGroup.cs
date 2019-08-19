using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    /// <summary>
    /// Represents an arbitrary number of commands grouped together in an easy-to-understand manner.
    /// </summary>
    public interface ICommandGroup : IEnumerable<IVersaCommand>
    {
        string FriendlyName { get; }
        string Prefix { get; }

        IEnumerable<IVersaCommand> AllCommandsFullPaths { get; }

        /// <summary>
        /// Returns false when the command is already present.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        bool TryAdd(IVersaCommand command);
        bool TryRemove(IVersaCommand command);

        /// <summary>
        /// Search tag for retrieving this command from a central storage of commands.
        /// TODO: not implemented.
        /// </summary>
        string Tag { get; }
        ICollection<Rules.IConditionalRule> Preconditions { get; }
        bool TryFindCommand(string externalPrefix, IMessageEvent e,
            out IVersaCommand command, out string commandMatchString);
    }

    /// <summary>
    /// A comparer used to order commands in the command list. Used to ensure that longer commands will be checked first.
    /// </summary>
    public class CommandExecOrderer :IComparer<IVersaCommand>,IComparer<string>, IComparer<ICommandGroup>
    {
        public int Compare(IVersaCommand x, IVersaCommand y) => Compare(x.CommandLine, y.CommandLine);

        public int Compare(string x, string y)
        {
            Debug.Assert(x != null, nameof(x) + " != null");
            Debug.Assert(y != null, nameof(y) + " != null");
            var lengthDiff = (x.Length - y.Length);
            return lengthDiff == 0 ? string.Compare(x, y, StringComparison.Ordinal) : -lengthDiff;
        }

        public int Compare(ICommandGroup x, ICommandGroup y) => Compare(x.Prefix, y.Prefix);
    }
}
