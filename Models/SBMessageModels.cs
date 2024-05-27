using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbqtExtensions.StreamerBot.Models
{
    public class SBMessageModels
    {
        public class SBMessageModel
        {
            public string id;
        }

        public class SBGenericModel
        {
            public string timeStamp;

            [JsonProperty(PropertyName = "event")]
            public SBEventModel SBevent;
        }

        public class SBEventModel
        {
            public string source;
            public string type;
        }

        public class SBEventsModel: SBMessageModel
        {
            public SBEventsListModel events;
        }

        public class SBEventsListModel
        {
            public string[] twitch;
            // TODO: support everything
        }

        public class SBTChatMessageModel : SBGenericModel
        {
            public SBTChatMessageDataModel data;
        }

        public class SBTFollowModel : SBGenericModel
        {
            public SBTFollowDataModel data;
        }

        public class SBTRaidModel : SBGenericModel
        {
            public SBTRaidDataModel data;
        }

        public class SBTSubsModel : SBGenericModel
        {
            public SBTSubsDataModel data;
        }

        public class SBTResubModel : SBGenericModel
        {
            public SBTResubDataModel data;
        }

        public class SBTGiftSubModel : SBGenericModel
        {
            public SBTGiftSubDataModel data;
        }

        public class SBTGiftBombModel : SBGenericModel
        {
            public SBTGiftBombDataModel data;
        }

        public class SBTRedeemModel : SBGenericModel
        {
            public SBTRedeemRedemptionDataModel data;
        }

        public class SBTHypeTrainStartModel : SBGenericModel
        {
            public SBTHypeTrainStartDataModel data;
        }

        public class SBTHypeTrainUpdateModel : SBGenericModel
        {
            public SBTHypeTrainUpdateDataModel data;
        }

        public class SBTHypeTrainLevelUpModel : SBGenericModel
        {
            public SBTHypeTrainLevelUpDataModel data;
        }

        public class SBTHypeTrainEndModel : SBGenericModel
        {
            public SBTHypeTrainEndDataModel data;
        }

        public class SBTEmoteModel
        {
            public string id;
            public string type;
            public string name;
            public int startIndex;
            public int endIndex;
            public string imageUrl;

            public bool EnsureIdExist()
            {
                // If the id is missing, extract it from the url depending on the type
                if (string.IsNullOrEmpty(id))
                {
                    if (type.StartsWith("BTTV"))
                    {
                        // https://cdn.betterttv.net/emote/58d2e73058d8950a875ad027/3x
                        var uri = new Uri(imageUrl);
                        var id = uri.Segments[2];
                        this.id = this.type + id.Substring(0, id.Length - 1); // Remove the trailing '/'
                    }
                    else if (type.StartsWith("FFZ"))
                    {
                        // FFZChannel, url: https://cdn.frankerfacez.com/emote/632194/4
                        var uri = new Uri(imageUrl);
                        var id = uri.Segments[2];
                        this.id = this.type + id.Substring(0, id.Length - 1); // Remove the trailing '/'
                    }
                    else if (type.StartsWith("7TV"))
                    {
                        // 7TVChannel, url: https://cdn.7tv.app/emote/60eb0c5c26a7a96f8a4c0a5b/4x
                        var uri = new Uri(imageUrl);
                        var id = uri.Segments[2];
                        this.id = this.type + id.Substring(0, id.Length - 1); // Remove the trailing '/'
                    }
                    // We don't know what kind of emote this is
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public class SBTCheerEmoteModel
        {
            public int bits;
            public string name;
            public string color;
            public string type;
            public int startIndex;
            public string endIndex;
            public string imageUrl;
        }

        public class SBTChatMessageDataModel
        {
            public TChatMessageModel message;
        }

        public class TChatMessageModel
        {
            public int userId;
            public string username;
            public string displayName;
            public string message;
            public int role;
            public bool subscriber;
            public bool isCustomReward;
            public bool hasBits;
            public int bits;
            public SBTEmoteModel[] emotes;
            public SBTCheerEmoteModel[] cheerEmotes;
        }

        public class SBTFollowDataModel
        {
            public int userId;
            public string userName;
            public string displayName;
        }

        public class SBTRaidDataModel
        {
            public int viewerCount;
            public int userId;
            public string userName;
            public string displayName;
            public int role;
        }

        public class SBTRewardDataModel
        {
            public string id;
            public string title;
            public int cost;
            public string prompt;
        }

        public class SBTRedeemRedemptionDataModel : SBMessageModel
        {
            public SBTRewardDataModel reward;
            public int user_id;
            public string user_name;
            public string user_input;
            public string status;
            public string redeemed_at;
        }

        public class SBTSubsDataModel
        {
            public int subTier;
            public string message;
            public int userId;
            public string userName;
            public string displayName;
            public int role;
            public SBTEmoteModel[] emotes;
        }

        public class SBTResubDataModel
        {
            public int cumulativeMonths;
            public bool shareStreak;
            public int streakMonths;
            public int subTier;
            public string message;
            public int userId;
            public string userName;
            public string displayName;
            public int role;
            public SBTEmoteModel[] emotes;
        }

        public class SBTGiftSubDataModel
        {
            public bool isAnonymous;
            public int totalSubsGifted;
            public int cumulativeMonths;
            public int monthGifted;
            public bool fromSubBomb;
            public int subBombCount;
            public string recipientUsername;
            public string recipientDisplayName;
            public int subTier;
            public int userId;
            public string userName;
            public string displayName;
            public int role;
        }

        public class SBTGiftBombDataModel
        {
            public bool isAnonymous;
            public int gifts;
            public int totalGifts;
            public int subTier;
            public int userId;
            public string userName;
            public string displayName;
            public int role;
        }

        public class SBTHypeTrainStartDataModel
        {
            public int level;
            public int levelGoal;
            public int levelTotal;
            public int totalGoal;
            public int total;
            public float percent;
        }

        public class SBTHypeTrainUpdateDataModel
        {
            public int contributors;
            public int level;
            public int levelGoal;
            public int levelTotal;
            public int totalGoal;
            public int total;
            public float percent;
        }

        public class SBTHypeTrainLevelUpDataModel
        {
            public int prevLevel;
            public int contributors;
            public int level;
            public int levelGoal;
            public int levelTotal;
            public int totalGoal;
            public int total;
            public float percent;
        }

        public class SBTHypeTrainEndDataModel
        {
            public SBTHypeTrainContributor conductor;
            public int level;
            public int contributorCount;
            public SBTHypeTrainContributor[] contributors;
            public int levelGoal;
            public int levelTotal;
            public int totalGoal;
            public int total;
            public float percent;
        }

        public class SBTHypeTrainContributor
        {
            public int id;
        }
    }
}
