using DB;
using DB.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DBTest.Model
{
    public class RowComposer : Row
    {
        public override void Init(DataRow row)
        {
            base.Init(row);

            Composer_id = SetInt32(row, 0);
            Name = SetString(row, 1);
        }

        [Col(IsAutoIncrement = true, IsPrimaryKey = true)]
        public Int32 Composer_id;

        [Col]
        public string Name;

    }
}
