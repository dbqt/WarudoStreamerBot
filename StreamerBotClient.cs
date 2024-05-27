using DbqtExtensions.StreamerBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DbqtExtensions.StreamerBot.Models.SBMessageModels;
using static DbqtExtensions.StreamerBot.Models.SBRequestModels;

namespace DbqtExtensions.StreamerBot
{
    public class StreamerBotClient
    {
        public enum Status { Connected, Disconnected }

        public Status ConnectionStatus { get; private set; }
        public event Action<string> OnOpen;
        public event Action<string> OnMessage;
        public event Action<string> OnClose;
        public event Action<string> OnError;

        private WebSocketClient wsClient = null;

        public string[] TwitchEvents => twitchEvents;
        private string[] twitchEvents;

        public SBActionsModel Actions => actions;
        private SBActionsModel actions;

        private Dictionary<string, EventData> guidToEvents = new Dictionary<string, EventData>();

        private class EventData
        {
            public string Guid;
            public IStreamerBotEventHandler EventHandler;
            public string EventName;
            public SBEnums.EventType EventType;

            public EventData(string guid, IStreamerBotEventHandler eventHandler, string eventName, SBEnums.EventType eventType)
            {
                Guid = guid;
                EventHandler = eventHandler;
                EventName = eventName;
                EventType = eventType;
            }
        }

        public StreamerBotClient()
        {
            ConnectionStatus = Status.Disconnected;
        }

        ~StreamerBotClient()
        {
            if (wsClient != null)
            {
                wsClient.Disconnect();
                wsClient.CleanUp();
            }
        }

        private void WsClient_OnError(string obj)
        {
            OnError?.Invoke(obj);
        }

        private void WsClient_OnClose(string obj)
        {
            ConnectionStatus = Status.Disconnected;

            OnClose?.Invoke(obj);
        }

        private void WsClient_OnMessage(string obj)
        {
            OnMessage?.Invoke(obj);

            // Specifically handle the geteventsid event
            var genericMessage = JsonConvert.DeserializeObject<SBMessageModel>(obj);
            if (genericMessage != null && genericMessage.id != null && genericMessage.id.Equals("geteventsid"))
            {
                var getEventsMessage = JsonConvert.DeserializeObject<SBEventsModel>(obj);
                if (getEventsMessage != null && getEventsMessage.events != null)
                {
                    twitchEvents = getEventsMessage.events.twitch;
                }
            }
            // Specifically handle the getactionsid event
            else if (genericMessage != null && genericMessage.id != null && genericMessage.id.Equals("getactionsid"))
            {
                var getActionsMessage = JsonConvert.DeserializeObject<SBActionsModel>(obj);
                if (getActionsMessage != null)
                {
                    actions = getActionsMessage;
                }
            }
            // Handle every other events
            else
            {
                var genericEvent = JsonConvert.DeserializeObject<SBGenericModel>(obj);
                if (genericEvent != null && genericEvent.SBevent != null)
                {
                    var pairs = guidToEvents.Where(o => o.Value.EventName == genericEvent.SBevent.type);
                    foreach (var item in pairs)
                    {
                        item.Value.EventHandler.Execute(obj);
                    }
                }
            } 
        }

        private void WsClient_OnOpen(string obj)
        {
            ConnectionStatus = Status.Connected;

            // Retrieve the events once on connecting
            GetEvents();
            GetActions();

            OnOpen?.Invoke(obj);
        }

        /// <summary>
        /// Retrieve all the events from StreamerBot.
        /// </summary>
        public void GetEvents()
        {
            wsClient.SendMessage(JsonConvert.SerializeObject(new GetEventsModel()));
        }

        public void GetActions()
        {
            wsClient.SendMessage(JsonConvert.SerializeObject(new GetActionsModel()));
        }

        /// <summary>
        /// Request StreamerBot to send specific events.
        /// </summary>
        public string SubscribeEvent(SBEnums.EventType eventType, string eventName, IStreamerBotEventHandler handler)
        {
            if (wsClient == null) { return null; }

            // Keep track of the subscription
            var guid = Guid.NewGuid().ToString();
            guidToEvents.Add(guid, new EventData(guid, handler, eventName, eventType));

            // Actually request StreamerBot
            var newEvent = new SBRequestModels.SubscribeModel(eventType, new string[] { eventName }, guid);
            wsClient.SendMessage(JsonConvert.SerializeObject(newEvent));
            return guid;
        }

        /// <summary>
        /// Request StreamerBot to stop sending specific events.
        /// </summary>
        public void UnSubscribeEvent(string guid)
        {
            if (wsClient == null) { return; }
            if (!guidToEvents.ContainsKey(guid)) { return; }

            var data = guidToEvents[guid];

            // Guid here is for the new unsub request, so it needs to be different
            var newEvent = new SBRequestModels.UnsubscribeModel(data.EventType, new string[] { data.EventName }, Guid.NewGuid().ToString());
            wsClient.SendMessage(JsonConvert.SerializeObject(newEvent));

            // Clean up the sub from the dictionary
            guidToEvents.Remove(guid);
        }

        /// <summary>
        /// Initializes the websocket client with the address and port and subscribes to all its events.
        /// </summary>
        public void ConnectStreamerBot(string address, string port)
        {
            wsClient = new WebSocketClient();
            wsClient.Initialize(address, port);
            wsClient.OnOpen += WsClient_OnOpen;
            wsClient.OnMessage += WsClient_OnMessage;
            wsClient.OnClose += WsClient_OnClose;
            wsClient.OnError += WsClient_OnError;
            wsClient.Connect();
        }

        /// <summary>
        /// Disconnects and cleans up the event subscriptions.
        /// </summary>
        public void DisconnectStreamerBot()
        {
            if (wsClient != null)
            {
                wsClient.OnOpen -= WsClient_OnOpen;
                wsClient.OnMessage -= WsClient_OnMessage;
                wsClient.OnClose -= WsClient_OnClose;
                wsClient.OnError -= WsClient_OnError;
                wsClient.Disconnect();
            }
            wsClient = null;
        }

        public void SendToTwitchChat(string message, string actionName, string actionId)
        {
            if (wsClient == null) { return; }

            var twitchMessage = new SBRequestModels.SendMessageModel(message, actionId: actionId, actionName: actionName);
            wsClient.SendMessage(JsonConvert.SerializeObject(twitchMessage));
        }

        /// <summary>
        /// Invokes an action in StreamerBot.
        /// </summary>
        public void DoAction(string actionName, string actionId)
        {
            if (wsClient == null) { return; }

            var actionRequest = new SBRequestModels.SendActionModel(actionId: actionId, actionName: actionName);
            wsClient.SendMessage(JsonConvert.SerializeObject(actionRequest));
        }

        /// <summary>
        /// Invokes an action in StreamerBot with arguments
        /// </summary>
        public void DoAction(string actionName, string actionId, string argument)
        {
            if (wsClient == null) { return; }

            // TODO: use the argument
            var actionRequest = new SBRequestModels.SendActionModel(actionId: actionId, actionName: actionName);
            wsClient.SendMessage(JsonConvert.SerializeObject(actionRequest));
        }
    }
}
