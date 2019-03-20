using System.Configuration;
using System.Linq;
using byte5.LinkedNodesContentApp.Controller;
using byte5.LinkedNodesContentApp.Helper;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Configuration = System.Configuration.Configuration;

namespace byte5.LinkedNodesContentApp.Composer
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class b5LinkedNodesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<b5LinkedNodesComponent>();
        }
    }

    public class b5LinkedNodesComponent : IComponent
    {
        private LinkedNodesConfigHelper _configHelper = new LinkedNodesConfigHelper();

        public void Initialize()
        {
            ContentService.Trashing += ContentService_Trashing;
            MediaService.Trashing += MediaService_Trashing;
        }

        private void MediaService_Trashing(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            Configuration linkedNodesConfig = _configHelper.GetConfigurationFile();

            AppSettingsSection appSettings = (linkedNodesConfig.GetSection("appSettings") as AppSettingsSection);

            if (appSettings.Settings["events.preventDeletionOfLinkedContentNodes"].Value != null)
            {
                if (appSettings.Settings["events.preventDeletionOfLinkedContentNodes"].Value == "true")
                {
                    var relatedLinksApi = new LinkedNodesContentAppApiController();

                    foreach (var media in e.MoveInfoCollection)
                    {
                        var result = relatedLinksApi.GetLinkedNodes(media.Entity.GetUdi().ToString(), false);
                        if (result.Count() != 0)
                        {
                            e.Cancel = true;
                            e.Messages.Add(new EventMessage("Error, deleting is denied",
                                "You have " + result.Count() +
                                " linked nodes! See details in 'Linked Nodes' Content App, then remove the links to the current Node and try again.",
                                EventMessageType.Error));
                        }
                    }
                }
            }
        }

        private void ContentService_Trashing(IContentService sender, MoveEventArgs<IContent> e)
        {
            Configuration linkedNodesConfig = _configHelper.GetConfigurationFile();

            AppSettingsSection appSettings = (linkedNodesConfig.GetSection("appSettings") as AppSettingsSection);

            if (appSettings.Settings["events.preventDeletionOfLinkedMediaNodes"].Value != null)
            {
                if (appSettings.Settings["events.preventDeletionOfLinkedMediaNodes"].Value == "true")
                {
                    var relatedLinksApi = new LinkedNodesContentAppApiController();

                    foreach (var content in e.MoveInfoCollection)
                    {
                        var result = relatedLinksApi.GetLinkedNodes(content.Entity.GetUdi().ToString(), true);
                        if (result.Count() != 0)
                        {
                            e.Cancel = true;
                            e.Messages.Add(new EventMessage("Error, deleting is denied",
                                "You have " + result.Count() +
                                " linked nodes! See details in 'Linked Nodes' Content App, then remove the links to the current Node and try again.",
                                EventMessageType.Error));
                        }
                    }
                }
            }
        }

        public void Terminate()
        {
        }
    }
}