using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract]
    public class LeagueSummary
    {
        [DataMember]
        public LeagueType LeagueType { get; set; }
        [DataMember]
        public LeagueTopRank LeagueTopRank { get; set; }
        [DataMember]
        public TeamType TeamType { get; set; }
        [DataMember]
        public String Division { get; set; }
        [DataMember]
        public int BestRank { get; set; }
        [DataMember]
        public int VictoriesCount { get; set; }
        [DataMember]
        public int MatchesCount { get; set; }

        public int LossesCount
        {
            get
            {
                return MatchesCount - VictoriesCount;
            }
        }

    }
}
