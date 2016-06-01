using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SC2MiM.Common;
using SC2MiM.Common.Entities;
using System.Configuration;
using SC2MiM.Common.Services;
using System.Net;

namespace SC2WebJob
{
    public class Functions
    {

        [NoAutomaticTrigger]
        public static void ProcessArgs(string arg, TextWriter log)
        {
            ArgsParser parser = null;
            try
            {
                string[] arguments = arg.Split(new[] { ' ' });

                parser = ArgsParser.ParseArgs(arguments);

                if (parser == null)
                    return;

            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message);
            }

            if (parser == null)
                return;

            // Default connection limit
            ServicePointManager.DefaultConnectionLimit = Int32.Parse(ConfigurationManager.AppSettings["DefaultConnectionLimit"]);

            // Déclaration des divers services
            ForumServices forumServices = new ForumServices(parser.RegionId, parser.CultureId);
            CharacterLeaguesServices characterLeaguesServices = new CharacterLeaguesServices(parser.RegionId);
            CharacterProfileServices characterProfileServices = new CharacterProfileServices(parser.RegionId);

            DateTime dtBegin = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Process Console :");
            sb.AppendLine("Begin time : " + dtBegin.ToShortDateString() + " " + dtBegin.ToShortTimeString());
            sb.AppendLine("Region : " + parser.RegionId + ". Culture : " + parser.CultureId);

            try
            {

                switch (parser.Target)
                {
                    case CodeGenTarget.Character:
                    case CodeGenTarget.C:
                        Character c = new Character
                        {
                            CharacterId = parser.CharacterId,
                            Name = parser.CharacterName,
                            RegionId = parser.RegionId
                        };
                        characterProfileServices.GetCharacterSummary(c, true);

                        Console.WriteLine(c.ToString());
                        break;
                    case CodeGenTarget.CharacterReward:
                    case CodeGenTarget.Cr:
                        Character cr = new Character
                        {
                            CharacterId = parser.CharacterId,
                            Name = parser.CharacterName,
                            RegionId = parser.RegionId
                        };
                        characterProfileServices.GetCharacterRewards(cr);

                        Console.WriteLine(cr.ToString());
                        break;
                    case CodeGenTarget.CharacterLeague:
                    case CodeGenTarget.Cl:
                        Character cLeagues = new Character
                        {
                            CharacterId = parser.CharacterId,
                            Name = parser.CharacterName,
                            RegionId = parser.RegionId
                        };
                        var leagues = characterLeaguesServices.GetCharacterLeagues(cLeagues, null, false);
                        foreach (var league in leagues)
                            Console.WriteLine(league);
                        break;
                    case CodeGenTarget.Forum:
                    case CodeGenTarget.F:
                        // get existing characters

                        forumServices.ProcessAllForums(null);
                        break;
                    case CodeGenTarget.Thread:
                    case CodeGenTarget.T:
                        if (parser.ForumId > 0)
                        {
                            var allThreads = forumServices.GetThreads(parser.ForumId, null);
                            foreach (var t in allThreads)
                                Console.WriteLine(t);
                        }
                        break;
                    case CodeGenTarget.Html:
                    case CodeGenTarget.H:
                        
                        var lstChars = CharacterProfileServices.ProcessHtmlPage(parser.Url, parser.RegionId);
                        foreach(var player in lstChars)
                            Console.WriteLine(player.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Exception : " + ex.Message);
                if (ex.InnerException != null)
                    sb.AppendLine("InnerException : " + ex.InnerException.Message);

                log.WriteLine(sb.ToString());
            }

            DateTime dtEnd = DateTime.Now;
            var span = dtEnd.Subtract(dtBegin);

            sb.AppendLine("Ended : " + dtEnd.ToShortTimeString());
            sb.AppendLine("Ellapsed Time : " + span.Hours + ":" + span.Minutes + ":" + span.Seconds);

            Console.WriteLine(sb.ToString());
        }

    }
}
