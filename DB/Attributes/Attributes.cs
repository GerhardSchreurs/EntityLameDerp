
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DB.Attributes
{

    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    //public class ColIsAutoIndex : System.Attribute
    //{
    //}

    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    //public class ColIsID : System.Attribute
    //{
    //}

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ColAttribute : Attribute
    {
        public ColAttribute(MySqlDbType type)
        {
            DBType = type;
        }

        private bool _isPrimaryKey;
        private bool _isAutoIncrement;
        private MySqlDbType _type;

        public MySqlDbType DBType
        {
            get => _type;
            set => _type = value;
        }

        public bool IsPrimaryKey
        {
            get => _isPrimaryKey;
            set => _isPrimaryKey = value;
        }

        public bool IsAutoIncrement
        {
            get => _isAutoIncrement;
            set => _isAutoIncrement = value;
        }
    }
}
