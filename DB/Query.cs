using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DB
{
    public class QueryGroup
    {
        public List<Query> queries;
    }

    public class Query
    {
        public string SQL;
        public CommandType CommandType = CommandType.StoredProcedure;
        public List<MySqlParameter> Parameters;

        public Query(string sql)
        {
            SQL = sql;
        }

        private void InitParameters()
        {
            if (Parameters == null)
            {
                Parameters = new List<MySqlParameter>();
            }
        }

        public void AddParamIds(string name, List<object> list)
        {
            InitParameters();
            Parameters.Add(new MySqlParameter(name, string.Join(",", list.ToArray())));
        }
    }
}
