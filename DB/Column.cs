using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DB
{
    public class Column
    {
        public Column(MemberInfo member, bool isPrimaryKey, bool isAutoIncrement)
        {
            Member = member;
            Name = member.Name;
            IsPrimaryKey = isPrimaryKey;
            IsAutoIncrement = isAutoIncrement;
        }

        public string Name;
        public MemberInfo Member { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
