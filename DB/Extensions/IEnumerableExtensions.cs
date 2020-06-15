using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMCWorkbench.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T FirstOrNull<T>(this IEnumerable<T> values) where T : class
        {
            return values.DefaultIfEmpty(null).FirstOrDefault();
        }
    }
}
