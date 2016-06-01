using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC2MiM.Common.Entities;

namespace SC2MiM.Common
{
    public class Urls
    {
        public const string GlobalForumUrl = "http://{0}.battle.net/sc2/{1}/forum/";
        public const string ForumUrl = "{0}/?page={1}";
        public const string TopicUrl = "topic/{0}?page={1}";

        public const string ProfileUrl = "http://{0}.battle.net/sc2/{1}/profile/{2}/{3}/{4}/";
        public const string RewardUrl = "http://{0}.battle.net/sc2/{1}/profile/{2}/{3}/{4}/rewards/";
        public const string GlobalLadderLeaguesUrl = "http://{0}.battle.net/sc2/{1}/profile/{2}/{3}/{4}/ladder/leagues";
        public const string LadderLeaguesUrl = "http://{0}.battle.net/sc2/{1}/profile/{2}/{3}/{4}/ladder/{5}";

        public const string GrandMasterUrl = "http://{0}.battle.net/sc2/{1}/ladder/grandmaster";


    }
}
