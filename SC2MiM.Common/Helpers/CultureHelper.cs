using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SC2MiM.Common.Helpers
{
    public class CultureHelper
    {


        public static DateTime? GetDateTimeFromCultureId(string cultureId, string dateText)
        {

            CultureInfo ci;

            switch (cultureId)
            {
                case "fr":
                    ci = new CultureInfo("fr-FR");
                    break;
                default:
                    ci = new CultureInfo("en-US");
                    break;
            }
            DateTime result;

            if (DateTime.TryParse(dateText, ci, DateTimeStyles.AdjustToUniversal, out result))
                return result;
            
            return null;
        }

        static CultureInfo defaultCultureInfo = new CultureInfo("en-us");

        public static string GetDefaultCultureId(string regionId)
        {
            if (regionId == "kr")
                return "ko";

            return "en";
        }

        public static CultureInfo DefaultCultureInfo
        {
            get
            {
                return defaultCultureInfo;
            }
        }

        public static string DefaultCultureId
        {
            get { return "en"; }
        }

        public static byte DefaultBattleZoneId
        {
            get { return 1; }
        }
    }
}
