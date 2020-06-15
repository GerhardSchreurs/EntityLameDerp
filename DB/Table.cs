using DB.Attributes;
using DB.Extensions;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using TMCWorkbench.Extensions;

namespace DB
{
    public interface ITable
    {
        //private void ColAdd(MemberInfo member, bool isPrimaryKey, bool isAutoIncrement);

        void Fill();
        void Update();
        string TableName { get; set; }
    }

    public abstract class Table<R> : ITable where R : Row, new()
    {
        private Executor _exe = Executor.Instance();

        public void Fill()
        {
            Rows.Clear();

            using (var table = _exe.GetTable($"SELECT * FROM {TableName};"))
            {
                foreach (DataRow row in table.Rows)
                {
                    var dbrow = new R();
                    dbrow.Init(row);
                    AddRowFromDB(dbrow);
                }
            }
        }

        public IEnumerable<Row> GetRowsDeleted()
        {
            return Rows.Where(x => x.DataRowState == DataRowState.Deleted);
        }

        void ProcessRowsDeleted()
        {
            var rows = GetRowsDeleted();
            if (!rows.Any()) return;

            var query = new Query($"DELETE FROM {TableName} WHERE {ColPK.Name} IN (@ids)");
            var ids = new List<object>();

            foreach (var row in rows)
            {
                ids.Add(row.GetValue(ColPK));
            }

            query.AddParamIds("@Deleted_Ids", ids);
            _exe.Queries.Add(query);
        }

        public void Update()
        {
            if (Rows.Count == 0) return;
            if (ColPK == null) throw new NotSupportedException("Cannot UpdateData() without a primary key");

            ProcessRowsDeleted();
        }

        /* REST */
        
        private Column ColPK;
        public List<R> Rows = new List<R>();
        private List<R> _rowsOld = new List<R>();
        private List<Column> Cols = new List<Column>();

        public string TableName { get; set; }

        public Table()
        {
            var members = typeof(R).GetMembers().Where(x => x.DeclaringType == typeof(R)); ;

            foreach (var member in members)
            {
                var attrs = (ColAttribute[])member.GetCustomAttributes(typeof(ColAttribute), false);

                foreach (var attr in attrs)
                {
                    ColAdd(member, attr.IsPrimaryKey, attr.IsAutoIncrement);
                }
            }
        }

        /**********************************************
        COLUMNS
        **********************************************/
        private void ColAdd(MemberInfo member, bool isPrimaryKey, bool isAutoIncrement)
        {
            if (isPrimaryKey)
            {
                ColPK = new Column(member, isPrimaryKey, isAutoIncrement);
            }
            else
            {
                Cols.Add(new Column(member, isPrimaryKey, isAutoIncrement));
            }
        }

        /**********************************************
        ROWS
        **********************************************/
        public R NewRow()
        { 
            var row = new R();
            row.DataRowState = DataRowState.Detached;
            return row;
        }

        public void AddRow(R row)
        {
            row.DataRowState = DataRowState.Added;
            Rows.Add(row);
            _rowsOld.Add(row.DeepClone());
        }

        private void AddRowFromDB(R row)
        {
            row.DataRowState = DataRowState.Unchanged;
            Rows.Add(row);
            _rowsOld.Add(row.DeepClone());
        }

        public void DeleteRowByIndex(int index)
        {
            Rows[index].DataRowState = DataRowState.Deleted;
            _rowsOld[index].DataRowState = DataRowState.Deleted;
        }

        public void DeleteRowById(int id)
        {
            GetRowById(id).DataRowState = DataRowState.Deleted;
            GetOldRowById(id).DataRowState = DataRowState.Deleted;
        }

        public void DeleteRow(R row)
        {
            DeleteRowById(row.GetValueInt32(ColPK));
        }

        public R GetRowById(int id)
        {
            return Rows.Where(x => x.GetValueInt32(ColPK) == id).FirstOrNull();
        }

        private R GetOldRowById(int id)
        {
            return _rowsOld.Where(x => x.GetValueInt32(ColPK) == id).FirstOrNull();
        }
    }
}
