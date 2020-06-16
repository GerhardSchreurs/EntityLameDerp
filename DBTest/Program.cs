using DB;
using DB.Extensions;
using DBTest.Model;
using DBTest.Tools;
using System;
using System.Collections.Generic;
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

        static void Main(string[] args)
        {
            var config = new ConfigReader("Private.config");
            var db = DB.DB.Instance();
            db.ConnectionString = config.ConnectionString;

            var table = db.AddTable<TableStyles>();
            db.Fill();

            Print(table);


            var row = table.NewRow();
            row.Name = "test";
            row.Weight = -1;
            table.AddRow(row);
            db.Update();

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
