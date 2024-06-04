using QTExtensions.StreamerBot.Nodes;
using System;
using Warudo.Core.Attributes;
using Warudo.Core.Plugins;

namespace QTExtensions.StreamerBot
{
    [PluginType(
        Id = "Dbqt.StreamerBot",
        Name = "StreamerBot",
        Description = "STREAMERBOT_DESCRIPTION",
        Author = "Dbqt",
        Version = "0.0.1",
        NodeTypes = new Type[]
        {
           // typeof(OnStreamerBotEventNode),
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
