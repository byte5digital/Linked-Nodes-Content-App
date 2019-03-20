using System;
using System.Configuration;
using System.Web;

namespace byte5.LinkedNodesContentApp.Helper
{
    public class LinkedNodesConfigHelper
    {
        public Configuration GetConfigurationFile()
        {
            try
            {
                ExeConfigurationFileMap linkedNodesConfigFileMap = new ExeConfigurationFileMap();
                var filePath = HttpContext.Current.Server.MapPath("/") +
                               "\\App_Plugins\\b5LinkedNodesContentApp\\linkedNodes.config";
                linkedNodesConfigFileMap.ExeConfigFilename = filePath;

                return ConfigurationManager.OpenMappedExeConfiguration(linkedNodesConfigFileMap,
                    ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}