using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.Entity;
using System.Threading.Tasks;

namespace Versagen.Entity
{
    public interface IEntityFactory<T> where T : IEntity
    {
        IQueryable<T> EntityTemplates { get; }
    }
}
