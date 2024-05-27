using Newtonsoft.Json;
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
using static DbqtExtensions.StreamerBot.Models.SBMessageModels;

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
        [Label("IP Address")]
        public string IpAddress = "127.0.0.1";

        [DataInput]
        [Label("Port")]
        public int Port = 5050;

        [Trigger]
        [Label("Refresh Connection")]
        public void TriggerReset()
        {
            ResetClient();
        }

        private StreamerBotClient client => Context.PluginManager.GetPlugin<StreamerBotPlugin>().StreamerBot;
        private bool isConnected = false;

        protected override void OnCreate()
        {
            // Watch for address and port changes, reset the websocket client when it happens
            Watch(nameof(IpAddress), delegate { ResetClient(); });
            Watch(nameof(Port), delegate { ResetClient(); });

            // Subscribe to client events
            if (client != null)
            {
                client.OnMessage += OnClientMessage;
                client.OnOpen += OnClientOpen;
                client.OnClose += OnClientClose;
                client.OnError += OnClientError;
            }

            ResetClient();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            // Clean up client events
            if (client != null)
            {
                client.OnMessage -= OnClientMessage;
                client.OnOpen -= OnClientOpen;
                client.OnClose -= OnClientClose;
                client.OnError -= OnClientError;
                client.DisconnectStreamerBot();
            }
            base.OnDestroy();
        }

        /// <summary>
        /// Disconnects the websocket client and reconnects it with the current address and port.
        /// </summary>
        private async void ResetClient()
        {
            if (client == null) { return; }

            client.DisconnectStreamerBot();
            await Context.PluginManager.GetPlugin<CorePlugin>().BeforeListenToPort();

            client.ConnectStreamerBot(IpAddress, Port.ToString());
            Context.PluginManager.GetPlugin<CorePlugin>().AfterListenToPort();

            UpdateStatus();
        }

        /// <summary>
        /// Updates the status string on the asset.
        /// </summary>
        private void UpdateStatus()
        {
            if (isConnected)
            {
                Status = $"Connected to {IpAddress}:{Port}";
            }
            else
            {
                Status = $"Disconnected - attempting to connect to {IpAddress}:{Port}";
            }
            BroadcastDataInput(nameof(Status));
        }

        /// <summary>
        /// Logs the message into Unity logs.
        /// </summary>
        /// <param name="message"></param>
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

        private void OnClientMessage(string obj)
        {
            Log(obj);
        }
    }
}
