using System.Collections.Generic;

namespace Versagen.Rules
{
    public interface IRuleEnumerable<T>:IEnumerable<T> where T:IRule
    {

        
    }
}
