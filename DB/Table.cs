﻿using DB.Attributes;
using DB.Extensions;
using Extensions;
using Force.DeepCloner;
using MySql.Data.MySqlClient;
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

        //List<Row> DataRows { get; private set; }

        //List<T> GetRows<T>() where T : Row, new(); 
    }

    public abstract class Table<R> : ITable where R : Row, new()
    {
        private Executor _exe = Executor.Instance();
        private Column ColPK;
        public List<R> Rows = new List<R>();
        private List<R> RowsOld = new List<R>();
        public List<Column> Cols = new List<Column>();
        public string TableName { get; set; }

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

        public IEnumerable<Row> GetRowsModified()
        {
            //Sadly, for now we have to set DataRowsState.Modified manually.
            //Maybe, I write my own dataset generator in the future
            //Once I have written more code.

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

            var query = new Query($"DELETE FROM {TableName} WHERE {ColPK.Name} IN (@Deleted_Ids);");
            var ids = new List<object>();

            foreach (var row in rows)
            {
                ids.Add(row.GetValue(ColPK));
            }

            query.AddParamIds("@Deleted_Ids", ids);
            _exe.AddQuery(query);
        }

        void ProcessRowsUpdated()
        {
            var rows = GetRowsModified();
            if (!rows.Any()) return;

            var queryGroup = new QueryGroup("UPDATED");
            queryGroup.MergeQueries = true;

            foreach (var row in rows)
            {
                var query = new Query();
                var sql = $"UPDATE {TableName} SET ";

                for (var i = 0; i < Cols.Count; i++)
                {
                    query.AddParam($"@update_id{i}", row.GetValue(Cols[i]));
                    sql += $"{Cols[i].Name} = @update_id{i},";
                }

                sql = sql.StripLastChar(",");
                sql += $" WHERE {ColPK.Name} = @id;";

                query.SQL = sql;
                query.AddParam("@id", row.GetValue(ColPK));

                queryGroup.AddQuery(query);
            }

            _exe.AddQuery(queryGroup);
        }

        void ProcessRowsInserted(bool retrieveIds = false)
        {
            var rows = GetRowsAdded();
            if (!rows.Any()) return;

            var queryPreID = new Query();
            var queryPostID = new Query();

            queryPreID.Name = "PreID";
            queryPostID.Name = "PostID";

            if (retrieveIds)
            {
                queryPreID.SQL = $"SELECT MAX({ColPK.Name}) INTO @FIRST_INSERT_ID FROM {TableName};";
                queryPreID.AddParamMustRenameThis("@FIRST_INSERT_ID", DBType.Int32);
                _exe.AddQuery(queryPreID);
            }

            var rowIndex = 0;
            var queryGroup = new QueryGroup("INSERTED");

            foreach (var row in rows)
            {
                var query = new Query();
                var sql = $"INSERT INTO {TableName}";
                var columns = "";
                var values = "";

                for (var j = 0; j < Cols.Count; j++)
                {
                    var col = Cols[j];

                    columns += col.Name + ",";
                    values += $"@insert_id{rowIndex}_{j},";

                    query.AddParam($"@insert_id{rowIndex}_{j}", row.GetValue(col), col.DBType);
                }

                columns = columns.StripLastChar(",");
                values = values.StripLastChar(",");

                query.SQL = $"{sql} ({columns}) VALUES ({values});";
                queryGroup.AddQuery(query);

                rowIndex++;
            }

            _exe.AddQuery(queryGroup);

            if (retrieveIds)
            {
                var sql = $@"SELECT 
                (
                    SELECT `{ColPK.Name}` FROM `{TableName}` 
                    WHERE {ColPK.Name} > @FIRST_INSERT_ID OR @FIRST_INSERT_ID IS NULL
                    LIMIT 1
                ) AS FIRST_INSERT_ID,
                (
	                SELECT LAST_INSERT_ID()
                );";

                queryPostID.SQL = sql;
                _exe.AddQuery(queryPostID);
            }
        }

        public void Update()
        {
            if (Rows.Count == 0) return;
            if (ColPK == null) throw new NotSupportedException("Cannot Update() without a primary key");

            ProcessRowsDeleted();
            ProcessRowsUpdated();
            ProcessRowsInserted();

            //_exe.TransactionBegin();
            _exe.ExecuteQueries();
            //_exe.TransactionCommit();
        }

        /* REST */
        
        public Table()
        {
            var members = typeof(R).GetMembers().Where(x => x.DeclaringType == typeof(R)); ;

            foreach (var member in members)
            {
                var attrs = (ColAttribute[])member.GetCustomAttributes(typeof(ColAttribute), false);

                foreach (var attr in attrs)
                {
                    ColAdd(member, attr.DBType, attr.IsPrimaryKey, attr.IsAutoIncrement);
                }
            }
        }

        /**********************************************
        COLUMNS
        **********************************************/
        private void ColAdd(MemberInfo member, DBType type, bool isPrimaryKey, bool isAutoIncrement)
        {
            if (isPrimaryKey)
            {
                ColPK = new Column(member, true, isAutoIncrement, type);
            }
            else
            {
                Cols.Add(new Column(member, false, false, type));
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
            RowsOld.Add(row.DeepClone());
        }

        private void AddRowFromDB(R row)
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
            return RowsOld.Where(x => x.GetValueInt32(ColPK) == id).FirstOrNull();
        }
    }
}
