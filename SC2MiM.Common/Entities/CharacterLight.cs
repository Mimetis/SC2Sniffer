using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name="charl")]
    public class CharacterLight
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
        
        [DataMember(Name = "mpr")]
        public RaceType MostPlayedRace { get; set; }
    }
}
