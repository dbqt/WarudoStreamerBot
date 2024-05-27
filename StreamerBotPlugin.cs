﻿using DbqtExtensions.StreamerBot.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warudo.Core.Attributes;
using Warudo.Core.Plugins;

namespace DbqtExtensions.StreamerBot
{
    [PluginType(
        Id = "Dbqt.StreamerBot",
        Name = "StreamerBot",
        Description = "Receive and send events through StreamerBot using Websocket",
        Author = "Dbqt",
        Version = "0.0.1",
        NodeTypes = new Type[]
        {
           typeof(OnStreamerBotEventNode),
           typeof(StreamerBotDoActionNode),
        },
        AssetTypes = new Type[]
        {
            typeof(StreamerBotAsset)
        }
    )]

    public class StreamerBotPlugin : Plugin
    {
        public StreamerBotClient StreamerBot { get; } = new StreamerBotClient();
    }
}
