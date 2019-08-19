using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Versagen.Structs
{
    /// <summary>
    /// Reliance on <see cref="N:System.Drawing" /> in .NET Core is being discouraged at the moment, so instead Versagen uses its own Color structure.
    /// NOTE: Assume all numbers are in the 8-digit RGBA format specified by CSS4.
    /// The recommended method of changing colors is by and-ing and or-ing bits together.
    /// </summary>
    [System.Serializable]
    // ReSharper disable once CommentTypo
    // ReSharper disable once InheritdocConsiderUsage
    public struct Color : ISerializable
    {
        public uint RawValue { get; }
        public byte Red => (byte) (RawValue >> 24);
        public byte Green => (byte) (RawValue >> 16);
        public byte Blue => (byte)(RawValue >> 8);
        public byte Alpha => (byte)RawValue;
        public Color AsOpaque => new Color(RawValue | 0xFFu);
        public static Color Black = 0xFFu;
        public static Color White = 0xFFFFFFFFu;
        public Color Mix(Color other) => (Color)(other.RawValue | RawValue);
        public static Color FromRGBA(uint rawValue) => new Color(rawValue);
        public static Color FromRGBA(byte R, byte G, byte B, byte A) => new Color((uint)((R << 24) | (G << 16) | (B << 8) | A));
        public static Color FromRGB(uint rawOpaque) => new Color(Black | (rawOpaque << 8));
        public static Color FromRGB(byte R, byte G, byte B) => FromRGBA(R, G, B, 255);
        public static Color FromCSSHex(string cssHex)
        {
            //Support duplicating values of bytes to comply with CSS short notation.
            byte DupByte(char singleCode)
            {
                var dupVal = Convert.ToByte(singleCode.ToString(), 16);
                return (byte)((dupVal << 4) | dupVal);
            }
            var workWith = cssHex.Trim('#').Trim();
            switch (workWith.Length)
            {
                case 6:
                    return FromRGB(Convert.ToUInt32(workWith, 16));
                case 8:
                    return new Color(Convert.ToUInt32(workWith, 16));
                case 3:
                    var r = DupByte(workWith[0]);
                    var g = DupByte(workWith[1]);
                    var b = DupByte(workWith[2]);
                    return FromRGB(r, g, b);
                case 4:
                    r = DupByte(workWith[0]);
                    g = DupByte(workWith[1]);
                    b = DupByte(workWith[2]);
                    var a = DupByte(workWith[3]);
                    return FromRGBA(r, g, b, a);
                default:
                    throw new FormatException("The length of the input string is not valid.");
            }
        }

        public string ToStringCssRGBA()
        {
            return "#" + RawValue.ToString("X8");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(RawValue), RawValue);
        }
        internal Color(uint rawValue)
        {
            RawValue = rawValue;
        }
        public Color(SerializationInfo info, StreamingContext context)
        {
            RawValue = (uint)info.GetValue(nameof(RawValue), typeof(uint));
        }
        public static implicit operator uint(Color color) => color.RawValue;
        //TODO:Implicit or explicit?
        public static implicit operator Color(uint rawValue) => new Color(rawValue);
    }
}
