using System;
using System.Configuration;

namespace DBTest.Tools
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
                throw new Exception("Please add an App.Config file to the project, rename it to Private.Config and add your connectionstring. Private.config is in .GITIGNORE\n\n" + ex.Message);
            }
        }
    }
}
