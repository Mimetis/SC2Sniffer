using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SC2MiM.Common.Entities;
using SC2MiM.Common.Helpers;

namespace SC2MiM.Common.Services
{
    public class GrandMasterServices
    {
        public String RegionId { get; private set; }
        public String CultureId { get; private set; }
        public String GrandMasterUrl { get; private set; }
        private object locker = new object();

        public event EventHandler<ApplicationException> ErrorOccuredEvent;


        
        public GrandMasterServices(string regionId, String cultureId)
        {
            this.RegionId = regionId;
            this.CultureId = cultureId;


            this.GrandMasterUrl = String.Format(Urls.GrandMasterUrl, this.RegionId, this.CultureId);
        }

        public GrandMasterServices(string regionId)
        {
            this.RegionId = regionId;
            this.CultureId = CultureHelper.DefaultCultureId;

            this.GrandMasterUrl = String.Format(Urls.GrandMasterUrl, this.RegionId, this.CultureId);

        }

        public GrandMasterServices(String url, String regionId, String cultureId)
        {
            this.RegionId = regionId;
            this.CultureId = CultureHelper.DefaultCultureId;

            this.GrandMasterUrl = url;

        }



        /// <summary>
        /// Gets the globale league doc.
        /// Something like http://eu.battle.net/sc2/fr/ladder/grandmaster/heart-of-the-swarm
        /// Updated Patch 1.3.0
        /// </summary>
        private HtmlDocument GetGrandMasterDocument()
        {
     
            try
            {
                HttpStatusCode statusCode;
                HtmlDocument doc = HtmlHelper.GetDocument(this.GrandMasterUrl, out statusCode);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {
                    String message = "[GrandMasterServices].[GetGrandMasterDocument] : HtmlDocument Is null. Url : " + this.GrandMasterUrl + ")";
                    RaiseError(new ApplicationException(message));
                    return null;

                }
                return doc;
            }
            catch (Exception ex)
            {

                String message = "[GrandMasterServices].[GetGrandMasterDocument] : Error on Url : " + this.GrandMasterUrl + ")";
                RaiseError(new ApplicationException(message, ex));

                return null;
            }
        }

        public List<CharacterLeague> GetGrandMasters()
        {
            List<CharacterLeague> grandMasters = new List<CharacterLeague>();
            try
            {


                var doc = this.GetGrandMasterDocument();

                // Récupération de toutes les leagues du joueur
                var allGrandMasters = doc.DocumentNode.SelectNodes("//div[@id='ladder']/table/tbody/tr");

                if (allGrandMasters == null)
                    return grandMasters;

                foreach (var grandMaster in allGrandMasters)
                {
                    // rank
                    Byte rank = 200;
                    var rankNode = grandMaster.SelectSingleNode("descendant::td[@data-raw]");
                    if (rankNode != null)
                        Byte.TryParse(rankNode.Attributes["data-raw"].Value, out rank);
                    
                    // name
                    string name = String.Empty ;
                    var nameNode = grandMaster.SelectSingleNode("descendant::div[@class='tooltip-title']");
                    if (nameNode != null)
                        name = nameNode.InnerText.Trim();

                    // Nombre de noeuds TD de premier niveau
                    var nodes = grandMaster.SelectNodes("child::td");
         
                    // losses
                    int losses = 0;
                    var lossesNode = nodes[nodes.Count - 1];
                    if (lossesNode != null)
                        int.TryParse(lossesNode.InnerText.Trim(), out losses);

                    // Wins
                    int wins = 0;
                    var winsNode = nodes[nodes.Count - 2];
                    if (winsNode != null)
                        int.TryParse(winsNode.InnerText.Trim(), out wins);

                    // Points
                    int points = 0;
                    var pointsNode = nodes[nodes.Count - 3];
                    if (pointsNode != null)
                        int.TryParse(pointsNode.InnerText.Trim(), out points);

                    // Id and Race
                    int id =0;
                    int zoneId = 1;
                    string race = String.Empty;
                    RaceType raceType = RaceType.None;

                    var IdNode = grandMaster.SelectSingleNode("descendant::a[@class and @data-tooltip and @href]");
                    if (IdNode != null)
                    {
                        var c = HtmlHelper.ParseAHrefLink(IdNode.Attributes["href"].Value, this.RegionId);
                        id = c.CharacterId;
                        zoneId = c.ZoneId;
                        var raceNode = IdNode.Attributes["class"].Value;
                        raceType = RaceTypeServices.ParseRaceTypeContainedInText(raceNode);
                    }

                    CharacterLeague cl = new CharacterLeague();
                    cl.CharacterId = id;
                    cl.LossesCount = losses;
                    cl.MostPlayedRace = raceType;
                    cl.Name = name;
                    cl.Points = points;
                    cl.Rank = rank;
                    cl.RegionId = RegionId;
                    cl.VictoriesCount = wins;
                    cl.ZoneId = zoneId;

                    grandMasters.Add(cl);
                }

            
            }
            catch (Exception ex)
            {
                RaiseError(new ApplicationException(ex.Message));
            }
            return grandMasters;

        }


        private void RaiseError(ApplicationException ex)
        {
            if (ErrorOccuredEvent != null)
                ErrorOccuredEvent(this, ex);
        }

    }
}
