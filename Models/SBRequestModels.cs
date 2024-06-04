using System.Collections.Generic;

namespace QTExtensions.StreamerBot.Models
{
    /// <summary>
    /// Models for serialization of StreamerBot outgoing requests.
    /// </summary>
    public class SBRequestModels
    {
        private const string DoActionRequestType = "DoAction";
        public class EventsModel
        {
            public string[] Twitch;
        }

        public class ActionModel
        {
            public string id;
            public string name;
        }

        public class RequestModel
        {
            public string request;
            public string id;
        }

        public class GetEventsModel : RequestModel
        {
            public GetEventsModel()
            {
                request = "GetEvents";
                id = "geteventsid";
            }
        }

        public class SubscribeModel : RequestModel
        {
            public EventsModel events;

            public SubscribeModel(SBEnums.EventType eventType, string[] eventsToAdd, string requestId = "subscribeid")
            {
                request = "Subscribe";
                id = requestId;
                events = new EventsModel();
                switch (eventType)
                {
                    case SBEnums.EventType.Twitch:
                        events.Twitch = eventsToAdd;
                        break;
                }
            }
        }

        public class UnsubscribeModel : RequestModel
        {
            public EventsModel events;

            public UnsubscribeModel(SBEnums.EventType eventType, string[] eventsToRemove, string requestId = "unsubscribeid")
            {
                request = "UnSubscribe";
                id = requestId;
                events = new EventsModel();
                switch (eventType)
                {
                    case SBEnums.EventType.Twitch:
                        events.Twitch = eventsToRemove;
                        break;
                }
            }
        }

        public class GetActionsModel : RequestModel
        {
            public GetActionsModel()
            {
                request = "GetActions";
                id = "getactionsid";
            }
        }

        public class SendActionModel : RequestModel
        {
            public ActionModel action;
            public Dictionary<string, string> args;

            public SendActionModel(string actionId = "", string actionName = "", Dictionary<string, string> arguments = null)
            {
                request = DoActionRequestType;
                id = actionName;
                action = new ActionModel() { id = actionId, name = actionName };

                if (arguments != null)
                {
                    args = arguments;
                }
            }
        }
    }
}
