using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;

namespace DbqtExtensions.StreamerBot
{
    [AssetType(
        Id = "f30ca027-aa50-4668-981c-93ab905c8f2b",
        Title = "StreamerBot Integration", 
        Category = "CATEGORY_EXTERNAL_INTEGRATION", 
        Singleton = true)]
    public class StreamerBotAsset : Asset
    {
        [Markdown]
        public string Status = "Not started";

        [DataInput]
        [DisabledIf(nameof(IsConnected))]
        [Label("IP Address")]
        public string IpAddress = "127.0.0.1";

        [DataInput]
        [DisabledIf(nameof(IsConnected))]
        [Label("Port")]
        public int Port = 5050;

        private bool isConnected = false;
        private StreamerBotClient client => Context.PluginManager.GetPlugin<StreamerBotPlugin>().StreamerBot;

        public bool IsConnected() { return isConnected; }

        public bool IsDisconnected() { return !isConnected; }

        [Trigger]
        [HiddenIf(nameof(IsConnected))]
        [Label("Connect")]
        public void ConnectStreamerBot()
        {
            client?.ConnectStreamerBot(IpAddress, Port.ToString());
        }

        [Trigger]
        [HiddenIf(nameof(IsDisconnected))]
        [Label("Disconnect")]
        public void DisconnectStreamerBot()
        {
            client?.DisconnectStreamerBot();
        }

        protected override void OnCreate()
        {
            Watch(nameof(IpAddress), delegate { UpdateStatus(); });
            Watch(nameof(Port), delegate { UpdateStatus(); });

            if (client != null)
            {
                client.OnMessage += OnClientMessage;
                client.OnOpen += OnClientOpen;
                client.OnClose += OnClientClose;
                client.OnError += OnClientError;
            }
            else
            {
                Log("Client was null during OnCreate");
            }

            UpdateStatus();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            if (client != null)
            {
                client.OnMessage -= OnClientMessage;
                client.OnOpen -= OnClientOpen;
                client.OnClose -= OnClientClose;
                client.OnError -= OnClientError;
            }
            base.OnDestroy();
        }
        private void UpdateStatus()
        {
            if (isConnected)
            {
                Status = $"Connected to {IpAddress}:{Port}";
            }
            else
            {
                Status = "Not started!";
            }
            BroadcastDataInput(nameof(Status));
        }

        private void Log(string message)
        {
            Debug.Log($"[Dbqt.Asset.StreamerBot] {message}");
        }

        private void OnClientError(string msg)
        {
            Log(msg);
            UpdateStatus();
        }

        private void OnClientClose(string obj)
        {
            isConnected = false;
            UpdateStatus();
        }

        private void OnClientOpen(string obj)
        {
            isConnected = true;
            UpdateStatus();
        }

        private void OnClientMessage(string msg)
        {
            Log(msg);
            UpdateStatus();
        }
    }
}
