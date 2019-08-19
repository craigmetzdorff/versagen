using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Versagen.PlayerSystem;

namespace Versagen.ASPNET.Identity.EntityFramework
{
    public static class EntityFrameworkIDExtensions
    {
        
        public class VersaUserIDGenerator :ValueGenerator<VersaCommsID>
        {
            public static VersaCommsID RandomUserID()
            {
                var rand = new Random();
                byte[] buf = new byte[8];
                rand.NextBytes(buf);
                ulong resul = BitConverter.ToUInt64(buf, 0);
                return VersaCommsID.FromEnum(EVersaCommIDType.User, resul);
            }

            public override VersaCommsID Next(EntityEntry entry)
            {
                return RandomUserID();
            }

            public override bool GeneratesTemporaryValues { get; } = false;
        }







        public class IdentityPlayerConfig<TIdentity,TKey> :IEntityTypeConfiguration<TIdentity> where TIdentity:IdentityUser<TKey>,IPlayer where TKey:IEquatable<TKey>
        {
            public void Configure(EntityTypeBuilder<TIdentity> b)
            {
                //TODO: Set as Unique Index instead to allow programatic configuration.
                b.HasAlternateKey(nameof(IPlayer.VersaID));

                b.Property(e => e.VersaID)
                    .HasConversion(p => p.RawValue, u => new VersaCommsID(u))
                    .IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NEXT VALUE FOR versagen.IDEnumerator");

                b.Ignore(e => e.Currencies)
                    .Ignore(e => e.PlayerInventory)
                    .Ignore(e => e.OwnedCharacters);
                //.OwnsMany(e => e.OwnedCharacters.ToClasses(), a =>
                //    {
                //        a.ToTable("PlayerCharacterMap","versagen");
                //        a.HasForeignKey(nameof(IPlayer.VersaID));

                //    });
            }
        }

        public class IDPairingEntity
        {
            public ulong Owner { get; }
            public ulong Owned { get; }
        }

        public static ModelBuilder SupportIPlayerIdentity<TIdentity,TKey>(this ModelBuilder b) where TIdentity:IdentityUser<TKey>,IPlayer where TKey:IEquatable<TKey>
        {
            b.HasSequence<long>("IDEnumerator","versagen")
                .IncrementsBy(1)
                .StartsAt(Convert.ToInt64(VersaCommsID.FromEnum(EVersaCommIDType.User, ulong.MinValue)))
                .HasMax(Convert.ToInt64(VersaCommsID.FromEnum(EVersaCommIDType.User, ulong.MaxValue)))
                .IsCyclic();
            b.ApplyConfiguration(new IdentityPlayerConfig<TIdentity, TKey>());
            return b;
        }


    }
    public class VersaCommsIDClass
    {
        public ulong heldID { get; set; }

        public VersaCommsIDClass(VersaCommsID IDtoHold)
        {
            heldID = IDtoHold;
        }
        public static implicit operator VersaCommsID(VersaCommsIDClass cl) => new VersaCommsID(cl.heldID);
        public static implicit operator VersaCommsIDClass(VersaCommsID iD) => new VersaCommsIDClass(iD);


    }

    public static class HoldListMethods
    {
        public static IList<VersaCommsIDClass> ToClasses(this IList<VersaCommsID> IDs)
        {
            return new List<VersaCommsIDClass>(IDs.Select(c => new VersaCommsIDClass(c)));
        }

        public static IList<VersaCommsID> ToStructs(this IList<VersaCommsIDClass> IDs)
        {
            return new List<VersaCommsID>(IDs.Select(c => new VersaCommsID(c.heldID)));
        }
    }
}
