using System.Collections.Generic;

namespace Versagen.Rules
{
    public interface IRuleSet<T>: ISet<T> where T:IRule
    {
        IEnumerable<TRule> GetRulesByType<TRule>() where TRule : T;

        T GetRule(string Tag);

        TRule GetRule<TRule>(string Tag) where TRule : T;
    }
}
