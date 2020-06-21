using DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBTest.Model
{
    public class TableComposers : Table<RowComposer>
    {
        public TableComposers()
        {
            TableName = "Composers";
        }
    }
}
