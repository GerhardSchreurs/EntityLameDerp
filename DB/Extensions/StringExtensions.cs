using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return s == null || s.Length == 0;
        }

        public static bool IsNotNullOrEmpty(this string s)
        {
            return !IsNullOrEmpty(s);
        }

        public static string Reverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static int ToInt(this string s)
        {
            return Int32.Parse(s);
        }

        public static bool IsNumeric(this string s)
        {
            double test;
            return double.TryParse(s, out test);
        }

        public static string StripLastChar(this string s, string strip)
        {
            if (s == null) return null;

            if (s.EndsWith(strip, StringComparison.InvariantCulture))
            {
                return s.Substring(0, s.LastIndexOf(strip, StringComparison.InvariantCulture));
            }

            return s;
        }
    }
}
