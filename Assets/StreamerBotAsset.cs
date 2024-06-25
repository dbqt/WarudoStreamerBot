using System.Threading.Tasks;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Localization;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core;

namespace QTExtensions.StreamerBot
{
    /// <summary>
    /// Asset to enable StreamerBot integration.
    /// </summary>
    [AssetType(
        Id = "f30ca027-aa50-4668-981c-93ab905c8f2b",
        Title = "STREAMERBOT_TITLE", 
        Category = "CATEGORY_EXTERNAL_INTEGRATION", 
        Singleton = true)]
    public class StreamerBotAsset : Asset
    {
        [Markdown]
        public string Status = "Not started";

        [DataInput]
        [Label("STREAMERBOT_IP")]
        public string IpAddress = "127.0.0.1";

        [DataInput]
        [Label("STREAMERBOT_PORT")]
        public int Port = 5050;

        [Trigger]
        [Label("STREAMERBOT_REFRESH")]
        public void TriggerReset()
        {
            ResetClient();
        }

        private StreamerBotClient client => Context.PluginManager.GetPlugin<StreamerBotPlugin>().StreamerBot;
        private bool isConnected = false;
        private float reconnectTimer = 0f;

        private const float ReconnectInterval = 5f;

        protected override void OnCreate()
        {
            Log("OnCreate called");
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
            Log("OnDestroy called");
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

        public override void OnUpdate()
        {
            // If not connected, try again once in a while
            if (!isConnected)
            {
                reconnectTimer += Time.deltaTime;
                if (reconnectTimer > ReconnectInterval)
                {
                    reconnectTimer = 0f;
                    Log("Trying to connect to StreamerBot...");
                    ResetClient();
                }
            }
            base.OnUpdate();
        }

        /// <summary>
        /// Disconnects the websocket client and reconnects it with the current address and port.
        /// </summary>
        private async void ResetClient()
        {
            if (client == null) { return; }

            await Task.Run(async () =>
            {
                client.DisconnectStreamerBot();
                await Context.PluginManager.GetPlugin<CorePlugin>().BeforeListenToPort();

                client.ConnectStreamerBot(IpAddress, Port.ToString());
                Context.PluginManager.GetPlugin<CorePlugin>().AfterListenToPort();
            });

            UpdateStatus();
        }

        /// <summary>
        /// Updates the status string on the asset.
        /// </summary>
        private void UpdateStatus()
        {
            if (isConnected)
            {
                Status = $"{"STREAMERBOT_CONNECTED".Localized()}{IpAddress}:{Port}";
            }
            else
            {
                Status = $"{"STREAMERBOT_DISCONNECTED".Localized()}{IpAddress}:{Port}";
            }

            SetActive(isConnected);
            BroadcastDataInput(nameof(Status));
            Log(Status);
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
