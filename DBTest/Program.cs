using DB;
using DB.Extensions;
using DBTest.Model;
using DBTest.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBTest
{
    class Program
    {
        #region README
        /*
        Please note. I have a config file in DBTest\Private.config
        This file is removed from repository due to privacy reasons.
        Add your own App.Config, rename to Private.Config and add:
        <configuration>
            <appSettings>
               <add key="ConnectionString" value="Server=***; database=***; UID=***; password=***" />
            </appSettings>
        </configuration>
        */
        #endregion

        private static bool _setupDatabase = true;
        private static Configurator _config;
        private static DB.DB _db = DB.DB.Instance();
        private static Executor _exe = Executor.Instance();

        private static void Init()
        {
            /*******************************************************
            NOTE: YOU MUST CREATE A DATABASE NAMED "dbtest" MANUALLY
            SetupDatabase() SHOULD DO THE REST
            *******************************************************/

            _config = new Configurator("Private.config");
            _db.ConnectionString = _config.ConnectionString;
        }


        private static void SetupDatabase()
        {
            Console.WriteLine("SetupDatabase()");
            var sqlCreate = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'dbtest'";
            var recreate = _exe.ExecuteQueryHasRows(sqlCreate);

            if (recreate)
            {
                recreate = ConsoleUtils.Confirm("WARNING: DBTest already exists. Recreate?");
            }

            if (recreate)
            {
                var sql = File.ReadAllText($"{_config.BasePath}\\setup.sql");
                Console.WriteLine($"SetupDatabase() => {_exe.ExecuteNonQuery(sql)}");
                _exe.ClearQueries();
            }
        }


        static void Main(string[] args)
        {
            Init();

            if (_setupDatabase)
            {
                SetupDatabase();
            }

            var table = _db.AddTable<TableStyles>();

            _db.TransactionBegin();
            _db.Fill();

            Print(table);

            //table.DeleteRow(table.Rows.Last());

            var row = table.NewRow();
            row.Name = "test2";
            row.Weight = -1;
            table.AddRow(row);
            table.DeleteRow(table.Rows.Last());
            table.Rows.Last().Name = "xxx";

            //var row = table.NewRow();
            //row.Name = "test2";
            //row.Weight = -1;
            //table.AddRow(row);
            //db.Update();

            _db.Update();
            _db.Fill();

            _db.TransactionCommit();

            Print(table);
        }

        static void Print(TableStyles table)
        {
            var colHeaders = new List<string>();
            table.Cols.ForEach(x => colHeaders.Add(x.Name));

            var printer = new TablePrinter();
            printer.SetHeaders(colHeaders.ToArray());

            foreach (var row in table.Rows)
            {
                var cols = new List<string>();
                table.Cols.ForEach(x => cols.Add(row.GetValueString(x)));
                printer.AddRow(cols.ToArray());
            }

            Console.WriteLine(printer.ToString());
        }
    }
}
