using QTExtensions.StreamerBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QTExtensions.StreamerBot.Models.SBMessageModels;
using static QTExtensions.StreamerBot.Models.SBRequestModels;

namespace QTExtensions.StreamerBot
{
    /// <summary>
    /// Client encapsulating the websocket client logic to communicate with StreamerBot and exposes some of its data.
    /// </summary>
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
        private string[] twitchEvents = { };

        public string[] Actions => actions;
        private string[] actions = { };

        public Dictionary<string, SBActionModel> ActionGuidToModel = new Dictionary<string, SBActionModel>();

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

            var genericMessage = JsonConvert.DeserializeObject<SBMessageModel>(obj);

            // Specifically handle the geteventsid event
            if (genericMessage != null && genericMessage.id != null && genericMessage.id.Equals("geteventsid"))
            {
                var getEventsMessage = JsonConvert.DeserializeObject<SBEventsModel>(obj);
                if (getEventsMessage != null && getEventsMessage.events != null)
                {
                    // TODO: Get other types of events as well
                    twitchEvents = getEventsMessage.events.twitch;
                }
            }
            // Specifically handle the getactionsid event
            else if (genericMessage != null && genericMessage.id != null && genericMessage.id.Equals("getactionsid"))
            {
                var getActionsMessage = JsonConvert.DeserializeObject<SBActionsModel>(obj);
                if (getActionsMessage != null)
                {
                    ActionGuidToModel.Clear();

                    List<string> actionIds = new List<string>();
                    foreach (var action in getActionsMessage.actions)
                    {
                        actionIds.Add(action.id);
                        ActionGuidToModel[action.id] = action;
                    }

                    actions = actionIds.ToArray();
                }
            }
            // Handle every other events
            else
            {
                var genericEvent = JsonConvert.DeserializeObject<SBGenericModel>(obj);
                if (genericEvent != null && genericEvent.SBevent != null)
                {
                    // Find all the event handlers listening to this event type
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

            // Retrieve the events and actions once on connecting
            RefreshEvents();
            RefreshActions();

            OnOpen?.Invoke(obj);
        }

        /// <summary>
        /// Whether the WebSocket client is connected.
        /// </summary>
        public bool IsReady()
        {
            return wsClient != null && ConnectionStatus == Status.Connected;
        }

        /// <summary>
        /// Retrieve all events from StreamerBot.
        /// </summary>
        public void RefreshEvents()
        {
            if (!IsReady()) { return; }

            wsClient.SendMessage(JsonConvert.SerializeObject(new GetEventsModel()));
        }

        /// <summary>
        /// Retrive all actions from StreamerBot.
        /// </summary>
        public void RefreshActions()
        {
            if (!IsReady()) { return; }

            wsClient.SendMessage(JsonConvert.SerializeObject(new GetActionsModel()));
        }

        /// <summary>
        /// Request StreamerBot to send specific events.
        /// </summary>
        public string SubscribeEvent(SBEnums.EventType eventType, string eventName, IStreamerBotEventHandler handler)
        {
            if (!IsReady()) { return null; }

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
            if (!IsReady()) { return; }
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

        /// <summary>
        /// Invokes an action in StreamerBot.
        /// </summary>
        public void DoAction(string actionName, string actionId)
        {
            DoAction(actionName, actionId);
        }

        /// <summary>
        /// Invokes an action in StreamerBot with arguments
        /// </summary>
        public void DoAction(string actionName, string actionId, Dictionary<string, string> arguments = null)
        {
            if (!IsReady()) { return; }

            var actionRequest = new SendActionModel(actionId: actionId, actionName: actionName, arguments: arguments);
            wsClient.SendMessage(JsonConvert.SerializeObject(actionRequest));
        }
    }
}
