using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using byte5.LinkedNodesContentApp.Helper;
using byte5.LinkedNodesContentApp.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Editors;

namespace byte5.LinkedNodesContentApp.Controller
{
    [Umbraco.Web.Mvc.PluginController("b5LinkedNodes")]
    public class LinkedNodesContentAppApiController : UmbracoAuthorizedJsonController
    {
        private LinkedNodesConfigHelper _configHelper = new LinkedNodesConfigHelper();
        private bool _listWithProperties;

        public LinkedNodesContentAppApiController()
        {
            _listWithProperties = CheckConfig_UsePropertiesList();
        }

        /// <summary>
        /// Get all Content Nodes from Cache
        /// </summary>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <param name="isContent">true, if User is in Content Section; false, if User is in Media Section</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        [HttpGet]
        public List<LinkedNodesDataModel> GetLinkedNodes(string currentUdi, bool isContent)
        {
            List<LinkedNodesDataModel> relatedLinks = new List<LinkedNodesDataModel>();

            // Get all root nodes
            foreach (var root in Umbraco.ContentAtRoot())
            {
                relatedLinks = GetRelatedProperty(relatedLinks, root, currentUdi, isContent);
                GetChildren(relatedLinks, root, currentUdi, isContent);
            }
            return relatedLinks;
        }

        /// <summary>
        /// Get all Childrens from a parent node
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="parent">Parent Node</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <param name="isContent">true, if User is in Content Section; false, if User is in Media Section</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> GetChildren(List<LinkedNodesDataModel> list, IPublishedContent parent, string currentUdi, bool isContent)
        {
            foreach (var child in parent.Children)
            {
                list = GetRelatedProperty(list, child, currentUdi, isContent);
                if (child.Children.Any())
                {
                    GetChildren(list, child, currentUdi, isContent);
                }
            }
            return list;
        }

        /// <summary>
        /// Check if Property of a node is a supported data type 
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="node">Content Node to check</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <param name="isContent">true, if User is in Content Section; false, if User is in Media Section</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> GetRelatedProperty(List<LinkedNodesDataModel> list,
            IPublishedContent node, string currentUdi, bool isContent)
        {
            string nodePath = GetRelatedPath(node);

            foreach (var prop in node.Properties.Where(x => x.PropertyType.EditorAlias == "Umbraco.ContentPicker"
                                                         || x.PropertyType.EditorAlias == "Umbraco.MultiNodeTreePicker"
                                                         || x.PropertyType.EditorAlias == "Umbraco.MultiUrlPicker"
                                                         || x.PropertyType.EditorAlias == "Umbraco.NestedContent"
                                                         || x.PropertyType.EditorAlias == "Umbraco.Grid"
                                                         || x.PropertyType.EditorAlias == "Umbraco.TinyMCE"
                                                         || x.PropertyType.EditorAlias == "Umbraco.MediaPicker"))
            {
                try
                {
                    if (node.AncestorOrSelf(1).ContentType.Alias.ToString() != "b5HelpContentAppRepository")
                    {
                        IPublishedContent relatedNode = prop.GetValue() as IPublishedContent;
                        IEnumerable<IPublishedContent> relatedNodeList = new List<IPublishedContent>();

                        // relatedNode is null dependent from the datatype settings (i.e. Media Picker => Single oder multiple items)
                        if (relatedNode == null)
                        {
                            relatedNodeList = prop.GetValue() as IEnumerable<IPublishedContent>;
                        }

                        switch (prop.PropertyType.EditorAlias)
                        {
                            case "Umbraco.ContentPicker":
                            case "Umbraco.MediaPicker":
                                list = AddRelatedNodePicker(list, node, relatedNode, prop, nodePath, currentUdi,
                                    relatedNodeList);
                                break;
                            case "Umbraco.MultiNodeTreePicker":
                                list = AddRelatedNodeMultiNodeTreePicker(list, node, prop, nodePath, currentUdi);
                                break;
                            case "Umbraco.MultiUrlPicker":
                                list = AddRelatedNodeMultiUrlPicker(list, node, prop, nodePath, currentUdi);
                                break;
                            case "Umbraco.NestedContent":
                            case "Umbraco.Grid":
                            case "Umbraco.TinyMCE":
                                list = AddRelatedNodeFromJsonSource(list, node, prop, nodePath, currentUdi, isContent);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error<LinkedNodesDataModel>(ex);
                }
            }
            return list;
        }

        /// <summary>
        /// Check if is a linked Node for Nested Content, Grid and TinyMCE
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="node">Content Node to check</param>
        /// <param name="prop">Property to check</param>
        /// <param name="nodePath">Complete node Path</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <param name="isContent">true, if User is in Content Section; false, if User is in Media Section</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> AddRelatedNodeFromJsonSource(List<LinkedNodesDataModel> list, IPublishedContent node, IPublishedProperty prop, string nodePath, string currentUdi, bool isContent)
        {
            var ncValue = prop.GetSourceValue();
            if (ncValue != null)
            {
                Regex r;
                if (isContent)
                {
                    r = new Regex("\"umb://document/(.*?)\"", RegexOptions.IgnoreCase);
                }
                else
                {
                    r = new Regex("\"umb://media/(.*?)\"", RegexOptions.IgnoreCase);
                }
                
                MatchCollection mc = r.Matches(ncValue.ToString());

                foreach (var result in mc)
                {
                    var cleanedResultList = result.ToString().Replace("\"", string.Empty).Replace("\\", string.Empty).Split(',');
                    if (cleanedResultList.Count() == 1)
                    {
                        list = AddRelatedNode(list, node, null, prop, nodePath, currentUdi, cleanedResultList.FirstOrDefault());
                    }
                    else
                    {
                        foreach (var resultItem in cleanedResultList)
                        {
                            list = AddRelatedNode(list, node, null, prop, nodePath, currentUdi, resultItem);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get linked node value from Multi Url Picker
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="node">Content Node to check</param>
        /// <param name="prop">Property to check</param>
        /// <param name="nodePath">Complete node Path</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> AddRelatedNodeMultiUrlPicker(List<LinkedNodesDataModel> list, IPublishedContent node, IPublishedProperty prop, string nodePath, string currentUdi)
        {
            if (prop.GetValue() != null)
            {
                foreach (Umbraco.Web.Models.Link link in prop.GetValue() as IEnumerable<Umbraco.Web.Models.Link>)
                {
                    list = AddRelatedNode(list, node, null, prop, nodePath, currentUdi, link.Udi.ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// Get linked node value from Multi Node Tree Picker
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="node">Content Node to check</param>
        /// <param name="prop">Property to check</param>
        /// <param name="nodePath">Complete node Path</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> AddRelatedNodeMultiNodeTreePicker(List<LinkedNodesDataModel> list, IPublishedContent node, IPublishedProperty prop, string nodePath, string currentUdi)
        {
            if (prop.GetValue() != null)
            {
                foreach (IPublishedContent item in prop.GetValue() as List<IPublishedContent>)
                {
                    list = AddRelatedNode(list, node, item, prop, nodePath, currentUdi);
                }
            }
            return list;
        }

        /// <summary>
        /// Check if node list is empty and add new linked node
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="node">Content Node to check</param>
        /// <param name="relatedNode">linked Node (if picker contains single item)</param>
        /// <param name="prop">Property to check</param>
        /// <param name="nodePath">Complete node Path</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <param name="relatedNodeList">List of nodes (if picker contains multiple items)</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> AddRelatedNodePicker(List<LinkedNodesDataModel> list, IPublishedContent node, IPublishedContent relatedNode,
            IPublishedProperty prop, string nodePath, string currentUdi, IEnumerable<IPublishedContent> relatedNodeList)
        {
            if (!relatedNodeList.Any())
            {
                list = AddRelatedNode(list, node, relatedNode, prop, nodePath, currentUdi);
            }
            else
            {
                foreach (var relNode in relatedNodeList)
                {
                    list = AddRelatedNode(list, node, relNode, prop, nodePath, currentUdi);
                }
            }
            return list;
        }

        /// <summary>
        /// Get node path from linked node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Comma separated string with node id's</returns>
        private string GetRelatedPath(IPublishedContent node)
        {
            string nodePath = string.Empty;
            int pos = 0;
            string[] pathList = node.Path.Split(',');

            foreach (var id in pathList)
            {
                pos++;
                if (id != "-1")
                {
                    nodePath = nodePath + Umbraco.Content(id).Name + (pos != pathList.Count() ? " > " : null);
                }
            }

            return nodePath;
        }

        /// <summary>
        /// Add linked Node to result list
        /// </summary>
        /// <param name="list">List with already linked Nodes</param>
        /// <param name="node">Content Node to check</param>
        /// <param name="relatedNode">linked Node (if picker contains single item)</param>
        /// <param name="prop">Property to check</param>
        /// <param name="nodePath">Complete node Path</param>
        /// <param name="currentUdi">UDI from current Content Node</param>
        /// <param name="relatedUdi">UDI of linked Node</param>
        /// <returns>List of LinkedNodesDataModel</returns>
        private List<LinkedNodesDataModel> AddRelatedNode(List<LinkedNodesDataModel> list, IPublishedContent node, IPublishedContent relatedNode,
            IPublishedProperty prop, string nodePath, string currentUdi, string relatedUdi = "")
        {
            string nodeUdi = string.Empty;
            if (relatedNode != null)
            {
                if (relatedNode.ItemType == PublishedItemType.Media)
                {
                    nodeUdi = "umb://media/" + relatedNode.Key.ToString().Replace("-", string.Empty);
                }
                else
                {
                    nodeUdi = "umb://document/" + relatedNode.Key.ToString().Replace("-", string.Empty);
                }
                
            }

            if ((relatedNode != null && nodeUdi == currentUdi) || relatedUdi == currentUdi)
            {
                LinkedNodesDataModel result;
                
                if (_listWithProperties)
                {
                    // if Property Alias is used in Content App overview table
                    // Prevent duplicate property aliases
                    result = list.FirstOrDefault(x => x.Id == node.Id && x.PropertyAlias == prop.Alias);
                }
                else
                {
                    // Prevent duplicate nodes
                    result = list.FirstOrDefault(x => x.Id == node.Id);
                }

                if (result == null)
                {
                    // Add new linked Node if not exist in list
                    list.Add(new LinkedNodesDataModel()
                    {
                        Id = node.Id,
                        Name = node.Name,
                        PropertyAlias = prop.Alias,
                        Path = nodePath,
                        RelLinkCountPerProp = 1
                    });
                }
                else
                {
                    // Add not linked Node, but add 1 to existed linked Node counter (using by badge in content app overview table)
                    result.RelLinkCountPerProp = result.RelLinkCountPerProp + 1;
                }
            }
            return list;
        }

        /// <summary>
        /// Check, if property alias should be shown in backend table
        /// </summary>
        /// <returns>true=Show (default) or false=Hide</returns>
        private bool CheckConfig_UsePropertiesList()
        {
            try
            {
                Configuration linkedNodesConfig = _configHelper.GetConfigurationFile();

                AppSettingsSection appSettings = (linkedNodesConfig.GetSection("appSettings") as AppSettingsSection);

                return appSettings.Settings["overview.showPropertyAlias"].Value == "true" ? true : false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
    }
}

