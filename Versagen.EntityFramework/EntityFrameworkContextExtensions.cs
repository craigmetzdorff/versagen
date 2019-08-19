using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Versagen.EntityFramework
{
    public static class VersagenIDUniqueEnforcer
    {
        [DbFunction("RAND")]
        public static double? Rand(int? seed)
        {
            return (seed.HasValue ? new Random(seed.Value) : new Random()).NextDouble();
        }
    }


    public static class EntityFrameworkContextExtensions
    {
        [Obsolete("Not yet properly supported in EF; will adapt later if support for universal type providers is added.")]
        public static ModelBuilder AddVersagenTypeSupport(this ModelBuilder b)
        {
             foreach (var item in b.Model.GetEntityTypes().Where(c => c.GetProperties().Any(p => p.ClrType.TypeHandle.Equals(typeof(VersaCommsID).TypeHandle)))
                 .SelectMany(c => c.GetProperties().Where(p => p.ClrType.TypeHandle.Equals(typeof(VersaCommsID).TypeHandle)))
                 .Select(r => b.Entity(r.DeclaringEntityType.ClrType).Property(r.Name)))
             {
                 item.HasConversion<ulong>();
             }
            return b;
        }

    }
}
