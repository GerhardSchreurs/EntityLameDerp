using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMCWorkbench.Extensions
{
    public static class ObjectExtensions
    {
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static bool IsNumeric(this object value)
        {
            if (value == null) { return false; }
            var s = value.ToString();
            return IsNumeric(s);
        }

    }
}
