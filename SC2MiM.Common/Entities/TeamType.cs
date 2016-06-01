using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "TeamType")]
    public enum TeamType : int
    {
        [EnumMember(Value = "OneVOne")]
        OneVOne = 1,
        [EnumMember(Value = "TwoVTwo")]
        TwoVTwo = 2,
        [EnumMember(Value = "ThreeVThree")]
        ThreeVThree = 3,
        [EnumMember(Value = "FourVFour")]
        FourVFour = 4,
    }

    public class TeamTypeServices
    {

        public static TeamType ParseTeamTypeFromShortText(string teamTypeShortText)
        {

            switch (teamTypeShortText.Trim().ToLower().Substring(0,1))
            {
                case "1": return TeamType.OneVOne;
                case "2": return TeamType.TwoVTwo;
                case "3": return TeamType.ThreeVThree;
                case "4": return TeamType.FourVFour;
                default: return TeamType.OneVOne;

            }
            
        }

        public static String GetTeamTypeString(TeamType tt)
        {
            switch (tt)
            {
                case TeamType.OneVOne: return "1v1";
                case TeamType.TwoVTwo: return "2v2";
                case TeamType.ThreeVThree: return "3v3";
                case TeamType.FourVFour : return "4v4";
                default: return "1v1";

            }
        }
        public static TeamType ParseTeamTypeContainedInText(string text)
        {

            text = text.Trim().ToLower();

            if (text.Contains("1v1"))
                return TeamType.OneVOne;
            if (text.Contains("2v2"))
                return TeamType.TwoVTwo;
            if (text.Contains("3v3"))
                return TeamType.ThreeVThree;
            if (text.Contains("4v4"))
                return TeamType.FourVFour;

            return TeamType.OneVOne;
        }
    }
}
