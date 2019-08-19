using System;
using System.Collections.Generic;
using System.Text;
using Versagen.Events.Commands;

namespace Versagen
{
    public interface IDescribable
    {
        VersaDescription Description { get; set; }
        VersaDescription GetDefaultDescription();
        string PrintFullDescription(ICommandContext context, IServiceProvider provider);

    }
}
