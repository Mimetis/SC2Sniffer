using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SC2MiM.Common.Entities;
using SC2MiM.Common.Helpers;

namespace SC2MiM.Common.Services
{
    public class CharacterProfileServices
    {
        private object locker = new object();

        public String RegionId { get; private set; }
        public String CultureId { get; private set; }
        public String ProfileUrl { get; private set; }
        public String RewardUrl { get; private set; }

        public event EventHandler<ApplicationException> ErrorOccuredEvent;
        public event EventHandler<String> EventOccured;


        public CharacterProfileServices(string regionId, string cultureId)
        {
            this.RegionId = regionId;
            this.CultureId = cultureId;

            this.ProfileUrl = String.Format(Urls.ProfileUrl, this.RegionId, this.CultureId, "{0}", "{1}", "{2}");
            this.RewardUrl = String.Format(Urls.RewardUrl, this.RegionId, this.CultureId, "{0}", "{1}", "{2}");

        }
        public CharacterProfileServices(string regionId)
        {
            this.RegionId = regionId;
            this.CultureId = CultureHelper.GetDefaultCultureId(regionId);

            this.ProfileUrl = String.Format(Urls.ProfileUrl, this.RegionId, this.CultureId, "{0}", "{1}", "{2}");
            this.RewardUrl = String.Format(Urls.RewardUrl, this.RegionId, this.CultureId, "{0}", "{1}", "{2}");

        }


        public bool TryGetCharacterDocument(Character c, out HtmlDocument doc, bool noCache)
        {
            String url = String.Format(this.ProfileUrl, c.CharacterId, c.ZoneId, c.Name);

            HttpStatusCode statusCode;
            doc = HtmlHelper.GetDocument(url, out statusCode, noCache);

            if (statusCode == HttpStatusCode.NotFound || statusCode == HttpStatusCode.BadRequest)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the character summary.
        /// Updated Patch 1.3.0
        /// </summary>
        public bool GetCharacterSummary(Character character, bool noCache)
        {
            String url = String.Format(this.ProfileUrl, character.CharacterId, character.ZoneId, character.Name);

            try
            {
                HttpStatusCode statusCode;
                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode, noCache);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {
                    String message = "[CharacterProfileServices].[GetCharacterSummary] : HtmlDocument Is null. Char : " + character.Name + " (" + character.RegionId + "-" + character.CharacterId + ")";
                    RaiseError(new ApplicationException(message));

                    return false;
                }


                // -----------------------------------------------------------
                // Récupération du character Code Character
                // -----------------------------------------------------------
                var characterCodeNumberNode = doc.DocumentNode.SelectSingleNode("//div[@id='profile-wrapper']/div[@id='profile-header']/h2/a/span");
                if (characterCodeNumberNode != null)
                {
                    var characterCodeNumberString = characterCodeNumberNode.InnerText;
                    if (characterCodeNumberString.StartsWith("#"))
                        characterCodeNumberString = characterCodeNumberString.Substring(1);

                    int characterCodeNumber;
                    int.TryParse(characterCodeNumberString, out characterCodeNumber);

                    character.Code = characterCodeNumber.ToString();
                }

                // -----------------------------------------------------------
                // Récupération du nombre de points de hauts faits
                // -----------------------------------------------------------
                var achievementsNumberNode = doc.DocumentNode.SelectSingleNode("//div[@id='profile-wrapper']/div[@id='profile-header']/h3");
                if (achievementsNumberNode != null)
                {
                    int achievementsPoints;
                    int.TryParse(achievementsNumberNode.InnerText, out achievementsPoints);

                    character.AchievementPoints = achievementsPoints;
                }

                // -----------------------------------------------------------
                // Récupération des victoires en leagues
                // -----------------------------------------------------------
                var vitoriesDuringLeaguesNode = doc.DocumentNode.SelectSingleNode("//div[@class='module-bot']/div[@class='module-right']/div/h2");
                if (vitoriesDuringLeaguesNode != null)
                {
                    int victoriesDuringLeagues;
                    int.TryParse(vitoriesDuringLeaguesNode.InnerText, out victoriesDuringLeagues);

                    character.LeaguesVictoriesCount = victoriesDuringLeagues;
                }

                // -----------------------------------------------------------
                // Récupération du nombre de match en leagues
                // -----------------------------------------------------------
                var mactchesCountDuringLeaguesNode = doc.DocumentNode.SelectSingleNode("//div[@class='module-bot']/div[@class='module-right']/div/ul/li/span");
                if (mactchesCountDuringLeaguesNode != null)
                {
                    int matchesCountDuringLeagues;
                    int.TryParse(mactchesCountDuringLeaguesNode.InnerText, out matchesCountDuringLeagues);

                    character.LeaguesMatchesCount = matchesCountDuringLeagues;
                }


                // -----------------------------------------------------------
                // Récupération des 4 leagues (1v1, 2v2, 3v3, 4v4)
                // -----------------------------------------------------------

                var seasonSnapshotNodes = doc.DocumentNode.SelectNodes("//div[@id='season-snapshot']/div[contains(@class, 'body')]/div[contains(@class, 'snapshot')]");

                if (seasonSnapshotNodes != null && seasonSnapshotNodes.Count > 0)
                {
                    // Récupération des 4 summaries (1, 2, 3 et 4c4)
                    List<LeagueSummary> summaries = HtmlHelper.GetSeasonsSnapshot2(seasonSnapshotNodes);
                    character.LeaguesSummary = summaries;
                    character.LeagueOneVOneSummary = summaries.FirstOrDefault((ls) => ls.TeamType == TeamType.OneVOne);
                    character.LeagueTwoVTwoSummary = summaries.FirstOrDefault((ls) => ls.TeamType == TeamType.TwoVTwo);
                    character.LeagueThreeVThreeSummary = summaries.FirstOrDefault((ls) => ls.TeamType == TeamType.ThreeVThree);
                    character.LeagueFourVFourSummary = summaries.FirstOrDefault((ls) => ls.TeamType == TeamType.FourVFour);

                }
                else
                {
                    character.LeaguesSummary = new List<LeagueSummary>();
                }

                // -----------------------------------------------------------
                // Récupération de la race préférée
                // -----------------------------------------------------------
                if (doc.DocumentNode.SelectNodes("//div[@class='module-body snapshot-protoss']") != null)
                    character.MostPlayedRace = RaceType.Protoss;
                else if (doc.DocumentNode.SelectNodes("//div[@class='module-body snapshot-terran']") != null)
                    character.MostPlayedRace = RaceType.Terran;
                else if (doc.DocumentNode.SelectNodes("//div[@class='module-body snapshot-zerg']") != null)
                    character.MostPlayedRace = RaceType.Zerg;
                else if (doc.DocumentNode.SelectNodes("//div[@class='module-body snapshot-random']") != null)
                    character.MostPlayedRace = RaceType.Random;
                else
                    character.MostPlayedRace = RaceType.None;

                // -----------------------------------------------------------
                // Récupération le niveau d'achèvement de la campagne
                // -----------------------------------------------------------

                var campagnNode = doc.DocumentNode.SelectSingleNode("//div[@id='career-stats']/div[contains(@class, 'module-body')]/h4[last()]");
                if (campagnNode != null)
                    character.CampaignBadge = campagnNode.InnerText.Trim();

                // -----------------------------------------------------------
                // Récupération du portrait
                // -----------------------------------------------------------

                var portraitNode = doc.DocumentNode.SelectSingleNode("//div[@id='profile-header']/div[@id='portrait']/span[@style]");
                if (portraitNode != null)
                {
                    var style = portraitNode.Attributes["style"].Value;

                    var pos1 = style.IndexOf("url('") + 5;
                    var pos2 = style.IndexOf("')", pos1);
                    if (pos1 >= 0 && pos2 >= 0)
                    {
                        var urlPortrait = style.Substring(pos1, pos2 - pos1);
                        character.PortraitUrl = urlPortrait;

                        var pos1jpg = urlPortrait.LastIndexOf("/") + 1;
                        var pos2jpg = urlPortrait.IndexOf("jpg") + 3;
                        if (pos1jpg >= 0 && pos2jpg >= 0)
                        {
                            var imagePortrait = urlPortrait.Substring(pos1jpg, pos2jpg - pos1jpg);
                            character.PortraitJpgName = imagePortrait;
                        }
                        else
                        {
                            Debug.WriteLine("style : " + style + " pos1jpg : " + pos1jpg + " - pos2jpg : " + pos2jpg);
                        }

                        var styleCoords = style.Substring(pos2);

                        // position X
                        var posX1 = styleCoords.IndexOf("-") + 1;
                        var posX2 = styleCoords.IndexOf("px", posX1);
                        if (posX1 >= 0 && posX2 >= 0)
                        {
                            var posX = styleCoords.Substring(posX1, posX2 - posX1);
                            int intPosX;
                            int.TryParse(posX, out intPosX);
                            character.PortraitPositionX = intPosX;

                            // position Y
                            styleCoords = styleCoords.Substring(posX2);
                            var posY1 = styleCoords.IndexOf("-") + 1;
                            var posY2 = styleCoords.IndexOf("px", posY1);

                            if (posY1 >= 0 && posY2 >= 0)
                            {
                                var posY = styleCoords.Substring(posY1, posY2 - posY1);
                                int intPosY;
                                int.TryParse(posY, out intPosY);
                                character.PortraitPositionY = intPosY;

                            }
                            else
                            {
                                Debug.WriteLine("style : " + style + " posY1 : " + posY1 + " - posY2 : " + posY2);
                            }

                        }
                        else
                        {
                            Debug.WriteLine("style : " + style + " posX1 : " + posX1 + " - posX2 : " + posX2);
                        }

                    }
                    else
                    {
                        Debug.WriteLine("style : " + style + " pos1 : " + pos1 + " - pos2 : " + pos2);
                    }



                }


            }
            catch (Exception ex)
            {
                String message = "[CharacterProfileServices].[GetCharacterSummary] : Character : " + character.Name + " (" + character.RegionId + "-" + character.CharacterId + ")";

                ApplicationException ae = new ApplicationException(message, ex);
                RaiseError(ae);
                return false;
            }


            return true;
        }


        /// <summary>
        /// Gets the character rewards.
        /// Updated Patch 1.3.0
        /// </summary>
        public List<CharacterReward> GetCharacterRewards(Character character)
        {
            String url = String.Format(this.RewardUrl, character.CharacterId, character.ZoneId, character.Name);
            try
            {
                HttpStatusCode statusCode;
                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode);

                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {

                    String message = "[CharacterProfileServices].[GetCharacterRewards] : HtmlDocument Is null. Character : " + character.Name + " (" + character.RegionId + "-" + character.CharacterId + ")";
                    RaiseError(new ApplicationException(message));
                    return null;
                }

                // -----------------------------------------------------------
                // Récupération de tous les nodes des rewards gagnés
                // -----------------------------------------------------------
                var rewardsNode = doc.DocumentNode.SelectNodes("//div[@class='reward-tile clickable earned']");

                if (rewardsNode == null || rewardsNode.Count == 0)
                    return null;

                List<CharacterReward> rewards = new List<CharacterReward>();

                foreach (HtmlNode rewardNode in rewardsNode)
                {

                    CharacterReward reward = new CharacterReward();

                    reward.CharacterId = character.CharacterId;
                    reward.RegionId = character.RegionId;

                    string id = rewardNode.Attributes["id"].Value.Trim();
                    reward.RewardId = id;

                    var rewardNameNode = rewardNode.SelectSingleNode("child::node()//div[@class='reward-tooltip']/div[@class='tooltip-title']");
                    reward.Name = rewardNameNode.InnerText.Trim();

                    var rewardDescription = rewardNode.SelectSingleNode("child::node()//div[@class='reward-tooltip']/strong");
                    reward.Description = rewardDescription.InnerText.Trim();

                    var rewardFullDescription = rewardNode.SelectSingleNode("child::node()//div[@class='reward-tooltip']/br").NextSibling;
                    reward.FullDescription = rewardFullDescription.InnerText.Trim();

                    reward.LastModifiedDate = DateTime.Now;

                    rewards.Add(reward);
                }

                return rewards;
            }
            catch (Exception ex)
            {
                String message = "[CharacterProfileServices].[GetCharacterRewards] : Character : " + character.Name + " (" + character.RegionId + "-" + character.CharacterId + ")";

                ApplicationException ae = new ApplicationException(message, ex);
                RaiseError(ae);
                return null;
            }
        }


        /// <summary>
        /// Parcours récursif
        /// Ordre de sortie : Plus aucun joueur à traiter
        /// </summary>
        public void RecurseCharacter(Character c,
                                      Dictionary<Int32, Character> alreadyProcessedCharacters,
                                      Dictionary<Int32, League> alreadyProcessedLeagues,
                                      int maxLevelDepth,
                                      ref int levelDepth, bool verifyIfCharacterReallyExist)
        {
            try
            {

                if (levelDepth > maxLevelDepth)
                    return;

                int currentDepth = levelDepth + 1;

                // Un joueur "traité a été parcouru intégralement
                // Pas besoin de retraiter
                if (alreadyProcessedCharacters.ContainsKey(c.CharacterId))
                    return;

                // Je remplis ses données
                c.Leagues = new List<League>();

                // Récupération du document contenant les Leagues du joueur
                // + Son Identifiant, pour éviter de charger le document une fois de trop 
                int currentLeagueId;
                CharacterLeaguesServices cls = new CharacterLeaguesServices(c.RegionId);

                var leagueDoc = cls.GetCharacterLadderLeagueDocument(c, out currentLeagueId);

                if (leagueDoc != null)
                {
                    // Récupération de toutes les leagues Ids
                    var allLeaguesId = cls.GetCharactersLeaguesIds(leagueDoc);

                    if (allLeaguesId != null)
                    {
                        // Parcours de toutes les leagues et récupération de la league complète
                        foreach (var leagueId in allLeaguesId)
                        {
                            // Si je ne l'ai pas déjà
                            if (!alreadyProcessedLeagues.ContainsKey(leagueId))
                            {
                                try
                                {

                                    HtmlDocument specificLeagueDoc;

                                    // Récupération du document qui servira a créer la league ET à remplir ses opposants
                                    if (leagueId == currentLeagueId)
                                        specificLeagueDoc = leagueDoc;
                                    else
                                        specificLeagueDoc = cls.GetCharacterLadderLeagueDocument(c, leagueId);

                                    // Get all players in the document
                                    var allPlayersInDoc = CharacterProfileServices.ProcessHtmlPage(specificLeagueDoc, this.RegionId);

                                    if (allPlayersInDoc != null && allPlayersInDoc.Count > 0)
                                    {
                                        foreach (var player in allPlayersInDoc)
                                        {
                                            // Double lock
                                            if (!alreadyProcessedCharacters.ContainsKey(player.CharacterId))
                                            {
                                                lock (locker)
                                                {
                                                    if (!alreadyProcessedCharacters.ContainsKey(player.CharacterId))
                                                        alreadyProcessedCharacters.Add(player.CharacterId, player);
                                                }
                                            }
                                        }
                                    }

                                    // Ajout de la league à la collection des leagues traitées
                                    League playerLeague = new League { LeagueId = leagueId, RegionId = this.RegionId, ZoneId = c.ZoneId };
                                    lock (locker)
                                    {
                                        if (!alreadyProcessedLeagues.ContainsKey(leagueId))
                                            alreadyProcessedLeagues.Add(leagueId, playerLeague);
                                    }


                                }
                                catch (Exception ex)
                                {
                                    String message = "[CharacterProfileServices].[RecurseCharacter] : error : " + ex.Message;
                                    RaiseError(new ApplicationException(message, ex));

                                    if (ex.InnerException != null)
                                        RaiseError(
                                            new ApplicationException(
                                                "[CharacterProfileServices].[RecurseCharacter] : InnerException : " +
                                                ex.InnerException.Message));
                                }

                            }
                            else
                            {
                                lock (locker)
                                {
                                    if (!c.Leagues.Contains(alreadyProcessedLeagues[leagueId]))
                                        c.Leagues.Add(alreadyProcessedLeagues[leagueId]);
                                }
                            }
                        }
                    }
                }

                foreach (League l in c.Leagues)
                {

                    //foreach (var cl in l.Characters)
                    Parallel.ForEach(l.Characters, (cl, pls) =>
                    {
                        Character cha = new Character() { CharacterId = cl.CharacterId, Name = cl.Name, RegionId = cl.RegionId, ZoneId = cl.ZoneId };

                        RecurseCharacter(cha, alreadyProcessedCharacters, alreadyProcessedLeagues,
                                         maxLevelDepth, ref currentDepth, verifyIfCharacterReallyExist);
                    });
                }

            }
            catch (Exception exx)
            {
                String message = "[CharacterProfileServices].[ProcessACharacter] : error : " + exx.Message;
                RaiseError(new ApplicationException(message, exx));

            }

        }




        /// <summary>
        /// get all chars in a page
        /// </summary>
        public static List<Character> ProcessHtmlPage(String url, String regionId)
        {
            try
            {
                HttpStatusCode statusCode;

                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                return ProcessHtmlPage(doc, regionId);

            }
            catch (Exception)
            {

                return null;
            }

        }

        public static List<Character> ProcessHtmlPage(HtmlDocument doc, String regionId)
        {
            try
            {
                // Get all node from a document
                List<HtmlNode> nodesChars = HtmlHelper.GetAllCharactersNodesInDocument(doc);
                List<Character> characters = new List<Character>();

                foreach (var node in nodesChars)
                {
                    var player = HtmlHelper.ParseAHrefLink(node.Attributes["href"].Value, regionId);

                    if (node.Attributes.Contains("class") && node.Attributes["class"].Value.Contains("race"))
                        player.MostPlayedRace = RaceTypeServices.ParseRaceTypeContainedInText(node.Attributes["class"].Value);

                    characters.Add(player);
                }

                return characters;
            }
            catch (Exception)
            {

                return null;
            }

        }


        public List<Character> Merge(IEnumerable<Character> oldList, IDictionary<String, Character> characters)
        {
            var newList = new List<Character>();

            try
            {

                foreach (var c in oldList)
                {
                    if (!characters.ContainsKey(String.Format("{0}/{1}/{2}", c.CharacterId, c.ZoneId, c.Name)))
                        newList.Add(c);
                }

                return newList;
            }
            catch (Exception ex)
            {
                String message = "[Extractor].[Merge] : Error during Merge. ";
                RaiseError(new ApplicationException(message, ex));
                return newList;
            }

        }


        private void RaiseError(ApplicationException ex)
        {
            if (ErrorOccuredEvent != null)
                ErrorOccuredEvent(this, ex);
        }
        private void RaiseMessage(String message)
        {
            if (EventOccured != null)
                EventOccured(this, message);
        }



    }
}
