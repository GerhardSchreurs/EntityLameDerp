using System;
using System.Collections.Generic;
using System.Text;

namespace DBTest
{
    public static class Queries
    {
        public static string CheckIfDbExists = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'dbtest'";
    }
}
