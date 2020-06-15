using DB;
using System;
using System.Linq;

namespace DBTest.Model
{
    public class TableStyles : Table<RowStyle>
    {
        public TableStyles()
        {
            this.TableName = "Styles";
        }

        public void DoSomething(IRow row)
        {

        }


        //TODO: Move to base class
        //public RowStyle FindById(Int16 id)
        //{
        //    return Rows.Where(x => x.Style_id == id).First();
        //}
    }
}
