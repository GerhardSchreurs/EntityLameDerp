using DB;
using DB.Extensions;
using DBTest.Model;
using DBTest.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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

        private static StackTrace _stackTrace = new StackTrace();
        private static Configurator _config;
        private static DB.DB _db = DB.DB.Instance();
        private static Executor _exe = Executor.Instance();

        private static TableStyles _tblStyles;
        private static TableComposers _tblComposers;

        private static void DoStuff()
        {
            var connection = new MySqlConnection(_config.ConnectionString);
            
            var cmd1 = new MySqlCommand();
            var param1 = new MySqlParameter();
            param1.ParameterName = "@FIRST_INSERT_ID";
            param1.DbType = System.Data.DbType.Int32;
            param1.Direction = System.Data.ParameterDirection.Output;
            param1.Value = 0;

            cmd1.Connection = connection;
            cmd1.CommandText = "SELECT MAX(style_id) FROM styles;";
            cmd1.Parameters.Add(param1);

            cmd1.Connection.Open();
            var max = cmd1.ExecuteScalar();
            cmd1.Connection.Close();
        }

        static void Main(string[] args)
        {
            /*******************************************************
            NOTE: YOU MUST CREATE A DATABASE NAMED "dbtest" MANUALLY
            InitDatabase() SHOULD DO THE REST
            *******************************************************/

            _config = new Configurator("Private.config");
            _db.ConnectionString = _config.ConnectionString;

            InitDatabase(true);
            InitTables();
            FillTables(true);

            PrintTable(_tblStyles);
            PrintTable(_tblComposers);

            Console.WriteLine("Filling dataset");


            return;

            var table = _db.AddTable<TableStyles>();

            _db.TransactionBegin();
            _db.Fill();

            PrintTable(table);

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

            PrintTable(table);
        }

        private static void InitTables()
        {
            _tblStyles = _db.AddTable<TableStyles>();
            _tblComposers = _db.AddTable<TableComposers>();
        }

        private static void FillTables(bool run)
        {
            if (ShouldNotRun(run)) return;

            _db.TransactionBegin();
            FillTableStyles(run);
            FillTableComposers(run);

            _db.TransactionCommit();

        }
        private static void FillTableComposers(bool run)
        {
            if (ShouldNotRun(run)) return;

            RowComposer row;

            row = _tblComposers.NewRow();
            row.Name = "djnonsens";
            _tblComposers.AddRow(row);

            row = _tblComposers.NewRow();
            row.Name = "djduck";
            _tblComposers.AddRow(row);

            row = _tblComposers.NewRow();
            row.Name = "koos";
            _tblComposers.AddRow(row);

            row = _tblComposers.NewRow();
            row.Name = "degoede";
            _tblComposers.AddRow(row);

            row = _tblComposers.NewRow();
            row.Name = "fubar";
            _tblComposers.AddRow(row);

            _tblComposers.Update();
        }


        private static void FillTableStyles(bool run)
        {
            if (ShouldNotRun(run)) return;

            RowStyle row;
            var rowHard = _tblStyles.NewRow();
            rowHard.Name = "HARD";
            rowHard.Weight = -1;
            _tblStyles.AddRow(rowHard);

            var rowStyles = _tblStyles.NewRow();
            rowStyles.Name = "HOUSE";
            rowStyles.Weight = -1;
            _tblStyles.AddRow(rowStyles);

            _tblStyles.UpdateWithIdentity();

            var rowHardcore = _tblStyles.NewRow();
            rowHardcore.Name = "Hardcore";
            rowHardcore.Parent_style_id = rowHard.Style_id;
            rowHardcore.Weight = 3;
            _tblStyles.AddRow(rowHardcore);

            var rowSpeedCore = _tblStyles.NewRow();
            rowSpeedCore.Name = "Speedcore";
            rowSpeedCore.Parent_style_id = rowHard.Style_id;
            rowSpeedCore.Weight = 3;
            _tblStyles.AddRow(rowSpeedCore);

            var rowFrenchCore = _tblStyles.NewRow();
            rowFrenchCore.Name = "Frenchcore";
            rowFrenchCore.Parent_style_id = rowHard.Style_id;
            rowFrenchCore.Weight = 3;
            _tblStyles.AddRow(rowFrenchCore);

            row = _tblStyles.NewRow();
            row.Name = "Hard core";
            row.Alt_style_id = rowHardcore.Style_id;
            row.Weight = 3;
            _tblStyles.AddRow(row);

            row = _tblStyles.NewRow();
            row.Name = "Speed core";
            row.Alt_style_id = rowSpeedCore.Style_id;
            row.Weight = 3;
            _tblStyles.AddRow(row);

            row = _tblStyles.NewRow();
            row.Name = "French core";
            row.Alt_style_id = rowFrenchCore.Style_id;
            row.Weight = 3;
            _tblStyles.AddRow(row);

            _tblStyles.UpdateWithIdentity();
        }

        private static bool ShouldNotRun(bool param, [CallerMemberName] string callerName = "")
        {
            Console.WriteLine($"{callerName} : {param}");
            return !param;
        }

        private static void PrintFunc(object args, [CallerMemberName] string callerName = "")
        {
            if (args == null)
            {
                Console.WriteLine($"{callerName}");
            }
            else
            {
                Console.WriteLine($"{callerName} : {args}");
            }
        }

        private static void InitDatabase(bool run)
        {
            if (ShouldNotRun(run)) return;

            var recreate = _exe.ExecuteQueryHasRows(Queries.CheckIfDbExists);

            if (recreate)
            {
                recreate = ConsoleUtils.Confirm("WARNING: DBTest already exists. Recreate?");
            }

            if (recreate)
            {
                var sql = File.ReadAllText($"{_config.BasePath}\\setup.sql");
                Console.WriteLine($"InitDatabase() => {_exe.ExecuteNonQuery(sql)}");
                _exe.ClearQueries();
            }
        }

        static void PrintTable<T>(Table<T> table) where T : DB.Row, new()
        {
            PrintFunc(typeof(T).FullName);

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
