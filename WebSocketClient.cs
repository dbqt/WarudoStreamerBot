using System;
using WebSocketSharp;

namespace QTExtensions.StreamerBot
{
    /// <summary>
    /// Generic client encapsulating a <see cref="WebSocket"/> while exposing some basic functionality and events.
    /// </summary>
    public class WebSocketClient
    {
        private string webSocketAddress;
        private string webSocketPort;

        private WebSocket webSocket = null;

        public event Action<string> OnOpen;
        public event Action<string> OnClose;
        public event Action<string> OnMessage;
        public event Action<string> OnError;

        public void Connect()
        {
            webSocket?.Connect();
        }

        public void Disconnect()
        {
            webSocket?.Close();
        }

        public void Initialize(string address = "127.0.0.1", string port = "5050")
        {
            webSocketAddress = address;
            webSocketPort = port;
            webSocket = new WebSocket($"ws://{webSocketAddress}:{webSocketPort}");
            webSocket.OnOpen += WebSocket_OnOpen;
            webSocket.OnClose += WebSocket_OnClose;
            webSocket.OnMessage += WebSocket_OnMessage;
            webSocket.OnError += WebSocket_OnError;
        }

        public void CleanUp()
        {
            if (webSocket != null)
            {
                webSocket.OnOpen -= WebSocket_OnOpen;
                webSocket.OnClose -= WebSocket_OnClose;
                webSocket.OnMessage -= WebSocket_OnMessage;
                webSocket.OnError -= WebSocket_OnError;
            }
        }

        public void SendMessage(string message)
        {
            webSocket.Send(message);
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke($"OnError: {e.Message} | {e.Exception.Message} | {e.Exception.StackTrace}");
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            OnMessage?.Invoke(e.Data);
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            OnClose?.Invoke($"OnClose: {e.Reason}");
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            OnOpen?.Invoke("OnOpen");
        }
    }
}
