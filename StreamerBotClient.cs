using DbqtExtensions.StreamerBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public StreamerBotClient()
        {
            ConnectionStatus = Status.Disconnected;
        }

        ~StreamerBotClient()
        {
            if (wsClient != null)
            {
                wsClient.Disconnect();
                wsClient.OnOpen -= WsClient_OnOpen;
                wsClient.OnMessage -= WsClient_OnMessage;
                wsClient.OnClose -= WsClient_OnClose;
                wsClient.OnError -= WsClient_OnError;
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
        }

        private void WsClient_OnOpen(string obj)
        {
            ConnectionStatus = Status.Connected;
            SubscribeTwitchEvents();

            OnOpen?.Invoke(obj);
        }

        private void SubscribeTwitchEvents()
        {
            if (wsClient == null) { return; }

            var subTwitchEvents = new SBRequestModels.SubscribeModel(new string[] { "ChatMessage", "RewardRedemption" });
            wsClient.SendMessage(JsonConvert.SerializeObject(subTwitchEvents));
        }

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

        public void DisconnectStreamerBot()
        {
            if (wsClient != null)
            { 
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

        public void DoAction(string actionName, string actionId)
        {
            if (wsClient == null) { return; }

            var actionRequest = new SBRequestModels.SendActionModel(actionId: actionId, actionName: actionName);
            wsClient.SendMessage(JsonConvert.SerializeObject(actionRequest));
        }
    }
}
