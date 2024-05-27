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
        Title = "StreamerBot Receiver", 
        Category = "CATEGORY_EXTERNAL_INTEGRATION", 
        Singleton = true)]
    public class StreamerBotReceiverAsset : Asset
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

        public bool IsConnected() { return isConnected; }

        public bool IsDisconnected() { return !isConnected; }

        [Trigger]
        [HiddenIf(nameof(IsConnected))]
        [Label("Connect")]
        public void ConnectStreamerBot()
        {
            ToggleConnection();
        }

        [Trigger]
        [HiddenIf(nameof(IsDisconnected))]
        [Label("Disconnect")]
        public void DisconnectStreamerBot()
        {
            ToggleConnection();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            Watch(nameof(IpAddress), delegate { UpdateConnection(); });
            Watch(nameof(Port), delegate { UpdateConnection(); });
        }

        public void ToggleConnection()
        {
            isConnected = !isConnected;

            UpdateConnection();
        }

        private void UpdateConnection()
        {
            if (isConnected)
            {
                Status = $"Connected to {IpAddress}:{Port}";
            }
            else
            {
                Status = "Not started";
            }
            BroadcastDataInput(nameof(Status));
        }
    }
}
