using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Versagen.Entity;
using Versagen.IO;
using Versagen.PlayerSystem;

namespace Versagen.Events
{
    public class MessageRepeater : ObserverBase<IEvent>
    {
        private string[] _ignoreWithPrefixes;

        private readonly Func<IMessageEvent, bool> _allowRepeat;

        private static bool WordListDelegate(IMessageEvent me, bool asWhiteList, IEnumerable<string> prefixes) =>
            asWhiteList ? prefixes.Any(me.FullMessage.StartsWith) : prefixes.Any(x => !me.FullMessage.StartsWith(x));

        public MessageRepeater(Func<IMessageEvent, bool> allowRepeat)
        {
            _allowRepeat = allowRepeat;
        }

        public MessageRepeater(bool asWhiteList, params string[] prefixes)
        {
            //if (asWhiteList && ignoreWithPrefix.Length == 0)
            //    throw new ArgumentOutOfRangeException(nameof(ignoreWithPrefix),"Can't use a whitelist with no contents!");
            _allowRepeat = me => WordListDelegate(me, asWhiteList, prefixes);
        }

        public MessageRepeater()
        {
            _allowRepeat = _ => true;
        }

        protected override async void OnNextCore(IEvent value)
        {
            if (!(value is IMessageEvent me)) return;
            if (!(me.Terminal.Obj is IVersaWriter vw)) return;
            if (!_allowRepeat(me)) return;
            await vw.WritePreambleAsync($"{(me.Entity.Obj is IEntity ent ? ent.Name : me.Player.Obj is IPlayer playa ? playa.UserName : "?????")}: ");
            await vw.WriteLineAsync($"{me.FullMessage}")
                .ConfigureAwait(false);
        }

        protected override void OnErrorCore(Exception error)
        {
            //ignored (currently no way to repeat the messages this way).
        }

        protected override void OnCompletedCore()
        {
            //ignored
        }
    }
}
