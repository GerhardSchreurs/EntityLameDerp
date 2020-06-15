using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace DBTest
{
    public class ConfigReader
    {
        public string ConnectionString;

        public ConfigReader(string path)
        {
            var basePath = Environment.CurrentDirectory;
            var projectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            basePath = basePath.Substring(0, basePath.LastIndexOf(projectName));
            path = basePath + projectName + "\\" + path;

            try
            {
                var map = new ExeConfigurationFileMap { ExeConfigFilename = path };
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

                ConnectionString = config.AppSettings.Settings["ConnectionString"].Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Please add an App.Config file to the project, with your connectionstring. I added my Private.config to .gitignore");
            }
        }
    }
}
