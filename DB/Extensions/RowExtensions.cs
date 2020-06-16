using DB.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DB.Extensions
{
    public static class RowExtensions
    {
        public static object GetValue(this Row row, Column col)
        {
            return row.GetType().GetField(col.Member.Name).GetValue(row);
        }

        public static string GetValueString(this Row row, Column col)
        {
            var value = GetValue(row, col);

            if (value == null) return string.Empty;
            return value.ToString();
        }

        public static int GetValueInt32(this Row row, Column col)
        {
            return Converter.ToInt32(GetValue(row, col));
        }

        public static object GetValueDbNull(this Row row, Column col)
        {
            var value = GetValue(row, col);

            if (value == null)
            {
                value = DBNull.Value;
            }

            return value;
        }

    }
}
