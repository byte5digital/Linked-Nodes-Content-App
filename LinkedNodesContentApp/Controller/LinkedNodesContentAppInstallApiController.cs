using System;
using System.Configuration;
using System.Net;
using System.Web.Http;
using byte5.LinkedNodesContentApp.Helper;
using byte5.LinkedNodesContentApp.Models;
using Umbraco.Web.Editors;
using Umbraco.Core.Logging;

namespace byte5.LinkedNodesContentApp.Controller
{
    [Umbraco.Web.Mvc.PluginController("b5LinkedNodes")]
    public class LinkedNodesContentAppInstallApiController : UmbracoAuthorizedJsonController
    {
        private LinkedNodesConfigHelper _configHelper = new LinkedNodesConfigHelper();

        [HttpGet]
        public LinkedNodesConfigModel GetConfiguration()
        {
            try
            {
                Configuration linkedNodesConfig = _configHelper.GetConfigurationFile();

                AppSettingsSection appSettings = (linkedNodesConfig.GetSection("appSettings") as AppSettingsSection);

                LinkedNodesConfigModel config = new LinkedNodesConfigModel()
                {
                    OverviewShowId = appSettings.Settings["overview.showId"].Value == "true",
                    OverviewShowPath = appSettings.Settings["overview.showPath"].Value == "true",
                    OverviewShowPropertyAlias = appSettings.Settings["overview.showPropertyAlias"].Value == "true",
                    EventsPreventDeletionOfLinkedContentNodes = appSettings.Settings["events.preventDeletionOfLinkedContentNodes"].Value == "true",
                    EventsPreventDeletionOfLinkedMediaNodes = appSettings.Settings["events.preventDeletionOfLinkedMediaNodes"].Value == "true"
                };
                return config;
            } catch (Exception ex)
            {
                Logger.Error<LinkedNodesContentAppInstallApiController>(ex);
                return null;
            }
        }

        [HttpPost]
        public HttpStatusCode SetConfiguration(LinkedNodesConfigModel config)
        {
            try
            {
                Configuration linkedNodesConfig = _configHelper.GetConfigurationFile();

                AppSettingsSection appSettings = (linkedNodesConfig.GetSection("appSettings") as AppSettingsSection);

                appSettings.Settings["overview.showId"].Value = config.OverviewShowId == true ? "true" : "false";
                appSettings.Settings["overview.showPath"].Value = config.OverviewShowPath == true ? "true" : "false";
                appSettings.Settings["overview.showPropertyAlias"].Value =
                    config.OverviewShowPropertyAlias == true ? "true" : "false";
                appSettings.Settings["events.preventDeletionOfLinkedContentNodes"].Value =
                    config.EventsPreventDeletionOfLinkedContentNodes == true ? "true" : "false";
                appSettings.Settings["events.preventDeletionOfLinkedMediaNodes"].Value =
                    config.EventsPreventDeletionOfLinkedMediaNodes == true ? "true" : "false";

                linkedNodesConfig.Save(ConfigurationSaveMode.Modified);

                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Logger.Error<LinkedNodesContentAppInstallApiController>(ex);
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}