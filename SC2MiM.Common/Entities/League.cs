using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name="l")]
    public class League
    {
        [DataMember(Name = "lid")]
        public int LeagueId { get; set; }

        [DataMember(Name = "rid")]
        public String RegionId { get; set; }

        [DataMember(Name = "zid")]
        public int ZoneId { get; set; }
        
        [DataMember(Name = "dnd")]
        public String DivisionName { get; set; }

        [DataMember(Name = "tt")]
        public TeamType TeamType { get; set; }

        [DataMember(Name = "lt")]
        public LeagueType LeagueType { get; set; }

        [DataMember(Name = "lmd")]
        public DateTime LastModifiedDateTime { get; set; }

        [DataMember(Name = "isr")]
        public bool IsRandom { get; set; }

        [DataMember(Name = "chars")]
        public List<CharacterLeague> Characters { get; set; }

        public League()
        {
            Characters = new List<CharacterLeague>();
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2}", this.DivisionName, this.LeagueType, this.TeamType);
        }
    }
}
