using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace DB
{
    public class DB
    {
        #region Singleton
        private DB() { }
        private static DB _instance = null;
        public static DB Instance()
        {
            if (_instance == null)
            {
                _instance = new DB();
            }

            return _instance;
        }
        #endregion

        private Executor _exe = Executor.Instance();
        public List<ITable> Tables;
        private string _connectionString;

        #region General
        private void WriteLine(string s)
        {
            Debug.WriteLine($"DB :: {s}");
        }
        
        private void WriteLine(string method, string s)
        {
            Debug.WriteLine($"DB :: {method} : {s}");
        }
        #endregion General

        public void Fill()
        {
            Tables.ForEach(t => t.Fill());
        }

        public void Update()
        {
            Tables.ForEach(t => t.Update());
        }

        public void TransactionBegin()
        {
            Tables.ForEach(t => t.TransactionBegin());
        }

        public void TransactionCommit()
        {
            Tables.ForEach(t => t.TransactionCommit());
        }


        public string ConnectionString
        {
            get => _connectionString;
            set 
            {
                _exe.SetConnection(value);
                _connectionString = value;
            }
        }

        public T AddTable<T>() where T : ITable, new()
        {
            if (Tables == null) Tables = new List<ITable>();
            var table = (T)Activator.CreateInstance(typeof(T));
            Tables.Add(table);
            return table;
        }

        //public T TableAdd<T>() where T : Table<Row>
        //{
        //    if (Tables == null) Tables = new List<Table<Row>>();
        //    var table = (T)Activator.CreateInstance(typeof(T));
        //    Tables.Add(table);
        //    return table;
        //}




    }
    }
