using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Versagen.Events.Commands;

namespace Versagen.Rules
{
    public interface IConditionalRule : IRule
    {
        [Pure]
        Task<(bool, string)> CheckRule(ICommandContext context, IServiceProvider provider);
    }
}
