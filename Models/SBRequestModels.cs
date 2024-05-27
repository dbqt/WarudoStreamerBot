using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbqtExtensions.StreamerBot.Models
{
    public class SBRequestModels
    {
        private const string SubscribeRequestType = "Subscribe";
        private const string DoActionRequestType = "DoAction";
        public class RequestModel
        {
            public string request;
            public string id;
        }

        public class SubscribeModel : RequestModel
        {
            public EventsModel events;

            public SubscribeModel(string[] twitchEvents, string requestId = "subscribeid")
            {
                request = SubscribeRequestType;
                id = requestId;
                events = new EventsModel();
                events.Twitch = twitchEvents;
            }
        }

        public class EventsModel
        {
            public string[] Twitch;
        }

        public class SendMessageModel : RequestModel
        {
            public ActionModel action;
            public SendMessageArgs args;

            public SendMessageModel(string message, string actionId = "f43d2f87-1e95-4eae-a7ed-dd280bfa934b", string actionName = "QTBotMessage", string requestId = "sendmessageid")
            {
                request = DoActionRequestType;
                id = requestId;
                action = new ActionModel() { id = actionId, name = actionName };
                args = new SendMessageArgs() { msg = message };
            }
        }

        public class SendActionModel : RequestModel
        {
            public ActionModel action;

            public SendActionModel(string actionId = "f43d2f87-1e95-4eae-a7ed-dd280bfa934b", string actionName = "QTBotMessage")
            {
                request = DoActionRequestType;
                id = actionName;
                action = new ActionModel() { id = actionId, name = actionName };
            }
        }

        public class ActionModel
        {
            public string id;
            public string name;
        }

        public class SendMessageArgs
        {
            public string msg;
        }
    }
}
