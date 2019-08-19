using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Events.Commands
{
    public class ActUponCommandBase<TContext>:VersaCommandBase<TContext> where TContext : ICommandContext
    {
        public ActUponCommandBase(ICommandBuilder<TContext, ActUponCommandBase<TContext>> b) : base(b)
        {
            
        }
    }
}
