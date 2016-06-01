using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "dp")]
    public class DeviceProperties
    {
        [DataMember(Name = "did")]
        public Guid DeviceId { get; set; }

        [DataMember(Name = "dm")]
        public String DeviceManufacturer { get; set; }

        [DataMember(Name = "dn")]
        public String DeviceName { get; set; }

        [DataMember(Name = "dfv")]
        public String DeviceFirmwareVersion { get; set; }

        [DataMember(Name = "dhv")]
        public String DeviceHardwareVersion { get; set; }

        [DataMember(Name = "dt")]
        public String DeviceType { get; set; }

        [DataMember(Name = "lmd")]
        public DateTime LastModifiedDate { get; set; }


    }
}
