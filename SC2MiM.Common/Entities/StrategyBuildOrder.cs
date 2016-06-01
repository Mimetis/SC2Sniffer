using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "sbo")]
    public class StrategyBuildOrder
    {
        [DataMember(Name = "sboid")]
        public Int32 BuildOrderId { get; set; }

        [DataMember(Name = "sid")]
        public Int32 StrategyId { get; set; }

        [DataMember(Name = "u")]
        public String Unit { get; set; }

        [DataMember(Name = "img")]
        public Byte[] Image { get; set; }

        [DataMember(Name = "o")]
        public Byte Order { get; set; }

        [DataMember(Name = "t")]
        public TimeSpan Time { get; set; }

        [DataMember(Name = "i")]
        public String Instruction { get; set; }

        [DataMember(Name = "d")]
        public String Description { get; set; }


    }
}
