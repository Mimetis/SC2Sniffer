using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SC2MiM.Common.Entities
{
    [DataContract(Name = "LeagueType")]
    public enum LeagueType : int
    {
        [EnumMember(Value = "None")]
        None = 0,
        [EnumMember(Value = "Bronze")]
        Bronze = 1,
        [EnumMember(Value = "Silver")]
        Silver = 2,
        [EnumMember(Value = "Gold")]
        Gold = 3,
        [EnumMember(Value = "Platinum")]
        Platinum = 4,
        [EnumMember(Value = "Diamond")]
        Diamond = 5,
        [EnumMember(Value = "Master")]
        Master = 6,
        [EnumMember(Value = "GrandMaster")]
        GrandMaster = 7
    }

    public enum LeagueTopRank : int
    {
        [EnumMember(Value = "None")]
        None = 0,
        [EnumMember(Value = "Top8")]
        Top8 = 8,
        [EnumMember(Value = "Top25")]
        Top25 = 25,
        [EnumMember(Value = "Top50")]
        Top50 = 50,
        [EnumMember(Value = "Top100")]
        Top100 = 100,
    }

    public class LeagueTypeServices
    {
    

        /// <summary>
        /// Parses the league type from text.
        /// Something like "badge-master badge-medium-3"
        /// </summary>
        public static LeagueType ParseLeagueTypeContainedInText(String text)
        {

            text = text.Trim().ToLower();

            // culture en, toujours

            if (text.Contains("bronze"))
                return LeagueType.Bronze;

            if (text.Contains("silver"))
                return LeagueType.Silver;

            if (text.Contains("gold"))
                return LeagueType.Gold;

            if (text.Contains("diamond"))
                return LeagueType.Diamond;

            if (text.Contains("platinum"))
                return LeagueType.Platinum;

            if (text.Contains("grandmaster"))
                return LeagueType.GrandMaster;

            if (text.Contains("master"))
                return LeagueType.Master;

            return LeagueType.None;


        }

        /// <summary>
        /// Parses the league rank from text.
        /// Something like "badge-master badge-medium-3"
        /// </summary>
        public static LeagueTopRank ParseLeagueTopRankContainedInText(String text)
        {

            text = text.Trim().ToLower();

            // culture en, toujours

            if (text.Contains("badge-medium-4"))
                return LeagueTopRank.Top8;

            if (text.Contains("badge-medium-3"))
                return LeagueTopRank.Top25;

            if (text.Contains("badge-medium-2"))
                return LeagueTopRank.Top50;

            if (text.Contains("badge-medium-1"))
                return LeagueTopRank.Top100;


            return LeagueTopRank.None;


         }

        /// <summary>
        /// Parses the league type from text.
        /// </summary>
        public static Boolean ParseLeagueTypeIsRandomContainedInText(String text)
        {

            text = text.Trim().ToLower();

            // culture en, toujours

            if (text.Contains("random"))
                return true;

            return false;

        }


        ///// <summary>
        ///// Parses the name of the league type from image name.
        ///// </summary>
        //public static LeagueType ParseLeagueTypeFromImageName(string imageName)
        //{
        //    string shortImageName = imageName;

        //    Int32 indexOfDot = shortImageName.IndexOf(".");

        //    // Virage du dernier "/" si il existe
        //    if (shortImageName.EndsWith("/"))
        //        shortImageName = shortImageName.Substring(0, shortImageName.Length - 1);

        //    // Récupération de l'url sans l'extension
        //    if (indexOfDot > 0)
        //        shortImageName = shortImageName.Substring(0, indexOfDot);

        //    // Virage du -medium (ou autre) de l'url
        //    Int32 indexOfTiret = shortImageName.LastIndexOf("-");
        //    if (indexOfTiret > 0)
        //        shortImageName = shortImageName.Substring(0, indexOfTiret);

        //    // Index du dernier "/" 
        //    int lastSlashPosition = shortImageName.LastIndexOf("/");

        //    // Index du début du nom de l'image
        //    int indexOfImageName = lastSlashPosition + 1;

        //    // Nom de l'image
        //    shortImageName = shortImageName.Substring(indexOfImageName);


        //    switch (shortImageName.ToLower())
        //    {
        //        case "bronze": return LeagueType.Bronze;
        //        case "silver": return LeagueType.Silver;
        //        case "gold": return LeagueType.Gold;
        //        case "platinum": return LeagueType.Platinum;
        //        case "diamond": return LeagueType.Diamond;
        //        case "master": return LeagueType.Master;
        //        case "grandmaster": return LeagueType.GrandMaster;
        //        default: return LeagueType.None;

        //    }

        //}

    }
}
