using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DB
{
    public class Column
    {
        public Column(MemberInfo member, bool isPrimaryKey = false, bool isAutoIncrement = false, DBType type = DBType.None)
        {
            DBType = type;
            Member = member;
            Name = member.Name;
            IsPrimaryKey = isPrimaryKey;
            IsAutoIncrement = isAutoIncrement;
        }

        public DBType DBType;
        public string Name;
        public MemberInfo Member { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
