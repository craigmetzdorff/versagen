using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Versagen.Events.Commands;

namespace Versagen.Rules
{
    public interface IDescriptiveRule :IRule
    {
        [Pure]
        string GetDescriptionPart(ICommandContext context);
    }
}
