using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SC2WebJob
{
    public class ArgsParser
    {
        private static string helpString;

        private CodeGenTarget target;

        public bool RegionSpectified { get; set; }

        public bool VerboseEnabled { get; set; }

        public bool HelpRequested { get; set; }

        public int ForumId { get; set; }

        public long ThreadId { get; set; }

        public String Url { get; set; }

        public String CultureId { get; set; }


        public CodeGenTarget Target
        {
            get { return target; }
            set { target = value; }
        }
        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        public String RegionId { get; set; }

      
        /// <summary>
        /// Gets or sets a value indicating whether return verbose information.
        /// </summary>
        public Boolean IsVerbose { get; set; }

        /// <summary>
        /// Gets or sets the name of the character to extract
        /// </summary>
        public String CharacterName { get; set; }

        /// <summary>
        /// Gets or sets the character id.
        /// </summary>
        public int CharacterId { get; set; }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns></returns>
        public static String GetHelp()
        {
            if (String.IsNullOrEmpty(helpString))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Params : [/?] [/region:Region id (eu - us)] [/zone:Zone id (1 or 2)] [/target:Operation] [/id:Int Id] [/name:Character Name] [/forumid:Forum Id] [/threadid:Thread Id] ");
                sb.AppendLine("Param Details :");
                sb.AppendLine("\t/?\tPrint this message");
                sb.AppendLine("\t/X\t : EXIT");
                sb.AppendLine("\t/region\tMANDATORY : Region to process : eu or us");
                sb.AppendLine("\t/zone\tMANDATORY : Zone : 1 or 2");
                sb.AppendLine("\t/culture\tCulture : Need for process forums (en, fr, it, es, de)");
                sb.AppendLine("\t/name\tName of the character to process.");
                sb.AppendLine("\t/sincedate\tDate limit for process.");
                sb.AppendLine("\t/id\tId of the character to process.");
                sb.AppendLine("\t/forumid\tId of the forum to process.");
                sb.AppendLine("\t/threadid\tId of the thread to process.");
                sb.AppendLine("\t/url\tA page url.");
                sb.AppendLine("\t-----------------------------------------------------------------");
                sb.AppendLine("\t/target\tMANDATORY : Operation to realize : ");
                sb.AppendLine("\t\t* Character (or C) : Get a Character (Need /CharacterId and /CharacterName) ");
                sb.AppendLine("\t\t* CharacterReward (or CR) : Get a Character rewards (Need /CharacterId and /CharacterName) ");
                sb.AppendLine("\t\t* CharacterLeague (or CL) : Get a Character League (Need /CharacterId and /CharacterName) ");
                sb.AppendLine("\t\t* Forum (or F) : Process all Forums to Get new chars. (Need /SinceDate) ");
                sb.AppendLine("\t\t* Thread (or T) : Process a Thread. (Need /ThreadId) ");
                sb.AppendLine("\t\t* Html (or H) : Process a Html page (need Url)");
                sb.AppendLine("\t\t* P1V1 : Process league 1 on 1");
                sb.AppendLine("\t\t* P2V2 : Process leagues 2 on 2");
                sb.AppendLine("\t\t* P3V3 : Process leagues 3 on 3");
                sb.AppendLine("\t\t* P4V4 : Process leagues 4 on 4");
                sb.AppendLine("\t-----------------------------------------------------------------");

                helpString = sb.ToString();
            }

            return helpString;
        }


        public DateTime? SinceDate { get; set; }

        public static ArgsParser ParseArgs(string[] args)
        {
            ArgsParser parser = new ArgsParser();
            // Default if not set
            parser.CultureId = "en";

            foreach (string str in args)
            {
                string[] source = str.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (((source.Count() < 2) && !source[0].Equals("/?", StringComparison.InvariantCultureIgnoreCase)))
                    throw new ArgumentException("Invalid parameter passed", str);

                switch (source[0].ToLowerInvariant())
                {
                    case "/region":
                        parser.RegionSpectified = true;
                        string region = source[1].ToLower().Trim();

                        if (region != "eu" && region != "us")
                            throw new InvalidOperationException(string.Format("Invalid {0} region specified.", source[0]));

                        parser.RegionId = region;
                        break;

                    case "/culture":
                        string culture = source[1].ToLower().Trim();

                        if (culture != "en" && culture != "fr" && culture != "it" && culture != "de" && culture != "es")
                            throw new InvalidOperationException(string.Format("Invalid {0} culture specified.", source[0]));

                        parser.CultureId = culture;
                        break;

                 
                    case "/target":
                        if (!EnumUtils.TryEnumParse(source[1], out parser.target))
                            throw new ArgumentOutOfRangeException(str, string.Format("Invalid {0} option specified.", source[0]));

                        break;

                    case "/name":
                        parser.CharacterName = source[1];
                        break;

                    case "/id":
                        int characterId;

                        if (!int.TryParse(source[1].Trim(), out characterId))
                            throw new InvalidOperationException(string.Format("Invalid {0}  specified.", source[0]));

                        parser.CharacterId = characterId;
                        break;

                    case "/forumid":
                        int forumId;

                        if (!int.TryParse(source[1].Trim(), out forumId))
                            throw new InvalidOperationException(string.Format("Invalid {0}  specified.", source[0]));

                        parser.ForumId = forumId;
                        break;

                    case "/threadid":
                        long threadId;

                        if (!long.TryParse(source[1].Trim(), out threadId))
                            throw new InvalidOperationException(string.Format("Invalid {0}  specified.", source[0]));

                        parser.ThreadId = threadId;
                        break;

                    case "/sincedate":
                        DateTime sinceDate;

                        if (!DateTime.TryParse(source[1].Trim(),  CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None , out sinceDate))
                            throw new InvalidOperationException(string.Format("Invalid {0}  specified.", source[0]));

                        parser.SinceDate = sinceDate;
                        break;

                    case "/verbose":
                        parser.VerboseEnabled = true;
                        break;

                    case "/url":
                        parser.Url = source[1].Trim();
                        break;

                    case "/?":
                        parser.HelpRequested = true;
                        Console.WriteLine(GetHelp());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(str);
                }
            }

            if (!parser.RegionSpectified)
                throw new InvalidOperationException("Region, Zone and Target are mandatory. See help");

            return parser;
        }

    
    }

    internal static class EnumUtils
    {
        // Methods
        public static bool TryEnumParse<T>(string enumString, out T mode)
        {
            mode = default(T);
            try
            {
                mode = (T)Enum.Parse(typeof(T), enumString, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }



    public enum CodeGenTarget
    {
        // Get Character
        Character,
        C,
        // Get Character League
        CharacterLeague,
        Cl,
        // Get Forum Thread
        Thread,
        T,
        // Get League
        //League,
        //L,
        // Get Forum
        Forum,
        F,
        // Process leagues
        P1V1,
        P2V2,
        P3V3,
        P4V4,
        // Process a page
        Html,
        H,
        // Character Rewards
        CharacterReward,
        Cr,
        RefreshDatabase,
        Rd
    }
}

