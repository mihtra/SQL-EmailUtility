using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SQLTechSuite
{
    static class ConfigurationHelper
    {
        public static string GetConfigurationValue(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
        public static string GetConnectionString(string key)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[key].ToString();
        }
    }
}
