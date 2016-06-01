using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Threading.Tasks;
using SC2MiM.Common.Entities;
using SC2MiM.Common.Helpers;

namespace SC2MiM.Common.Services
{

    /// <summary>
    /// This class gets all leagues opponents from a character
    /// </summary>
    public class CharacterLeaguesServices
    {
        public String RegionId { get; private set; }
        public String CultureId { get; private set; }
        public String GlobalLadderLeaguesUrl { get; private set; }
        public String LadderLeaguesUrl { get; private set; }
        private object locker = new object();

        public event EventHandler<ApplicationException> ErrorOccuredEvent;


        public CharacterLeaguesServices(String regionId)
        {
            this.RegionId = regionId;
            this.CultureId = CultureHelper.DefaultCultureId;

            this.GlobalLadderLeaguesUrl = String.Format(Urls.GlobalLadderLeaguesUrl, this.RegionId, this.CultureId, "{0}", "{1}", "{2}");
            this.LadderLeaguesUrl = String.Format(Urls.LadderLeaguesUrl, this.RegionId, this.CultureId, "{0}", "{1}", "{2}", "{3}");
        }


        /// <summary>
        /// Gets the character leagues.
        /// Updated Patch 1.3.0
        /// </summary>
        public List<League> GetCharacterLeagues(Character c, Dictionary<Int32, League> alreadyProcessedLeagues, bool verifyIfCharacterReallyExist)
        {
            // Récupération du document contenant les Leagues du joueur
            // + Son Identifiant, pour éviter de charger le document une fois de trop 
            int currentLeagueId;
            var leagueDoc = this.GetCharacterLadderLeagueDocument(c, out currentLeagueId);

            List<League> lstLeagues = null;

            if (leagueDoc != null)
            {
                // Récupération de toutes les leagues Ids
                var allLeaguesId = this.GetCharactersLeaguesIds(leagueDoc);
                if (allLeaguesId != null)
                {
                    lstLeagues = new List<League>();

                    // Parcours de toutes les leagues et récupération de la league complète
                    //foreach (var leagueId in allLeaguesId)
                    Parallel.ForEach(allLeaguesId, leagueId =>
                    {
                        // Si je ne l'ai pas déjà
                        if (alreadyProcessedLeagues == null || !alreadyProcessedLeagues.ContainsKey(leagueId))
                        {
                            try
                            {

                                HtmlDocument specificLeagueDoc;

                                // Récupération du document qui servira a créer la league ET à remplir ses opposants
                                if (leagueId == currentLeagueId)
                                    specificLeagueDoc = leagueDoc;
                                else
                                    specificLeagueDoc = this.GetCharacterLadderLeagueDocument(c, leagueId);

                                //Récupération de la league de ce joueur
                                League playerLeague = this.GetLeague(specificLeagueDoc, leagueId, c, verifyIfCharacterReallyExist);

                                // Ajout de la league à la collection des leagues traitées
                                lock (locker)
                                {
                                    if (alreadyProcessedLeagues != null)
                                        if (!alreadyProcessedLeagues.ContainsKey(leagueId))
                                            alreadyProcessedLeagues.Add(leagueId, playerLeague);

                                    lstLeagues.Add(playerLeague);
                                }
                            }
                            catch (Exception ex)
                            {
                                String message = "[Extractor].[ProcessLeagues] : error : " + ex.Message;
                                RaiseError(new ApplicationException(message, ex));
                            }

                        }
                        // Try to not overhead CPU
                        System.Threading.Thread.Sleep(10);
                        //}
                    });
                }
            }

            return lstLeagues;
        }


        /// <summary>
        /// Gets the globale league doc.
        /// Something like http://eu.battle.net/sc2/fr/profile/605538/1/Mimetis/ladder/leagues#current-rank
        /// Updated 2015-07-29
        /// </summary>
        public HtmlDocument GetCharacterLadderLeagueDocument(Character c, out int currentLeagueId)
        {
            String url = String.Format(this.GlobalLadderLeaguesUrl, c.CharacterId, c.ZoneId, c.Name);
            currentLeagueId = 0;

            try
            {
                HttpStatusCode statusCode;
                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {
                    String message = "[CharacterProfileServices].[GetCharacterLadderLeagueDocument] : HtmlDocument Is null. " +
                        "Character : " + c.Name + " (" + this.RegionId + "-" + c.CharacterId + ")";

                    RaiseError(new ApplicationException(message));
                    return null;

                }

                // Récupère l'identifiant de la league en cours
                var cl = doc.DocumentNode.SelectSingleNode("//ul[@id='profile-menu']/li[contains(@class, 'active') and contains(@class, 'submenu')]/a[@href]");
                if (cl != null)
                {
                    Int32? lid = HtmlHelper.ParseAHrefLeagueLink(cl.Attributes["href"].Value);
                    if (lid.HasValue)
                        currentLeagueId = lid.Value;
                }
                return doc;
            }
            catch (Exception ex)
            {

                String message = "[CharacterLeaguesServices].[GetCharacterLadderLeagueDocument] : Character : "
                    + c.Name + " (" + this.RegionId + "-" + c.CharacterId + ")";
                RaiseError(new ApplicationException(message, ex));

                return null;
            }
        }

        /// <summary>
        /// Gets specific league doc.
        /// Something like http://eu.battle.net/sc2/en/profile/605538/1/Mimetis/ladder/4488#current-rank
        /// Updated 2015-07-29
        /// </summary>
        public HtmlDocument GetCharacterLadderLeagueDocument(Character c, Int32 leagueId)
        {
            String url = String.Format(this.LadderLeaguesUrl, c.CharacterId, c.ZoneId, c.Name,  leagueId);
            try
            {
                HttpStatusCode statusCode;

                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {
                    String message = "[CharacterProfileServices].[GetCharacterLadderLeagueDocument] : HtmlDocument Is null. " + 
                        "Character : " + c.Name + " (" + this.RegionId + "-" + c.CharacterId + ")";
                    RaiseError(new ApplicationException(message));
                    return null;

                }
                return doc;
            }
            catch (Exception ex)
            {

                String message = "[CharacterLeaguesServices].[GetCharacterLadderLeagueDocument] : Character : " + 
                    c.Name + " (" + this.RegionId + "-" + c.CharacterId + ")";
                RaiseError(new ApplicationException(message, ex));

                return null;
            }
        }


        /// <summary>
        /// Get all leagues Id from a global league character document
        /// Updated 2015-07-29
        /// </summary>
        internal List<Int32> GetCharactersLeaguesIds(HtmlDocument doc)
        {

            List<Int32> leagueIds = new List<Int32>();
            try
            {
                // Récupération de toutes les leagues du joueur
                var allLeagues = doc.DocumentNode.SelectNodes("//ul[@id='profile-menu']/li/a[contains(@href, '#current-rank')]");

                if (allLeagues == null)
                    return null;

                List<String> allLeaguesUrl = new List<string>();

                foreach (var leagueNode in allLeagues)
                {
                    var tmpLeagueUrl = leagueNode.Attributes["href"].Value.Replace("#current-rank", "");

                    if (!tmpLeagueUrl.EndsWith("/ladder/"))
                        allLeaguesUrl.Add(tmpLeagueUrl);
                }

                foreach (var leagueUrl in allLeaguesUrl)
                {
                    int? id = HtmlHelper.ParseAHrefLeagueLink(leagueUrl);

                    if (id.HasValue)
                        leagueIds.Add(id.Value);
                }

                return leagueIds;
            }
            catch (Exception ex)
            {
                RaiseError(new ApplicationException(ex.Message));
                return null;
            }

        }


        /// <summary>
        /// Crée une league complète
        /// Updated Patch 1.3.0
        /// </summary>
        internal League GetLeague(HtmlDocument doc, int leagueId, Character playerReferer, bool verifyIfCharacterReallyExist)
        {
            try
            {
                // --------------------------------------------------------------------------------
                // Get Division Name
                // --------------------------------------------------------------------------------

                var dataTitle = doc.DocumentNode.SelectSingleNode("//div[@class='data-title']/div/h3");

                if (dataTitle != null && dataTitle.ChildNodes.Count > 2)
                {
                    League currentLeague = new League();

                    // <h3>
                    //    Season 1 <span>-</span> 
                    //    1v1 Master
                    //   <span>Division Aldaris Sigma</span>
                    // </h3>

                    // Cas particulier : GrandMaster

                    // <h3>
                    //    Season 1 <span>-</span> 
                    //    1v1 GrandMaster
                    // </h3>

                    var dn = dataTitle.ChildNodes[dataTitle.ChildNodes.Count - 1].InnerText.Trim();

                    currentLeague.DivisionName = dn;

                    var firstChildText = dataTitle.ChildNodes[2].InnerText.Trim();

                    // Capable de récupérer le LeagueType
                    currentLeague.LeagueType = LeagueTypeServices.ParseLeagueTypeContainedInText(firstChildText);

                    // Récupérer si c'est random ou non
                    currentLeague.IsRandom = LeagueTypeServices.ParseLeagueTypeIsRandomContainedInText(firstChildText);

                    // Récupérer le teamtype
                    currentLeague.TeamType = TeamTypeServices.ParseTeamTypeContainedInText(firstChildText);

                    // --------------------------------------------------------------------------------
                    // Get League Players
                    // --------------------------------------------------------------------------------

                    // Récupération de tous les Character League de chaque ligne
                    currentLeague.Characters = HtmlHelper.GetCharactersLeaguesFromRows(doc, playerReferer.RegionId, playerReferer.ZoneId, verifyIfCharacterReallyExist);
                    currentLeague.LeagueId = leagueId;
                    currentLeague.ZoneId = playerReferer.ZoneId;
                    currentLeague.RegionId = playerReferer.RegionId;
                    currentLeague.LastModifiedDateTime = DateTime.Now;

                    return currentLeague;
                }

                return null;
            }
            catch (Exception ex)
            {
                RaiseError(new ApplicationException(ex.Message));

                if (ex.InnerException != null)
                    RaiseError(new ApplicationException(ex.InnerException.Message));
                return null;
            }


        }


        private void RaiseError(ApplicationException ex)
        {
            if (ErrorOccuredEvent != null)
                ErrorOccuredEvent(this, ex);
        }
    }
}
