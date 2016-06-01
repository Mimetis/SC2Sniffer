using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "s")]
    public class Strategy
    {
        [DataMember(Name = "sid")]
        public Int32 StrategyId { get; set; }

        [DataMember(Name = "mu")]
        public String MatchUp { get; set; }

        [DataMember(Name = "t")]
        public String Title { get; set; }

        [DataMember(Name = "d")]
        public String Description { get; set; }

        [DataMember(Name = "n")]
        public Byte Note { get; set; }

        [DataMember(Name = "lmd")]
        public DateTime LastModifiedDate { get; set; }

    }
}
