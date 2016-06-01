using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Net.Cache;
using System.Configuration;
using System.Globalization;
using System.Diagnostics;
using SC2MiM.Common.Entities;
using SC2MiM.Common.Helpers;

namespace SC2MiM.Common.Services
{
    public class HtmlHelper
    {
        static Random r = new Random(2);
        public static event EventHandler<ApplicationException> ErrorOccuredEvent;

        private static Int32 timeOut;
        private static Int32 defaultConnectionLimit;

        private static ConcurrentDictionary<String, Tuple<HtmlDocument, HttpStatusCode>>
            documentsCache = new ConcurrentDictionary<string, Tuple<HtmlDocument, HttpStatusCode>>();



        public static HtmlDocument GetDocument(String url, out HttpStatusCode statusCode, bool noCache = true)
        {

            HttpWebResponse response = null;
            try
            {
                Tuple<HtmlDocument, HttpStatusCode> docTuple;
                if (documentsCache.TryGetValue(url, out docTuple))
                {
                    statusCode = docTuple.Item2;
                    return docTuple.Item1;
                }

                if (timeOut == 0)
                    timeOut = Int32.Parse(ConfigurationManager.AppSettings["TimeOut"]);

                if (defaultConnectionLimit == 0)
                    defaultConnectionLimit = Int32.Parse(ConfigurationManager.AppSettings["DefaultConnectionLimit"]);


                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.AllowAutoRedirect = false;
                req.AutomaticDecompression = DecompressionMethods.GZip;
                req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                req.KeepAlive = false;
                req.ServicePoint.ConnectionLimit = defaultConnectionLimit;
                req.Timeout = timeOut;

                var doc = new HtmlDocument();
                try
                {
                    response = req.GetResponse() as HttpWebResponse;
                }
                catch (WebException wex)
                {
                    response = wex.Response as HttpWebResponse;
                }
                //HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                if (response != null)
                {
                    statusCode = response.StatusCode;
                    doc.Load(response.GetResponseStream(), Encoding.UTF8);
                    docTuple = new Tuple<HtmlDocument, HttpStatusCode>(doc, statusCode);
                    if (!noCache)
                        documentsCache.TryAdd(url, docTuple);

                    response.Close();
                }
                else
                {
                    statusCode = HttpStatusCode.BadRequest;
                }

                return doc;
            }
            catch (Exception ex)
            {
                String message = "[HtmlHelper].[GetDocument] : Error. url = " + url;
                RaiseError(new ApplicationException(message, ex));
                statusCode = HttpStatusCode.BadRequest;
                if (response != null) response.Close();
                return null;
            }
        }

        public static String GetImageNameFromSrc(String imgSrc)
        {

            string shortImageName = imgSrc;

            Int32 indexOfDot = shortImageName.IndexOf(".");

            // Virage du dernier "/" si il existe
            if (shortImageName.EndsWith("/"))
                shortImageName = shortImageName.Substring(0, shortImageName.Length - 1);

            // Récupération de l'url sans l'extension
            if (indexOfDot > 0)
                shortImageName = shortImageName.Substring(0, indexOfDot);

            // Index du dernier "/" 
            int lastSlashPosition = shortImageName.LastIndexOf("/");

            // Index du début du nom de l'image
            int indexOfImageName = lastSlashPosition + 1;

            // Nom de l'image
            shortImageName = shortImageName.Substring(indexOfImageName);

            return shortImageName;
        }

        /// <summary>
        /// parse something liek /sc2/en/profile/788178/1/AcerNerchio
        /// </summary>
        /// <param name="hrefValue"></param>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public static Character ParseAHrefLink(String hrefValue, string regionId)
        {

            if (String.IsNullOrEmpty(hrefValue))
                return null;

            if (!hrefValue.Contains("profile"))
                return null;

            // Index du "/" juste avant l'id
            int slashPositionAfterProfile = hrefValue.IndexOf("profile") + ("profile").Length;

            // Index de l'ID
            int idIndexOf = slashPositionAfterProfile + 1;

            // Index du "/" juste aprés l'id
            int slashPositionAfterId = hrefValue.IndexOf("/", idIndexOf);

            // Longueur de la chaine Id
            int idLenghtOf = slashPositionAfterId - idIndexOf;

            // Id en chaine de characatère
            string idStr = hrefValue.Substring(idIndexOf, idLenghtOf);

            // Index de la zone
            string zoneStr = hrefValue.Substring(slashPositionAfterId + 1, 1);
            int zoneId = 1;
            int.TryParse(zoneStr, out zoneId);


            // Virage du dernier "/" si il existe
            if (hrefValue.EndsWith("/"))
                hrefValue = hrefValue.Substring(0, hrefValue.Length - 1);

            // Index du début du nom du personnage (Position du "/" + 1)
            int charIndexOf = hrefValue.LastIndexOf("/") + 1;

            // Longueur du nom du personnage
            int charLenghtOf = hrefValue.Length - charIndexOf;

            // Id en chaine de characatère
            string characterNameOf = hrefValue.Substring(charIndexOf, charLenghtOf);

            int id;

            if (int.TryParse(idStr, out id))
            {
                Character c = new Character
                {
                    CharacterId = id,
                    Name = characterNameOf,
                    RegionId = regionId,
                    ZoneId = zoneId
                };

                return c;
            }

            return null;
        }

        /// <summary>
        /// Gets all characters A href node from document. 
        /// </summary>
        public static List<HtmlNode> GetAllCharactersNodesInDocument(HtmlDocument doc)
        {
         
            var allCharacters = doc.DocumentNode.SelectNodes("//a[contains(@href, 'profile') and contains(@class, 'race')]").Distinct().ToList();

            return allCharacters.Count > 0 ? allCharacters : null;
        }

        

        /// <summary>
        /// Get all characters from all rows of a league page
        /// Updated Patch 1.3.0
        /// </summary>
        public static List<CharacterLeague> GetCharactersLeaguesFromRows(HtmlDocument doc, string regionId, int zoneId, Boolean verify)
        {
            try
            {

                var allRows =
                    doc.DocumentNode.SelectNodes("//table[contains(@class,'data-table')]/tbody/tr[contains(@class,'row')]");

                var columnsHeaderCount =
                    doc.DocumentNode.SelectNodes("//table[contains(@class,'data-table')]/thead/tr/th").Count;


                CharacterProfileServices cps = new CharacterProfileServices(regionId);

                List<CharacterLeague> charactersLeagues = new List<CharacterLeague>();

                // Fore each row, get players (1, 2, 3 or 4)
                // and get Points
                foreach (var row in allRows)
                {
                    var columns = row.SelectNodes("descendant::td");


                    if (columns == null || columns.Count == 0)
                        continue;

                    int points = 0;
                    int victoriesCount = 0;
                    int lossesCount = 0;
                    byte rank = 0;

                    // Get current rank
                    int indexRank = columns.Count == columnsHeaderCount ? 2 : 1;
                    var nodeRankText = columns[indexRank].InnerText;
                    if (nodeRankText.Length > 0)
                    {
                        string numberValue = String.Empty;
                        foreach (char nc in nodeRankText)
                        {
                            if (nc == '0' || nc == '1' || nc == '2' || nc == '3' || nc == '4' || nc == '5' || nc == '6' || nc == '7' || nc == '8' || nc == '9')
                            {
                                numberValue += nc;
                            }
                        }
                        Byte.TryParse(numberValue, out rank);
                    }


                    // Get current players points
                    if (columns.Count > 3)
                    {
                        int indexPoint = columns.Count == columnsHeaderCount ? 4 : 3;
                        var txtPoint = columns[indexPoint].InnerText;
                        Int32.TryParse(txtPoint, out points);

                        int indexVictories = columns.Count == columnsHeaderCount ? 5 : 4;
                        var txtVictoriesCount = columns[indexVictories].InnerText;
                        Int32.TryParse(txtVictoriesCount, out victoriesCount);

                        if (columnsHeaderCount > 6)
                        {
                            int indexLosses = columns.Count == columnsHeaderCount ? 6 : 5;
                            var txtLossesCount = columns[indexLosses].InnerText;
                            Int32.TryParse(txtLossesCount, out lossesCount);
                        }


                    }
                    // Get players
                    var allCharacters = (from op in row.SelectNodes("descendant::a[@href]")
                                         where op.Attributes["href"].Value.Contains("/profile/")
                                         select op).ToList();


                    foreach (var hrefNode in allCharacters)
                    {
                        String href = hrefNode.Attributes["href"].Value;
                        Character c = HtmlHelper.ParseAHrefLink(href, regionId);

                        if (c != null)
                        {

                            HtmlDocument cDoc;
                            if (verify)
                            {
                                bool exist = cps.TryGetCharacterDocument(c, out cDoc, false);
                                if (!exist)
                                    continue;
                            }

                            CharacterLeague cl = new CharacterLeague();

                            if (hrefNode.Attributes.Contains("class"))
                            {
                                switch (hrefNode.Attributes["class"].Value)
                                {
                                    case "race-terran":
                                        cl.MostPlayedRace = RaceType.Terran;
                                        break;
                                    case "race-protoss":
                                        cl.MostPlayedRace = RaceType.Protoss;
                                        break;
                                    case "race-zerg":
                                        cl.MostPlayedRace = RaceType.Zerg;
                                        break;
                                    case "race-random":
                                        cl.MostPlayedRace = RaceType.Random;
                                        break;
                                    default:
                                        cl.MostPlayedRace = RaceType.None;
                                        break;
                                }
                            }

                            cl.RegionId = regionId;
                            cl.Name = c.Name;
                            cl.LastModifiedDate = DateTime.Now;
                            cl.CharacterId = c.CharacterId;
                            cl.LossesCount = lossesCount;
                            cl.VictoriesCount = victoriesCount;
                            cl.Points = points;
                            cl.Rank = rank;
                            cl.ZoneId = zoneId;

                            if (!charactersLeagues.Any((tmp => cl.CharacterId == tmp.CharacterId && cl.RegionId == tmp.RegionId && cl.ZoneId == tmp.ZoneId)))
                                charactersLeagues.Add(cl);

                        }
                    }

                }

                return charactersLeagues;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the seasons snapshot2.
        /// Updated Patch 1.3.0
        /// </summary>
        internal static List<LeagueSummary> GetSeasonsSnapshot2(HtmlNodeCollection seasonSnapshotNodes)
        {
            List<LeagueSummary> leagues = new List<LeagueSummary>();

            foreach (HtmlNode divNode in seasonSnapshotNodes)
            {

                LeagueSummary summary = new LeagueSummary();

                // -----------------------------------------------------------
                // Récupération League Type and League Type Rank span node
                // -----------------------------------------------------------
                var spanNode = divNode.SelectSingleNode("child::node()//a/span[contains(@class, 'badge')]");

                if (spanNode != null)
                {
                    summary.LeagueType = LeagueTypeServices.ParseLeagueTypeContainedInText(spanNode.Attributes["class"].Value);
                    summary.LeagueTopRank = LeagueTypeServices.ParseLeagueTopRankContainedInText(spanNode.Attributes["class"].Value);

                }
                else
                {
                    summary.LeagueType = LeagueType.None;
                    summary.LeagueTopRank = LeagueTopRank.None;
                }

                // -----------------------------------------------------------
                // Récupération de la TeamType
                // -----------------------------------------------------------
                var teamTypeNode = divNode.SelectSingleNode("child::div[@class='division']");

                if (teamTypeNode != null)
                    summary.TeamType = TeamTypeServices.ParseTeamTypeFromShortText(teamTypeNode.InnerText);

                // -----------------------------------------------------------
                // Récupération du Nombre de matchs
                // -----------------------------------------------------------
                var teamMatchesNode = divNode.SelectSingleNode("child::node()//div[@class='graph-bars primary']/div[@class='graph-bar']/span[@class='totals']");
                int matchesCount = 0;
                if (teamMatchesNode != null)
                {
                    string teamMatchesCountString = teamMatchesNode.InnerText.Trim();

                    // Récupération de la première chaine (arrêté aprés l'espace)
                    teamMatchesCountString = teamMatchesCountString.Substring(0, teamMatchesCountString.IndexOf(" "));

                    Tools.TryParse(teamMatchesCountString, out matchesCount);

                }
                summary.MatchesCount = matchesCount;

                // -----------------------------------------------------------
                // Récupération du Nombre de victoires
                // -----------------------------------------------------------
                var teamVictoriesNode = divNode.SelectSingleNode("child::node()//div[@class='graph-bars secondary']//div[@class='graph-bar']/span[@class='totals']");
                int victoriesCount = 0;
                if (teamVictoriesNode != null)
                {
                    string teamVictoriesCountString = teamVictoriesNode.InnerText.Trim();

                    // Récupération de la première chaine (arrêté aprés l'espace)
                    teamVictoriesCountString = teamVictoriesCountString.Substring(0, teamVictoriesCountString.IndexOf(" "));

                    Tools.TryParse(teamVictoriesCountString, out victoriesCount);

                }
                summary.VictoriesCount = victoriesCount;

                // No double bar, so 1st bar is victories count
                if (teamVictoriesNode == null && victoriesCount == 0 && matchesCount > 0)
                    summary.VictoriesCount = matchesCount;


                // -----------------------------------------------------------
                // Récupération de la division et du meilleur Rang
                // -----------------------------------------------------------
                var divIdName = "best-team-";
                if (summary.TeamType == TeamType.OneVOne)
                    divIdName += "1";
                else if (summary.TeamType == TeamType.TwoVTwo)
                    divIdName += "2";
                else if (summary.TeamType == TeamType.ThreeVThree)
                    divIdName += "3";
                else if (summary.TeamType == TeamType.FourVFour)
                    divIdName += "4";

                var div2IdName = "badge-";
                if (summary.LeagueType == LeagueType.Bronze)
                    div2IdName += "bronze";
                else if (summary.LeagueType == LeagueType.Silver)
                    div2IdName += "silver";
                else if (summary.LeagueType == LeagueType.Gold)
                    div2IdName += "gold";
                else if (summary.LeagueType == LeagueType.Platinum)
                    div2IdName += "platinum";
                else if (summary.LeagueType == LeagueType.Diamond)
                    div2IdName += "diamond";
                else if (summary.LeagueType == LeagueType.Master)
                    div2IdName += "master";
                else if (summary.LeagueType == LeagueType.None)
                    div2IdName += "none";


                var divBestDivisions = divNode.SelectSingleNode("child::node()//div[@id='" + divIdName + "']/div[last()]");

                if (divBestDivisions != null)
                {
                    // Récupération du nom de la division
                    var divisionText = divBestDivisions.ChildNodes[2].InnerText.Trim();
                    divisionText = divisionText.Replace("\"", "").Trim();

                    // Récupération du rang
                    var bestRankText = divBestDivisions.ChildNodes[6].InnerText.Trim();
                    bestRankText = bestRankText.Replace("\"", "").Trim();

                    int bestRank = 0;

                    Tools.TryParse(bestRankText, out bestRank);

                    summary.BestRank = bestRank;

                    summary.Division = divisionText;
                }

                leagues.Add(summary);

            }

            return leagues;
        }


        /// <summary>
        /// Parses the A href league link.
        /// Something like /sc2/fr/profile/241327/1/DeltaKosh/ladder/1620#current-rank
        /// Or Something like /sc2/fr/profile/241327/1/DeltaKosh/ladder/1620
        /// Returns the League Id
        /// </summary>
        public static int? ParseAHrefLeagueLink(String hrefValue)
        {
            if (String.IsNullOrEmpty(hrefValue))
                return null;

            hrefValue = hrefValue.Replace("#current-rank", "");

            // Index du "/" juste avant l'id
            int slashPositionBeforeId = hrefValue.LastIndexOf("/");

            // Index de l'ID
            int idIndexOF = slashPositionBeforeId + 1;

            // Longueur de la chaine Id
            int idLenghtOF = hrefValue.Length - idIndexOF;

            // Id en chaine de characatère
            string idStr = hrefValue.Substring(idIndexOF, idLenghtOF);


            int id = 0;

            if (Int32.TryParse(idStr, out id))
                return id;

            return null;
        }



        private static void RaiseError(ApplicationException ex)
        {
            if (ErrorOccuredEvent != null)
                ErrorOccuredEvent(null, ex);
        }

    }
}



//if (r.Next() == 1)
//    req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.3 (KHTML, like Gecko) Chrome/6.0.472.62 Safari/534.3";

//else
//    req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; MS-RTC LM 8; Zune 4.7)";

//req.Method = "GET";
//req.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/x-shockwave-flash, */*";
//req.Headers.Add("Accept-Language", "fr-FR,en-US;q=0.5");


//if (response.StatusCode == HttpStatusCode.Found)
//{

//    var cc = response.Cookies;


//    req = (HttpWebRequest)HttpWebRequest.Create(response.Headers["Location"].ToString());
//    req.AllowAutoRedirect = false;
//    req.AutomaticDecompression = DecompressionMethods.GZip;
//    req.KeepAlive = false;
//    req.ServicePoint.ConnectionLimit = 6;
//    req.CookieContainer = new CookieContainer();
//    CookieCollection cookies = req.CookieContainer.GetCookies(req.RequestUri);
//    response = (HttpWebResponse)req.GetResponse();

//    req = (HttpWebRequest)HttpWebRequest.Create(url);
//    req.AllowAutoRedirect = false;
//    req.AutomaticDecompression = DecompressionMethods.GZip;
//    req.KeepAlive = false;
//    req.ServicePoint.ConnectionLimit = 6;
//    req.CookieContainer = new CookieContainer();

//    foreach (Cookie c in response.Cookies)
//        req.CookieContainer.Add(c);

//    CookieCollection cookies2 = req.CookieContainer.GetCookies(req.RequestUri);
//    response = (HttpWebResponse)req.GetResponse();
//}