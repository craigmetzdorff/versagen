using System;
using System.Threading.Tasks;
using Versagen.Events.Commands;

namespace Versagen.Rules
{
    /// <summary>
    /// Default implementation of <see cref="IRule"/>
    /// </summary>
    public class LambdaRule : IConditionalRule
    {
        public string Tag { get; }

        public Func<ICommandContext, IServiceProvider, Task<(bool, string)>> __innerRule { get; }

        public string Name { get; }

        public Task<(bool, string)> CheckRule(ICommandContext context, IServiceProvider provider) => __innerRule.Invoke(context, provider);

        public LambdaRule(string Name, string tagName,
            Func<ICommandContext, IServiceProvider, Task<(bool, string)>> func)
        {
            this.Name = Name;
            Tag = tagName;
            __innerRule = func;
        }
            
    }
}
