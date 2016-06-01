using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "charr")]
    public class CharacterReward
    {
        [DataMember(Name = "rid")]
        public String RegionId { get; set; }

        [DataMember(Name = "cid")]
        public Int32 CharacterId { get; set; }

        [DataMember(Name = "zid")]
        public Int32 ZoneId { get; set; }

        [DataMember(Name = "rewid")]
        public String RewardId { get; set; }

        [DataMember(Name = "n")]
        public String Name { get; set; }

        [DataMember(Name = "d")]
        public String Description { get; set; }

        [DataMember(Name = "fd")]
        public String FullDescription { get; set; }

        [DataMember(Name = "lmd")]
        public DateTime LastModifiedDate { get; set; }
    }
}
