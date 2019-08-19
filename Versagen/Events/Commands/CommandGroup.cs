using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.Rules;

namespace Versagen.Events.Commands
{
    public class CommandGroup : ICommandGroup
    {
        private SortedDictionary<string,IVersaCommand> _commands { get; }

        private SortedDictionary<string, IVersaCommand> _subGroups { get; }

        public string FriendlyName { get; }

        //TODO: Put an appropriate backing on this.
        public ICollection<IConditionalRule> Preconditions { get; }

        public string Prefix { get; }

        public IEnumerable<IVersaCommand> AllCommandsFullPaths =>
            _commands.Values.Select(c => c.WithCommandLinePrefix(Prefix));

        public string Tag { get; }

        public void Add(IVersaCommand command)
        {
            if(TryAdd(command)==false)
                throw new ArgumentException("Duplicate command already present in system.", nameof(command));
        }

        public IEnumerator<IVersaCommand> GetEnumerator()
        {
            return _commands.Values.GetEnumerator();
        }

        public bool TryAdd(IVersaCommand command)
        {
            if (_commands.ContainsKey(command.CommandLine)) return false;
            _commands.Add(command.CommandLine, command);
            return true;

        }

        public bool TryFindCommand(string externalPrefix, IMessageEvent e, out IVersaCommand command,
            out string commandMatchString)
        {
            string finalPrefix;
            if (!string.IsNullOrWhiteSpace(Prefix))
                finalPrefix = (externalPrefix + Prefix).Trim() + " ";
            else finalPrefix = externalPrefix;
            foreach (var item in _commands)
            {
                if (!e.FullMessage.StartsWith(finalPrefix + item.Key)) continue;
                commandMatchString = finalPrefix + item.Key;
                command = item.Value;
                return true;
            }
            commandMatchString = default;
            command = default;
            return false;
        }

        public bool TryRemove(IVersaCommand command) => _commands.Remove(command.CommandLine);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_commands).GetEnumerator();
        }

        public CommandGroup(string prefix, string tag, string friendlyName = null)
        {
            Prefix = prefix;
            Tag = tag;
            FriendlyName = friendlyName ?? Tag;
            Preconditions = new List<IConditionalRule>();
            _subGroups = new SortedDictionary<string, IVersaCommand>();
            _commands = new SortedDictionary<string, IVersaCommand>(new CommandExecOrderer());
        }

        //public IVersaCommand this[int index] { get => this[index]; set => commandList[index] = value; }

        //private object[] commandList = new object[8];
        //private int count;

        //private List<IVersaCommand> _coms;

        //public string FriendlyName { get; set; }

        //public string Prefix { get; set; }

        //public string Tag { get; set; }

        //public IEnumerable<IConditionalRule> Preconditions { get => Preconditions; }

        //public int Count => commandList.Count();

        //public bool IsReadOnly { get; }

        //public void Add(IVersaCommand item) => _coms.Add(item);

        //public void Clear() => count = 0;

        //public bool Contains(IVersaCommand item) => commandList.Contains(item);

        //public void CopyTo(IVersaCommand[] array, int arrayIndex)
        //{
        //    commandList.CopyTo(array, arrayIndex);
        //}

        //public IEnumerator<IVersaCommand> GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        //public int IndexOf(IVersaCommand item)
        //{
        //    return IndexOf(item);
        //}

        //public void Insert(int index, IVersaCommand item)
        //{
        //    if (!commandList.Contains(index))
        //    {
        //        commandList[index] = item;
        //        count++;
        //    }
        //}

        //public bool Remove(IVersaCommand item) {
        //    if (commandList.Contains(item))
        //    {
        //        RemoveAt((IndexOf(item)));
        //        return true;
        //    }
        //    else { return false; }
        //}

        //public void RemoveAt(int index)
        //{
        //    RemoveAt(index);
        //    count--;
        //}
    }
}
