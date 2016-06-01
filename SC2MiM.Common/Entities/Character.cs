using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SC2MiM.Common.Helpers;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "char")]
    public class Character
    {
        [DataMember(Name = "rid")]
        public String RegionId { get; set; }

        [DataMember(Name = "cid")]
        public Int32 CharacterId { get; set; }

        [DataMember(Name = "zid")]
        public Int32 ZoneId { get; set; }

        [DataMember(Name = "n")]
        public String Name { get; set; }

        [DataMember(Name = "c")]
        public String Code { get; set; }

        [DataMember(Name = "lvc")]
        public int LeaguesVictoriesCount { get; set; }

        [DataMember(Name = "lmc")]
        public int LeaguesMatchesCount { get; set; }

        [DataMember(Name = "lmd")]
        public DateTime LastModifiedDate { get; set; }

        [DataMember(Name = "ap")]
        public int AchievementPoints { get; set; }

        [DataMember(Name = "mpr")]
        public RaceType MostPlayedRace { get; set; }

        [DataMember(Name = "cb")]
        public String CampaignBadge { get; set; }

        [DataMember(Name = "lovos")]
        public LeagueSummary LeagueOneVOneSummary { get; set; }

        [DataMember(Name = "ltvts")]
        public LeagueSummary LeagueTwoVTwoSummary { get; set; }

        [DataMember(Name = "ltrvtrs")]
        public LeagueSummary LeagueThreeVThreeSummary { get; set; }

        [DataMember(Name = "lfvfs")]
        public LeagueSummary LeagueFourVFourSummary { get; set; }

        [DataMember(Name = "cr")]
        public List<CharacterReward> CharacterRewards { get; set; }

        [DataMember(Name = "ls")]
        public List<LeagueSummary> LeaguesSummary { get; set; }


        [DataMember(Name = "pu")]
        public string PortraitUrl { get; set; }
        
        [DataMember(Name = "pjpg")]
        public string PortraitJpgName { get; set; }
        
        [DataMember(Name = "ppx")]
        public int PortraitPositionX { get; set; }

        [DataMember(Name = "ppy")]
        public int PortraitPositionY { get; set; }

        public int MostMatchesInOneLeague
        {
            get
            {
                if (LeaguesSummary == null || LeaguesSummary.Count == 0)
                    return 0;

                int max = 0;
                foreach (var ls in LeaguesSummary)
                {
                    max = Math.Max(max, Math.Max(ls.VictoriesCount, ls.LossesCount));
                }

                return max;
            }
        }

        [DataMember(Name = "leags")]
        public List<League> Leagues { get; set; }

        public override bool Equals(object obj)
        {
            Character c = (Character)obj;

            return c.CharacterId == this.CharacterId && this.ZoneId == c.ZoneId && this.RegionId == c.RegionId;
        }

        public override string ToString()
        {
            return this.Name + " (" + this.CharacterId + "/" + this.ZoneId + "/" + this.MostPlayedRace + ")";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


    }
}
