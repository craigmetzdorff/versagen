using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Versagen.PlayerSystem;

namespace Versagen
{
    /// <summary>
    /// Maybe I should treat this as a long, and if it uses the second byte it becomes a temporary location to store things in.
    /// </summary>
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Flags]
    public enum EVersaCommIDType : byte
    {
        Unknown = 0,
        Unicast = 1,
        Multicast = Unicast << 1,
        IsGameID = Multicast << 1,
        ComplexEntity = IsGameID << 1,
        CanSpeak = ComplexEntity << 1,
        UserOwned =  CanSpeak << 1,
        Area = UserOwned << 1,
        UsesSecondByte = Area << 1,

        Broadcast = Multicast | Unicast,
        User = Unicast,
        UserGroup = Multicast,
        Entity = IsGameID | Unicast,
        PlayerCharacter = IsGameID | Unicast | UserOwned | CanSpeak | ComplexEntity,
        Scenario = Broadcast | IsGameID,
        Location = Area | Multicast | IsGameID,

    }
    /// <summary>
    /// Represents a chattable destination with a ulong backing.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public struct VersaCommsID
    {
        public EVersaCommIDType IdType => (EVersaCommIDType)(RawValue >> 56);
        // ReSharper disable once InconsistentNaming
        public ulong SubID => RawValue & 0x00FFFFFFFFFFFF; //Ignore the first byte.
        public ulong RawValue { get; }

        /// <summary>
        /// Allows packing extra data into the ID as needed (although not sure if this will be really done or not).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index] => (byte) (RawValue >> (56 - (index * 8)));

        public VersaCommsID(ulong rawValue) => RawValue = rawValue;
        public override string ToString() => RawValue.ToString();
        public override bool Equals(object obj) => obj != null && ((obj is VersaCommsID vid &&
                                                   RawValue == vid.RawValue) || (obj is ulong uID &&
                                                                   RawValue == uID));
        public override int GetHashCode() => RawValue.GetHashCode();
        public static implicit operator ulong(VersaCommsID vCommsId) => vCommsId.RawValue;
        public static implicit operator VersaCommsID(ulong uID) => new VersaCommsID(uID);
        public static implicit operator VersaCommsID(int inID) => new VersaCommsID((ulong)Math.Round((double)Math.Abs(inID)));

        public static VersaCommsID Random(EVersaCommIDType type)
        {
            var rand = new Random();
            var buf = new byte[sizeof(ulong)];
            rand.NextBytes(buf);
            buf[0] = (byte)type;
            return BitConverter.ToUInt64(buf, 0);
        }

        //internal static void EnsureRangeHasSpace(VersaCommsID min, VersaCommsID max, ISet<VersaCommsID> IDs)
        //{
        //    ulong rangeCount = min - max;
        //}

        //internal static void EnsureRangeHasSpace(ISet<VersaCommsID> IDs)
        //{
        //    if (IDs.Count == int.MaxValue)
        //    {

        //    }
        //}

        public static VersaCommsID RandomOutsideRange(EVersaCommIDType type, IEnumerable<VersaCommsID> otherIDs) =>
            RandomOutsideRange(type, otherIDs.ToImmutableHashSet());

        public static VersaCommsID RandomOutsideRange(EVersaCommIDType type, params VersaCommsID[] otherIDs) =>
            RandomOutsideRange(type, otherIDs.ToImmutableHashSet());

        public static VersaCommsID RandomOutsideRange(EVersaCommIDType type, ISet<VersaCommsID> otherIDs)
        {
            VersaCommsID nID;
            do
            {
                nID = Random(type);
            } while (otherIDs.Contains(nID));
            return nID;
        }

        public static VersaCommsID FromEnum(EVersaCommIDType idType, ulong uID)
        {
            return ((ulong) idType << 56) + ((ulong.MaxValue >> 8) & uID);
        }

        public static VersaCommsID AddMaskBits(ulong baseID, params byte?[] bytesInOrder)
        {
            var newId = baseID;
            for (var i = bytesInOrder.Length-1; i > -1; i--)
            {
                if (!bytesInOrder[i].HasValue) continue;
                newId = newId & (0x00FFFFFFFFFFFFul >> (i+1)*8);
                var heldBits = ((ulong) bytesInOrder[i].Value) << (56 - i * 8);
                newId = newId & heldBits;
            }
            return new VersaCommsID(newId);
        }

        internal bool ReferenceEquals(VersaCommsID playerId)
        {
            return true;
        }
    }
}
