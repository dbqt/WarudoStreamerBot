using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Server;

namespace DbqtExtensions.StreamerBot.Nodes
{
    [NodeType(
    Id = "751950bb-c504-4ee6-8336-849d799f48b6",
    Title = "Do StreamerBot Action",
    Category = "StreamerBot")]
    public class StreamerBotDoActionNode : Node
    {
        [FlowInput]
        public Continuation Enter() 
        {
            client.DoAction(actionName, actionId);
            return null;
        }

        [FlowOutput]
        public Continuation Exit;

        [DataInput]
        [AutoComplete(nameof(GetActions), true)]
        public string Action;

        private string actionId;
        private string actionName;

        private StreamerBotClient client => Context.PluginManager.GetPlugin<StreamerBotPlugin>().StreamerBot;

        protected override void OnCreate()
        {
            Watch(nameof(Action), delegate { UpdateAction(); });

            base.OnCreate();
        }

        public async UniTask<AutoCompleteList> GetActions()
        {
            AutoCompleteList list = new AutoCompleteList();
            list.categories = new List<AutoCompleteCategory>();
            var organizedActions = new Dictionary<string, List<(string, string)>>();

            // Get every action possible and split them by category
            foreach (var action in client.Actions.actions)
            {
                // HACK: There can be a null category, assign a string of one space instead
                var groupString = string.IsNullOrEmpty(action.group) ? " " : action.group;

                // Create the category if it doesn't exist yet
                if (!organizedActions.ContainsKey(groupString))
                {
                    organizedActions.Add(groupString, new List<(string, string)>());
                }

                // Add the action to the category
                organizedActions[groupString].Add((action.name, action.id));
            }

            foreach (var group in organizedActions)
            {
                // Give the AutoCompleteCategory the correct label and initialize the list of entries
                var category = new AutoCompleteCategory() { title = group.Key, entries = new List<AutoCompleteEntry>() };

                // Add every action under that category
                foreach (var action in group.Value)
                {
                    category.entries.Add(new AutoCompleteEntry() { label = $"{action.Item1} - {action.Item2}", value = action.Item2 });
                }
                list.categories.Add(category);
            }

            return list;
        }

        public void UpdateAction()
        {
            actionId = Action;
            actionName = client.Actions.actions.Where(a => a.id == actionId).FirstOrDefault()?.name;
        }
    }
}
