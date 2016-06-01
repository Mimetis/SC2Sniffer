using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "charlig")]
    public class CharacterLeague
    {
        [DataMember(Name = "rid")]
        public String RegionId { get; set; }

        [DataMember(Name = "cid")]
        public Int32 CharacterId { get; set; }

        [DataMember(Name = "cnm")]
        public String Name { get; set; }

        [DataMember(Name = "zid")]
        public Int32 ZoneId { get; set; }

        [DataMember(Name = "mpr")]
        public RaceType MostPlayedRace { get; set; }
        
        [DataMember(Name = "rnk")]
        public Byte Rank { get; set; }

        [DataMember(Name = "pts")]
        public int Points { get; set; }

        [DataMember(Name = "vic")]
        public int VictoriesCount { get; set; }

        [DataMember(Name = "loss")]
        public int LossesCount { get; set; }

        [DataMember(Name = "lmd")]
        public DateTime LastModifiedDate { get; set; }

        public override string ToString()
        {
            return String.Concat("CharacterId:", CharacterId, " - Name:", Name, " - RegionId:", RegionId, " - ZoneId:", ZoneId,
                          " - MostPlayedRace:", MostPlayedRace.ToString(), " - Rank:", Rank, " - Points:", Points,
                          " - Victories:", VictoriesCount, " - LossesCount:", LossesCount);

        }
    }
}
