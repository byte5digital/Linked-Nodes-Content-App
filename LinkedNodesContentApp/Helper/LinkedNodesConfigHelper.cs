using System;
using System.Configuration;

namespace byte5.LinkedNodesContentApp.Helper
{
    public class LinkedNodesConfigHelper
    {
        public Configuration GetConfigurationFile()
        {
            try
            {
                ExeConfigurationFileMap linkedNodesConfigFileMap = new ExeConfigurationFileMap();
                var filePath = System.Web.Hosting.HostingEnvironment.MapPath("/") +
                               "\\App_Plugins\\b5LinkedNodesContentApp\\linkedNodes.config";
                linkedNodesConfigFileMap.ExeConfigFilename = filePath;

                return ConfigurationManager.OpenMappedExeConfiguration(linkedNodesConfigFileMap,
                    ConfigurationUserLevel.None);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}