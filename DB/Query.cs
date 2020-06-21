using Extensions;
using Force.DeepCloner;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace DB
{
    public class QueryGroup
    {
        private string _paramGroupName = string.Empty;
        public List<Query> Queries = new List<Query>();
        public bool MergeQueries = true;
        public QueryExecuteMethod ExecuteMethod = QueryExecuteMethod.NonQuery;

        public QueryGroup(string paramGroupName = "")
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
            if (_paramGroupName == string.Empty)
            {
                return parameter.Name;

            }

            return $"@{_paramGroupName}_{parameter.Name.StripFirstChar("@")}";
        }

        public string GetMergedQueryString()
        {
            var builder = new StringBuilder();
            Queries.ForEach(x => builder.AppendLine(x.SQL));
            var sql = builder.ToString();

            if (_paramGroupName == string.Empty) return sql;

            //Replace parameter names
            foreach (var query in Queries)
            {
                foreach (var param in query.Parameters)
                {
                    //sql = Regex.Replace(sql, $"({param.Name})([,|\\)\\ ])", m => $"{GetMergedParameterName(param)}{m.Groups[2].Value}");
                    //({param.Name})([ |,|;|\\)])

                    sql = Regex.Replace(sql, $"({param.Name})([ |,|;|\\)])", m => $"{GetMergedParameterName(param)}{m.Groups[2].Value}");
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
        public Parameter(string name, object value, DBType type)
        {
            Name = name;
            Value = value;
            DBType = type;
        }

        public Parameter(string name, Func<object> func, DBType type)
        {
            Name = name;
            _func = func;
            DBType = type;
        }

        private Func<object> _func;
        public string Name;
        public object Value;
        public ParameterDirection Direction = ParameterDirection.Input;
        public DBType DBType = DBType.None;

        internal void Update()
        {
            if (_func != null)
            {
                Value = _func();
            }
        }
    }


    public class Query
    {
        public string Name;
        public string SQL;
        private object _value;
        private Action _func;

        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                if (_func != null) _func.Invoke();
            }
        }

        public CommandType CommandType = CommandType.Text;
        public QueryExecuteMethod ExecuteMethod = QueryExecuteMethod.NonQuery;

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

        public void AddParam(string name, object value, DBType type)
        {
            Parameters.Add(new Parameter(name, value, type));
        }

        public void AddParamMustRenameThis(string name, DBType type = DBType.VarChar)
        {
            var param = new Parameter();
            param.Name = name;
            param.DBType = type;
            param.Direction = ParameterDirection.Input;

            Parameters.Add(param);
        }

        internal void AddRetrieveParam(string name, Func<object> func, DBType type)
        {
            Parameters.Add(new Parameter(name, func, type));
        }

        internal void AfterUpdate(Action func)
        {
            _func = func;
        }
    }

    public enum QueryExecuteMethod
    {
        NonQuery,
        Scalar,
        Reader
    }
}
