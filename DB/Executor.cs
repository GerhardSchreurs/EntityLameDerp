using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace DB
{
    public class Executor
    {
        #region Singleton
        private Executor() { }
        private static Executor _instance = null;
        public static Executor Instance()
        {
            if (_instance == null)
            {
                _instance = new Executor();
            }

            return _instance;
        }
        #endregion

        public List<QueryGroup> Queries;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;
        private Exception _exceptionTransaction;

        /**********************************************
        CONNECTIONS
        **********************************************/
        #region Connections
        public void SetConnection(string connectionString)
        {
            ConnectionClose();
            _connection = new MySqlConnection(connectionString);
        }

        private void ConnectionClose()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                if (_connection.State == ConnectionState.Executing)
                {
                    throw new Exception("Cannot set connection, connection is executing");
                }

                _connection.Close();
            }
        }

        private void ConnectionOpen()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }
        #endregion Connections

        /**********************************************
        TRANSACTIONS
        **********************************************/
        #region TransActions
        public bool TransactionActive
        {
            get
            {
                return _transaction != null;
            }
        }

        public void TransactionBegin()
        {
            ConnectionOpen();
            _transaction = _connection.BeginTransaction();
        }

        public void TransactionCommit()
        {
            if (_exceptionTransaction != null) { throw _exceptionTransaction; }
            if (_transaction == null) { throw new Exception("Transaction is null"); }

            _transaction.Commit();
        }

        public void TransactionRollback(Exception ex = null)
        {
            _transaction.Rollback();
            _exceptionTransaction = ex;
            _connection.Close();
        }
        #endregion Transactions

        /**********************************************
        DATATABLE
        **********************************************/
        public DataTable GetTable(string sql)
        {
            var table = new DataTable();
            using (var dataAdapter = new MySqlDataAdapter(sql, _connection))
            {
                dataAdapter.Fill(table);
            }
            return table;
        }

        /**********************************************
        QUERY TO COMMAND
        **********************************************/
        public MySqlCommand QueryToCommand(Query query)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = query.SQL;
            cmd.CommandType = query.CommandType;
            cmd.Connection = _connection;

            if (query.Parameters != null)
            {
                foreach (var param in query.Parameters)
                {
                    if (param.IsDBTypeDefined)
                    {
                        var p = new MySqlParameter();
                        p.ParameterName = param.Name;
                        p.Value = param.Value;
                        p.MySqlDbType = param.DbType;
                        cmd.Parameters.Add(p);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(param.Name, param.Value);
                    }


                }
            }

            return cmd;
        }

        /**********************************************
        QUERY PROCESSING
        **********************************************/
        private bool HasNoQueries()
        {
            return Queries == null || Queries.Count == 0;
        }

        private void QueriesInstanceIfNull()
        {
            if (Queries == null) Queries = new List<QueryGroup>();
        }

        internal void ExecuteQueries()
        {
            if (HasNoQueries()) return;

            foreach (var group in Queries)
            {
                if (group.MergeQueries)
                {
                    ExecuteNonQuery(group.MergedQuery);
                }
                else
                {
                    foreach (var query in group.Queries)
                    {
                        ExecuteNonQuery(query);
                    }
                }
            }
        }

        public void AddQuery(Query query)
        {
            QueriesInstanceIfNull();
            Queries.Add(new QueryGroup(query));
        }
        public void AddQuery(QueryGroup query)
        {
            QueriesInstanceIfNull();
            Queries.Add(query);
        }

        /**********************************************
        EXECUTERS
        **********************************************/
        public int ExecuteNonQuery(Query query, int returnValue = -1)
        {
            using (var cmd = QueryToCommand(query))
            {
                if (TransactionActive)
                {
                    cmd.Transaction = _transaction;

                    try
                    {
                        returnValue = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        TransactionRollback(ex);
                    }
                }
                else
                {
                    cmd.Connection.Open();
                    returnValue = cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }

            return returnValue;
        }

        public object ExecuteScalar(Query query, object returnValue = null)
        {
            using (var cmd = QueryToCommand(query))
            {
                if (TransactionActive)
                {
                    cmd.Transaction = _transaction;

                    try
                    {
                        returnValue = cmd.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        TransactionRollback(ex);
                    }
                }
                else
                {
                    cmd.Connection.Open();
                    returnValue = cmd.ExecuteScalar();
                    cmd.Connection.Close();
                }
            }

            return returnValue;
        }
    }
}
