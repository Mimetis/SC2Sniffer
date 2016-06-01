using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "RaceType")]
    public enum RaceType : int
    {
        [EnumMember(Value = "None")]
        None = 0,
        [EnumMember(Value = "Terran")]
        Terran = 1,
        [EnumMember(Value = "Protoss")]
        Protoss = 2,
        [EnumMember(Value = "Zerg")]
        Zerg = 3,
        [EnumMember(Value = "Random")]
        Random = 4,
    }

    public class RaceTypeServices
    {

        /// <summary>
        /// Parses a race from text
        /// </summary>
        public static RaceType ParseRaceTypeContainedInText(String text)
        {

            text = text.Trim().ToLower();

            // culture en, toujours

            if (text.Contains("zerg"))
                return RaceType.Zerg;

            if (text.Contains("terran"))
                return RaceType.Terran;

            if (text.Contains("protoss"))
                return RaceType.Protoss;

            if (text.Contains("random"))
                return RaceType.Random;

            return RaceType.None;


        }
    }
}
