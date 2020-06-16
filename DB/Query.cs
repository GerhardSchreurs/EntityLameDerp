using Extensions;
using Force.DeepCloner;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DB
{
    public class QueryGroup
    {
        public List<Query> Queries = new List<Query>();

        private string _paramGroupName = string.Empty;
        public bool MergeQueries = true;

        public QueryGroup(string paramGroupName)
        {
            _paramGroupName = paramGroupName;
        }

        public QueryGroup(Query query)
        {
            AddQuery(query);
        }

        public void AddQuery(Query query)
        {
            Queries.Add(query);
        }

        private string GetMergedParameterName(Parameter parameter)
        {
            return $"@{_paramGroupName}_{parameter.Name.StripFirstChar("@")}";
        }

        public string GetMergedQueryString()
        {
            var builder = new StringBuilder();
            Queries.ForEach(x => builder.AppendLine(x.SQL));
            var sql = builder.ToString();

            //Replace parameter names
            foreach (var query in Queries)
            {
                foreach (var param in query.Parameters)
                {
                    sql = Regex.Replace(sql, $"({param.Name})([,|\\)\\ ])", m => $"{GetMergedParameterName(param)}{m.Groups[2].Value}");
                }
            }

            return sql;
        }

        public List<Parameter> GetParameters()
        {
            var list = new List<Parameter>();
            foreach (var query in Queries)
            {
                list.AddRange(query.Parameters);
            }
            return list;
        }

        public List<Parameter> GetMergedParameters()
        {
            var list = GetParameters().DeepClone();

            foreach (var param in list)
            {
                //param.ParameterName = $"{_paramGroupName}_{param.ParameterName}";
                param.Name = GetMergedParameterName(param);
            }

            return list;
        }

        public Query MergedQuery
        {
            get
            {
                var query = new Query();
                query.SQL = GetMergedQueryString();
                query.Parameters = GetMergedParameters();
                return query;

            }
            private set { }
        }
    }

    public class Parameter
    {
        public Parameter()
        {

        }
        public Parameter(string name)
        {
            Name = name;
        }
        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
        public Parameter(MySqlDbType type, string name, object value)
        {
            Name = name;
            Value = value;
            DbType = type;
            IsDBTypeDefined = true;
        }

        public string Name;
        public object Value;
        public ParameterDirection Direction = ParameterDirection.Input;
        public bool IsDBTypeDefined;
        public MySqlDbType DbType;
    }


    public class Query
    {
        public string Name;
        public string SQL;
        public CommandType CommandType = CommandType.Text;
        //public List<MySqlParameter> Parameters = new List<MySqlParameter>();
        public List<Parameter> Parameters = new List<Parameter>();

        public Query()
        {

        }

        public Query(string sql)
        {
            SQL = sql;
        }

        public void AddParamIds(string name, List<object> list)
        {
            Parameters.Add(new Parameter(name, string.Join(",", list.ToArray())));
        }

        public void AddParam(string name, object value)
        {
            Parameters.Add(new Parameter(name, value));
        }

        public void AddParam(MySqlDbType type, string name, object value)
        {
            Parameters.Add(new Parameter(type, name, value));
        }


        public void AddParamMustRenameThis(string name, MySqlDbType type = MySqlDbType.VarChar)
        {
            var param = new Parameter();
            param.Name = name;
            param.DbType = type;
            param.Direction = ParameterDirection.Output;

            Parameters.Add(param);
        }
    }
}
