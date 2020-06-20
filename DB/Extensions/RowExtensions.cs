using DB.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DB.Extensions
{
    public static class RowExtensions
    {
        private static bool IsNullableType(Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static void SetFieldValue(FieldInfo field, object targetObj, object value)
        {
            object valueToSet;

            if (value == null || value == DBNull.Value)
            {
                valueToSet = null;
            }
            else
            {
                Type fieldType = field.FieldType;
                //assign enum
                if (fieldType.IsEnum)
                    valueToSet = Enum.ToObject(fieldType, value);
                //support for nullable enum types
                else if (fieldType.IsValueType && IsNullableType(fieldType))
                {
                    Type underlyingType = Nullable.GetUnderlyingType(fieldType);
                    valueToSet = underlyingType.IsEnum ? Enum.ToObject(underlyingType, value) : value;
                }
                else
                {
                    //we always need ChangeType, it will convert the value to the proper number type, for example.
                    valueToSet = Convert.ChangeType(value, fieldType);
                }
            }
            field.SetValue(targetObj, valueToSet);
        }

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

        public static void SetValueInt32(this Row row, Column col, int value)
        {
            var fieldInfo = row.GetType().GetField(col.Member.Name);
            SetFieldValue(fieldInfo, row, value);

            //var fieldInfo = row.GetType().GetField(col.Member.Name);
            //var fieldType = fieldInfo.FieldType;


            //Int32 x = 1;
            //fieldInfo.SetValue(row, x);

            //////////

            //var item = row.GetType().GetField(col.Member.Name);
            //var o = item.GetValue(row);

            //Type t = Nullable.GetUnderlyingType(item.FieldType) ?? item.FieldType;
            //object safeValue = (o == null) ? null : Convert.ChangeType(o, t);
            //item.SetValue(value, safeValue);



            //var p = dst.GetType().GetField(item.Name);
            //if (p != null)
            //{
            //    Type t = Nullable.GetUnderlyingType(p.FieldType) ?? p.FieldType;
            //    object safeValue = (o == null) ? null : Convert.ChangeType(o, t);
            //    p.SetValue(dst, safeValue);
            //}


            //row.GetType().GetField(col.Member.Name).SetValue(row, value);
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
