using Extensions;
using Force.DeepCloner;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DB.Attributes;
using DB.Extensions;
using TMCWorkbench.Extensions;

namespace DB
{
    public class C
    {
        public C(MemberInfo member, bool isPrimaryKey, bool isAutoIncrement)
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

    //public class Table<T> where T : Row, new()
    public class TableOld : ITable
    {
        private List<IRow> RowsOld = new List<IRow>();
        public List<IRow> Rows = new List<IRow>();
        public List<C> Cols = new List<C>();
        public C ColPK;

        public List<IRow> Rowz
        {
            get
            {
                return Rows.Where(x => x.DataRowState != DataRowState.Deleted || x.DataRowState == DataRowState.Detached).ToList();
            }
        }

        private string _tableName;

        public TableOld(string tableName)
        {
            _tableName = tableName;

            var members = typeof(T).GetMembers().Where(x => x.DeclaringType == typeof(T));

            foreach (var member in members)
            {
                var attrs = (Col[])member.GetCustomAttributes(typeof(Col), false);

                foreach (var attr in attrs)
                {
                    AddCol(member, attr.IsPrimaryKey, attr.IsAutoIncrement);
                }
            }

            //var type = typeof(T);
            //var properties = type.GetProperties().Where(x => x.DeclaringType == type);

            //foreach (var property in properties)
            //{
            //    var attrs = (Col[])property.GetCustomAttributes(typeof(Col), false);

            //    foreach (var attr in attrs)
            //    {
            //        AddCol(property, attr.IsPrimaryKey, attr.IsAutoIncrement);
            //    }
            //}
        }

        public T NewRow()
        {
            var row = new T();
            //var row = ProxyFactory.GetProxyInstance<T>();
            row.DataRowState = DataRowState.Detached;
            return row;
        }

        public void AddRow(T row)
        {
            row.DataRowState = DataRowState.Added;
            Rows.Add(row);
            RowsOld.Add(row.DeepClone());
        }

        private void AddRowFromDB(T row)
        {
            row.DataRowState = DataRowState.Unchanged;
            Rows.Add(row);
            RowsOld.Add(row.DeepClone());
        }

        public void DeleteRowByIndex(int index)
        {
            Rows[index].DataRowState = DataRowState.Deleted;
            RowsOld[index].DataRowState = DataRowState.Deleted;
        }

        public void DeleteRowById(int id)
        {
            GetRowById(id).DataRowState = DataRowState.Deleted;
            GetOldRowById(id).DataRowState = DataRowState.Deleted;
        }

        public void DeleteRow(T row)
        {
            DeleteRowById(row.GetValueInt32(ColPK));
        }


        public T GetRowById(int id)
        {
            return Rows.Where(x => x.GetValueInt32(ColPK) == id).FirstOrNull();
        }

        private T GetOldRowById(int id)
        {
            return RowsOld.Where(x => x.GetValueInt32(ColPK) == id).FirstOrNull();
        }

        private void AddCol(MemberInfo member, bool isPrimaryKey, bool isAutoIncrement)
        {
            if (isPrimaryKey)
            {
                ColPK = new C(member, isPrimaryKey, isAutoIncrement);
            }
            else
            {
                Cols.Add(new C(member, isPrimaryKey, isAutoIncrement));
            }
        }

        public async Task GetDataAsync()
        {
            await Task.Run(() => GetData());
        }

        public void GetData()
        {
            Rows.Clear();

            using (var table = DB.Executor.GetDataTable($"SELECT * FROM {_tableName};"))
            {
                foreach (DataRow row in table.Rows)
                {
                    var dbrow = new T();
                    dbrow.Init(row);
                    //Rows.Add(ProxyFactory.GetProxyInstance<T>(dbrow));
                    //RowsOld.Add(dbrow.DeepClone());

                    AddRowFromDB(dbrow);
                }
            }
        }

        public IEnumerable<Row> GetRowsDeleted()
        {
            return Rows.Where(x => x.DataRowState == DataRowState.Deleted);
        }

        public IEnumerable<Row> GetRowsModified()
        {
            //Sadly, for now we have to set DataRowsState.Modified manually.
            //Maybe, I write my own dataset generator in the future
            //Once I have written some more code.

            for (var i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];

                if (row.DataRowState == DataRowState.Unchanged)
                {
                    var rowOld = RowsOld[i];

                    foreach (var col in Cols)
                    {
                        var newValue = row.GetValue(col);
                        var oldValue = rowOld.GetValue(col);

                        if (oldValue == null && newValue == null)
                        {
                            continue;
                        }

                        if ((oldValue == null && newValue != null) || (oldValue != null && newValue == null) || (oldValue.Equals(newValue) == false))
                        {
                            //modified
                            row.DataRowState = DataRowState.Modified;
                            rowOld.DataRowState = DataRowState.Modified;
                            break;
                        }
                    }
                }
            }

            return Rows.Where(x => x.DataRowState == DataRowState.Modified);
        }
        public IEnumerable<Row> GetRowsAdded()
        {
            return Rows.Where(x => x.DataRowState == DataRowState.Added);
        }

        void ProcessRowsDeleted()
        {
            var rows = GetRowsDeleted();
            if (!rows.Any()) return;

            DB.Executor.QueryNew(DataRowState.Deleted);
            DB.Executor.QuerySetSQL(DataRowState.Deleted, $"DELETE FROM {_tableName} WHERE {ColPK.Name} IN (@ids)");

            var ids = new List<object>();

            foreach (var row in rows)
            {
                ids.Add(row.GetValue(ColPK));
            }

            DB.Executor.QueryAddParamIds(DataRowState.Deleted, "@ids", ids);
        }

        void ProcessRowsModified()
        {
            var rows = GetRowsModified();
            if (!rows.Any()) return;

            DB.Executor.QueryNew(DataRowState.Modified);

            foreach (var row in rows)
            {
                var sql = $"UPDATE {_tableName} SET ";

                for (var i = 0; i < Cols.Count; i++)
                {
                    DB.Executor.QueryAddParam(DataRowState.Modified, $"@update_id{i}", row.GetValue(Cols[i]));
                    sql += $"{Cols[i].Name} = @update_id{i},";
                }

                sql = sql.StripLastChar(",");
                sql += $" WHERE {ColPK.Name} = @id";

                DB.Executor.QuerySetSQL(DataRowState.Modified, sql);
                DB.Executor.QueryAddParam(DataRowState.Modified, "@id", row.GetValue(ColPK));
            }
        }

        void ProcessRowsAdded(bool retrieveIds = false)
        {
            var rows = GetRowsAdded();
            if (!rows.Any()) return;

            var firstId = -1;
            var lastId = -1;

            if (retrieveIds)
            {
                DB.Executor.QueryNew(DataRowState.Added);
                DB.Executor.QuerySetSQL(DataRowState.Added, $"SELECT MAX({ColPK.Name}) INTO @FIRST_INSERT_ID FROM {_tableName};");
                DB.Executor.QueryAddParam(DataRowState.Added, "@FIRST_INSERT_ID");
            }

            DB.Executor.QueryNew(DataRowState.Added);

            var rowIndex = 0;

            foreach (var row in rows)
            {
                var sql = $"INSERT INTO {_tableName}";
                var columns = "";
                var values = "";

                for (var j = 0; j < Cols.Count; j++)
                {
                    var col = Cols[j];

                    columns += col.Name + ",";
                    values += $"@insert_id{rowIndex}_{j},";

                    DB.Executor.QueryAddParam(DataRowState.Added, $"@insert_id{rowIndex}_{j}", row.GetValue(col));
                }

                columns = columns.StripLastChar(",");
                values = values.StripLastChar(",");

                DB.Executor.QuerySetSQL(DataRowState.Added, $"{sql} ({columns}) VALUES ({values})");

                rowIndex++;
            }

            if (retrieveIds)
            {
                DB.Executor.QueryNew(DataRowState.Added);

                var sql = $@"SELECT 
                (
                    SELECT `{ColPK.Name}` FROM `{_tableName}` 
                    WHERE {ColPK.Name} > @FIRST_INSERT_ID OR @FIRST_INSERT_ID IS NULL
                    LIMIT 1
                ) AS FIRST_INSERT_ID,
                (
	                SELECT LAST_INSERT_ID()
                )";

                DB.Executor.QuerySetSQL(DataRowState.Added, sql);
            }
        }

        public void UpdateData()
        {
            if (Rows.Count == 0) return;
            if (ColPK == null) throw new NotSupportedException("Cannot UpdateData() without a primary key");

            ProcessRowsDeleted();
            ProcessRowsModified();
            ProcessRowsAdded();

            DB.Executor.QueryProcess();

            CleanUp();
        }

        public void UpdateDataUpdateIds()
        {
            if (Rows.Count == 0) return;
            if (ColPK == null) throw new NotSupportedException("Cannot UpdateData() without a primary key");

            ProcessRowsDeleted();
            ProcessRowsModified();
            ProcessRowsAdded(true);

            DB.Executor.QueryProcess();

            CleanUp();
        }

        void CleanUp()
        {
            if (Rows.Count == 0) return;

            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                var row = Rows[i];

                if (row.DataRowState == DataRowState.Deleted)
                {
                    Rows.RemoveAt(i);
                    RowsOld.RemoveAt(i);
                }
                else if (row.DataRowState == DataRowState.Added || row.DataRowState == DataRowState.Modified)
                {
                    row.DataRowState = DataRowState.Unchanged;
                    RowsOld[i].DataRowState = DataRowState.Unchanged;
                }
            }
        }

        public void UpdateDataAndReflect(bool retrieveIdValues)
        {
        }

        public void UpdateDataAndRefresh()
        {

        }
    }
}
