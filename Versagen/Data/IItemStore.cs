using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Versagen.Entity;
using Versagen.Items;

namespace Versagen.Data
{
    interface IItemStore : IQueryable<IItem>
    {
        Task<IItem> GetItem(ulong id);
        Task<IItem> GetItemTemplate(ulong id);
        Task<IItem> GetItemTemplate(IItem item);
        Task<IQueryable<IItem>> GetInventoryForEntity(IEntity entity);
        Task<(bool, ulong)> Update(IItem item);
    }
}
