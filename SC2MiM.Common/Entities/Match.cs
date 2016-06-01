using System;
using System.Net;
using System.Windows;

namespace SC2MiM.Common.Entities
{
    public class Match
    {
        public String Carte { get; set; }
        public String Type { get; set; }
        public String Resultat { get; set; }
        public bool IsWin { get; set; }
        public DateTime Date { get; set; }
        public int Point { get; set; }

        public String DateString
        {
            get
            {
                return this.Date.ToString();
            }
        }

        public String PointString
        {
            get
            {
                if (IsWin)
                    return "+" + Point.ToString();
                else
                    return "-" + Point.ToString();
            }
        }

        //public BitmapImage ResultImage
        //{
        //    get
        //    {
        //        if (IsWin)
        //            return new BitmapImage(UriHelper.Create("Images/MatchWin.png"));
        //        else
        //            return new BitmapImage(UriHelper.Create("Images/MatchLost.png"));

        //    }
        //}

        //public SolidColorBrush PointColor
        //{
        //    get
        //    {
        //        if (IsWin)
        //            return new SolidColorBrush(Colors.Green);
        //        else
        //            return new SolidColorBrush(Colors.Red);
        //    }
        //}
        
    }
}
