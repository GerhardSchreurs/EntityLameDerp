using DBTest.Model;
using System;

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

            foreach (var row in table.Rows)
            {
                Console.WriteLine(row.Name);
            }
        }
    }
}
