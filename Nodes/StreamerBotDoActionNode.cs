using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Localization;
using static QTExtensions.StreamerBot.Models.SBMessageModels;

namespace QTExtensions.StreamerBot.Nodes
{
    /// <summary>
    /// Node to execute any action on StreamerBot with optional arguments.
    /// </summary>
    [NodeType(
    Id = "751950bb-c504-4ee6-8336-849d799f48b6",
    Title = "STREAMERBOT_DOACTION",
    Category = "STREAMERBOT_NODE_CATEGORY")]
    public class StreamerBotDoActionNode : Node
    {
        [FlowInput]
        public Continuation Enter() 
        {
            client.DoAction(actionName, actionId, Args);
            return null;
        }

        [FlowOutput]
        public Continuation Exit;

        [DataInput]
        [AutoComplete(nameof(GetActions), true)]
        public string Action;

        [DataInput]
        public Dictionary<string, string> Args;

        private string actionId;
        private string actionName;

        private StreamerBotClient client => Context.PluginManager.GetPlugin<StreamerBotPlugin>().StreamerBot;

        protected override void OnCreate()
        {
            Watch(nameof(Action), delegate { UpdateAction(); });

            client?.RefreshActions();

            base.OnCreate();
        }

        /// <summary>
        /// Retrieves all the possible actions from StreamerBot and generate a dropdown list.
        /// </summary>
        public async UniTask<AutoCompleteList> GetActions()
        {
            if (client == null) { return null; }

            AutoCompleteList list = new AutoCompleteList();
            list.categories = new List<AutoCompleteCategory>();
            var organizedActions = new Dictionary<string, List<SBActionModel>>();

            await Task.Run(() =>
            {
                // Get every action possible and split them by category
                foreach (var actionId in client.Actions)
                {
                    // There can be a null category, assign an arbitrary string
                    var action = client.ActionGuidToModel[actionId];
                    var groupString = string.IsNullOrEmpty(action.group) ? "STREAMERBOT_NONAMEACTIONCATEGORY".Localized() : action.group;

                    // Create the category if it doesn't exist yet
                    if (!organizedActions.ContainsKey(groupString))
                    {
                        organizedActions.Add(groupString, new List<SBActionModel>());
                    }

                    // Add the action to the category
                    organizedActions[groupString].Add(action);
                }

                foreach (var group in organizedActions)
                {
                    // Give the AutoCompleteCategory the correct label and initialize the list of entries
                    var category = new AutoCompleteCategory() { title = group.Key, entries = new List<AutoCompleteEntry>() };

                    // Add every action under that category
                    foreach (var action in group.Value)
                    {
                        category.entries.Add(new AutoCompleteEntry() { label = $"{action.name} [{action.id}]", value = action.id });
                    }
                    list.categories.Add(category);
                }
            });

            return list;
        }

        /// <summary>
        /// Update the local data with the selected action.
        /// </summary>
        public void UpdateAction()
        {
            if (client == null) { return; }

            actionId = Action;
            actionName = client.ActionGuidToModel[actionId].name;

            client?.RefreshActions();
        }
    }
}
