using System;

namespace DB.Tools
{
    public static class Converter
    {
        private static IFormatProvider _fmt = System.Globalization.CultureInfo.InvariantCulture;

        public static string ToString(object value)
        {
            return Convert.ToString(value, _fmt);
        }

        public static Int16 ToInt16(object value)
        {
            return Convert.ToInt16(value, _fmt);
        }

        public static Int32 ToInt32(object value)
        {
            return Convert.ToInt32(value, _fmt);
        }

        public static Byte ToByte(object value)
        {
            return Convert.ToByte(value, _fmt);
        }

        public static SByte ToSByte(object value)
        {
            return Convert.ToSByte(value, _fmt);
        }
    }
}
