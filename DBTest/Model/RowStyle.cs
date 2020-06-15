using DB;
using DB.Attributes;
using System;
using System.Data;

namespace DBTest.Model
{
    public class RowStyle : Row
    {
        public override void Init(DataRow row)
        {
            //TODO, make func, set if exists

            Style_id = SetInt16(row, 0);
            Alt_style_id = SetInt16Null(row, 1);
            Parent_style_id = SetInt16Null(row, 2);
            Name = SetString(row, 3);
            Weight = SetSByte(row, 4);
        }

        [Col(IsAutoIncrement = true, IsPrimaryKey = true)]
        public Int16 Style_id;

        [Col]
        public Int16? Alt_style_id;

        [Col]
        public Int16? Parent_style_id;

        [Col]
        public string Name;

        [Col]
        public sbyte Weight;
    }
}
