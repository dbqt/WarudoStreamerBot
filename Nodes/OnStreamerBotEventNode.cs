using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace DbqtExtensions.StreamerBot.Nodes
{
    [NodeType(
    Id = "3ee08a89-d2be-4eb7-b6e2-930b928650c3",
    Title = "On StreamerBot Event",
    Category = "StreamerBot")]
    public class OnStreamerBotEventNode : Node
    {
        [DataInput]
        public string EventName;

        // Usually, we name the default flow input "Enter". You are of course free to name a flow input differently.
        [FlowInput]
        public Continuation Enter()
        {
            return null;
        }

        // Usually, we name the default flow input "Exit".
        [FlowOutput]
        public Continuation Exit;
    }
}
