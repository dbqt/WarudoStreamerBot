using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using static QTExtensions.StreamerBot.Models.SBMessageModels;

namespace QTExtensions.StreamerBot.Nodes
{
    /// <summary>
    /// Node to receive any event from StreamerBot, this node doesn't get any arguments.
    /// </summary>
    // TODO: Enable receiving generic events
    /*[NodeType(
    Id = "3ee08a89-d2be-4eb7-b6e2-930b928650c3",
    Title = "On StreamerBot Event",
    Category = "StreamerBot")]*/
    public class OnStreamerBotEventNode : Node, IStreamerBotEventHandler
    {
        [DataInput]
        [AutoComplete(nameof(GetEvents), true)]
        public string EventName;

        [FlowOutput]
        public Continuation Exit;

        private StreamerBotClient client => Context.PluginManager.GetPlugin<StreamerBotPlugin>().StreamerBot;
        private string eventGuid;

        protected override void OnCreate()
        {
            Watch(nameof(EventName), delegate { SubscribeEvent(); });

            client?.RefreshEvents();

            base.OnCreate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async UniTask<AutoCompleteList> GetEvents()
        {
            if (client == null) { return null; }

            AutoCompleteList list = new AutoCompleteList();
            list.categories = new List<AutoCompleteCategory>();
            var platforms = new Dictionary<string, List<SBActionModel>>();

            await Task.Run(() => 
            {
                // Add all Twitch events
                var platform = new AutoCompleteCategory() { title = "Twitch" };
                platform.entries = Array.ConvertAll(client.TwitchEvents ?? new string[] { }, e => new AutoCompleteEntry() { label = e, value = e }).ToList();
                list.categories.Add(platform);
            });
            
            return list;
        }

        /// <summary>
        /// Subscribe to the selected event
        /// </summary>
        public void SubscribeEvent()
        {
            if (client == null) { return; }

            // Unsub from previous event if any
            if (!string.IsNullOrEmpty(eventGuid))
            {
                client.UnSubscribeEvent(eventGuid);
            }

            eventGuid = client.SubscribeEvent(Models.SBEnums.EventType.Twitch, EventName, this);

            client?.RefreshEvents();
        }

        /// <summary>
        /// Invoke the flow connected to this node
        /// </summary>
        public void Execute(string _)
        {
            InvokeFlow(nameof(Exit));
        }
    }
}
