using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;

namespace DbqtExtensions.StreamerBot.Nodes
{
    [NodeType(
    Id = "3ee08a89-d2be-4eb7-b6e2-930b928650c3",
    Title = "On StreamerBot Event",
    Category = "StreamerBot")]
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

            base.OnCreate();
        }

        public async UniTask<AutoCompleteList> GetEvents()
        {
            var events = Array.ConvertAll(client.TwitchEvents, e => new AutoCompleteEntry() { label = e, value = e });
            return AutoCompleteList.Single(events);
        }

        public void SubscribeEvent()
        {
            // Unsub from previous event if any
            if (!string.IsNullOrEmpty(eventGuid))
            {
                client.UnSubscribeEvent(eventGuid);
            }

            eventGuid = client.SubscribeEvent(Models.SBEnums.EventType.Twitch, EventName, this);
        }

        public void Execute(string obj)
        {
            InvokeFlow(nameof(Exit));
        }
    }
}
