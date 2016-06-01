using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Threading.Tasks;
using SC2MiM.Common.Entities;
using SC2MiM.Common.Helpers;
using System.Threading;

namespace SC2MiM.Common.Services
{
    /// <summary>
    /// Inspect Blizzard Forums and retrieve players urls
    /// Updated 2015-07-29
    /// </summary>
    public class ForumServices
    {
        public String RegionId { get; private set; }
        public String CultureId { get; private set; }
        public String GlobalForumUrl { get; private set; }
        public String ForumUrl { get; private set; }
        public String TopicUrl { get; set; }
        private object locker = new object();

        private Dictionary<String, Character> existingCharacters;

        public event EventHandler<ApplicationException> ErrorOccuredEvent;
        public event EventHandler<String> EventOccured;

        public ForumServices(string regionId, String cultureId)
        {
            this.RegionId = regionId;
            this.CultureId = cultureId;


            this.GlobalForumUrl = String.Format(Urls.GlobalForumUrl, this.RegionId, this.CultureId);

            this.ForumUrl = String.Concat(this.GlobalForumUrl, Urls.ForumUrl);
            this.TopicUrl = String.Concat(this.GlobalForumUrl, Urls.TopicUrl);
        }

        public ForumServices(string regionId)
        {
            this.RegionId = regionId;
            this.CultureId = CultureHelper.DefaultCultureId;

            this.GlobalForumUrl = String.Format(Urls.GlobalForumUrl, this.RegionId, this.CultureId);

            this.ForumUrl = String.Concat(this.GlobalForumUrl, Urls.ForumUrl);
            this.TopicUrl = String.Concat(this.GlobalForumUrl, Urls.TopicUrl);
        }


        public void ProcessAllForums(DateTime? sinceDate)
        {

            this.existingCharacters = DatabaseServices.GetMinimumCharactersFields(this.RegionId);

            // Parcours des forums trouvés
            foreach (int forumId in this.GetForums())
            {
                this.ProcessForum(forumId, sinceDate);
            }
        }

        public void ProcessForum(Int32 forumId, DateTime? sinceDate)
        {
            // If we call directly this method, be sure to get the existing characters
            if (this.existingCharacters == null)
                this.existingCharacters = DatabaseServices.GetMinimumCharactersFields(this.RegionId);

            // Get all threads from forum by forumId
            var lstThread = this.GetThreads(forumId, sinceDate);

            if (lstThread == null)
                return;

            //Parcours des threads trouvés dans le forum
            Parallel.ForEach(lstThread, Tools.GetParallelOptions(), topic =>
            {
                this.ProcessThread(topic);

                // Try to not overhead CPU
                Thread.Sleep(100);
            });

        }

        public void ProcessThread(Int64 threadId)
        {
            try
            {

                // Récupération des Joueurs
                List<Character> lstCharacters = this.GetCharactersFromTopic(threadId);

                // Character profile service
                CharacterProfileServices characterProfileServices = new CharacterProfileServices(this.RegionId);

                if (lstCharacters != null)
                {


                    //Pour chaque joueur non traités
                    Parallel.ForEach(lstCharacters, Tools.GetParallelOptions(), c =>
                    {

                        Dictionary<Int32, Character> alreadyProcessedCharacters = new Dictionary<int, Character>();
                        Dictionary<Int32, League> alreadyProcessedLeagues = new Dictionary<int, League>();
                        int levelDepth = 1;

                        characterProfileServices.RecurseCharacter(c, alreadyProcessedCharacters, alreadyProcessedLeagues,
                                                                  1, ref levelDepth, false);

                        lock (locker)
                        {
                            List<Character> lstCharacters2 =
                                alreadyProcessedCharacters.Select(ch => ch.Value).ToList();

                            // je merge ma liste courante et j'enlève ceux déjà traités
                            List<Character> lstCharactersMerged = null;

                            if (this.existingCharacters == null)
                                lstCharactersMerged = lstCharacters2;
                            else
                                lstCharactersMerged = characterProfileServices.Merge(lstCharacters2, this.existingCharacters);

                            DatabaseServices.MergeCharacters(lstCharactersMerged);

                            if (this.existingCharacters != null)
                                foreach (var cc in lstCharactersMerged)
                                    this.existingCharacters.Add(String.Format("{0}/{1}/{2}", cc.CharacterId, cc.ZoneId, cc.Name), cc);

                            if (lstCharactersMerged.Count > 0)
                                RaiseMessage("Process Character : " + c.Name + " and " + lstCharactersMerged.Count.ToString() + " players");
                        }


                        // Try to not overhead CPU
                        Thread.Sleep(10);
                    });


                }
            }
            catch (Exception ex)
            {
                String message = "[ForumServices].[ProcessThread] : Error : ThreadId" + threadId + " (" + this.RegionId + ")";
                RaiseError(new ApplicationException(message, ex));
            }
        }

        /// <summary>
        /// Gets all the Forums Id from one specific Region Name
        /// Updated 2015-07-29
        /// </summary>
        public List<Int32> GetForums()
        {

            List<Int32> forums = new List<int>();
            try
            {
                HttpStatusCode statusCode;

                HtmlDocument doc = HtmlHelper.GetDocument(this.GlobalForumUrl, out statusCode, true);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {

                    const string message = "[ForumServices].[GetForums] : HtmlDocument Is null.";
                    RaiseError(new ApplicationException(message));
                    return null;
                }

                var tableForums = doc.DocumentNode.SelectNodes("//a[@class='forum-link']").ToList();

                foreach (var forumLinkNode in tableForums)
                {
                    int forumId;

                    if (!forumLinkNode.Attributes.Contains("href"))
                        continue;


                    String href = forumLinkNode.Attributes["href"].Value;
                    href = href.Substring(0, href.Length - 1);

                    if (Int32.TryParse(href, out forumId))
                        forums.Add(forumId);
                }
            }
            catch (Exception ex)
            {
                String message = "[ForumServices].[GetForums] : Error during GetForums)";
                RaiseError(new ApplicationException(message, ex));

                throw ex;

            }

            return forums;

        }

        /// <summary>
        /// Gets all the threads ids from one specific Forum
        /// Updated 2015-07-29
        /// </summary>
        public List<Int64> GetThreads(int forumId, DateTime? sinceDateTime)
        {

            String url = String.Format(this.ForumUrl, forumId, "1");

            List<Int64> threads = new List<Int64>();

            try
            {
                HttpStatusCode statusCode;
                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode, true);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {

                    String message = "[ForumServices].[GetThreads] : HtmlDocument Is null. ForumId = " + forumId;
                    RaiseError(new ApplicationException(message));
                    return null;
                }

                // Récupération du nombre de page de ce forum 
                int forumPageNumbers = GetForumPageNumber(doc, forumId);

                Parallel.For(1, forumPageNumbers + 1, Tools.GetParallelOptions(), (currentPageId) =>
                {
                    try
                    {
                        // Chargement de la page en cours (sauf pour la première, inutile vu que déjà chargée
                        if (currentPageId != 1)
                        {
                            url = String.Format(this.ForumUrl, forumId, currentPageId);
                            doc = HtmlHelper.GetDocument(url, out statusCode, true);
                        }

                        if (doc != null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                        {

                            var allRows = doc.DocumentNode.SelectNodes("//tr[@class='regular-topic']").ToList();

                            if (allRows != null && allRows.Count > 0)
                            {
                                foreach (var rowNode in allRows)
                                {
                                    DateTime? dtTime = null;

                                    if (sinceDateTime.HasValue)
                                    {
                                        var dateNode = rowNode.SelectSingleNode("child::node()//meta[@itemprop='dateModified']");
                                        if (dateNode != null)
                                        {
                                            var dateText = dateNode.Attributes["content"].Value;

                                            dtTime = CultureHelper.GetDateTimeFromCultureId(this.CultureId, dateText);
                                        }
                                    }

                                    if (dtTime == null || dtTime > sinceDateTime)
                                    {

                                        var dataTopicId = rowNode.Attributes["data-topic-id"];

                                        if (dataTopicId == null)
                                            continue;

                                        Int64 threadId = 0;

                                        if (Int64.TryParse(dataTopicId.Value, out threadId))
                                            threads.Add(threadId);

                                    }
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        String message = "[ForumServices].[GetThreads] : Error during Parrallel For GetThreads : forumId = " + forumId + " sinceDate = " + (sinceDateTime.HasValue ? sinceDateTime.Value.ToString() : "NULL");
                        RaiseError(new ApplicationException(message, ex));
                    }
                });
            }
            catch (Exception ex)
            {
                String message = "[ForumServices].[GetThreads] : rror during GetThreads : forumId = " + forumId + " sinceDate = " + (sinceDateTime.HasValue ? sinceDateTime.Value.ToString() : "NULL");
                RaiseError(new ApplicationException(message, ex));

            }

            return threads;
        }

        /// <summary>
        /// Gets all the Ids from a specific thread designe by its thread Id
        /// Updated 2015-07-29
        /// </summary>
        public List<Character> GetCharactersFromTopic(long topicId)
        {
            // TODO : Make Zone Parametrable in URL
            String url = String.Format(this.TopicUrl, topicId, "1");

            List<Character> characters = new List<Character>();

            try
            {

                HttpStatusCode statusCode;
                HtmlDocument doc = HtmlHelper.GetDocument(url, out statusCode, true);
                if (doc == null || statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound)
                {

                    String message = "[ForumServices].[GetCharactersFromTopic] : HtmlDocument Is null. TopicId = " + topicId;
                    RaiseError(new ApplicationException(message));
                    return null;
                }

                // Récupération du nombre de page de ce forum 
                int forumPageNumbers = GetTopicPageNumber(doc, topicId);

                Parallel.For(1, forumPageNumbers + 1, Tools.GetParallelOptions(), currentPageId =>
                {
                    try
                    {
                        // Chargement de la page en cours (sauf pour la première, inutile vu que déjà chargée

                        if (currentPageId != 1)
                        {
                            HttpStatusCode sCode;
                            url = String.Format(this.TopicUrl, topicId, currentPageId);
                            doc = HtmlHelper.GetDocument(url, out sCode, true);
                        }

                        var allNodes = doc.DocumentNode.SelectNodes("//div[@class='avatar-outer']/a");

                        if (allNodes != null)
                        {

                            foreach (var charNode in allNodes)
                            {

                                String href = charNode.Attributes["href"].Value;

                                Character c = HtmlHelper.ParseAHrefLink(href, this.RegionId);

                                if (c != null)
                                {
                                    lock (locker)
                                    {
                                        if (!characters.Any(tmpc => tmpc.CharacterId == c.CharacterId))
                                            characters.Add(c);
                                    }

                                }

                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        String message = "[ForumServices].[GetCharactersFromTopic] : Error during Parrallel For GetCharactersFromTopic. TopicId = " + topicId + ". TopicUrl = " + url;
                        RaiseError(new ApplicationException(message, ex));
                        throw ex;

                    }
                });
            }
            catch (Exception exx)
            {
                String message = "[ForumServices].[GetCharactersFromTopic] : Error during GetCharactersFromTopic. TopicId = " + topicId + ". TopicUrl = " + url;
                RaiseError(new ApplicationException(message, exx));
                return null;
            }

            return characters;
        }

        /// <summary>
        /// Gets the numbers of page in a topic document
        /// Updated 2015-07-29
        /// </summary>
        private int GetTopicPageNumber(HtmlDocument topicPageDocument, long topicId)
        {
            Int32 pageNumbers = 1;

            try
            {
                List<HtmlNode> allPagesHRef = null;
                try
                {
                    allPagesHRef = (from n in topicPageDocument.DocumentNode.SelectNodes("//div[@class='forum-wrapper']/descendant::a[@data-pagenum]")
                                    select n).ToList();
                }
                catch (Exception)
                {
                    return pageNumbers;
                }

                foreach (var hrefNode in allPagesHRef)
                {
                    var hrefText = hrefNode.InnerText;
                    int tmpPage = 0;

                    Int32.TryParse(hrefNode.InnerText, out tmpPage);

                    if (tmpPage > pageNumbers)
                        pageNumbers = tmpPage;
                }
            }
            catch (Exception ex)
            {

                String message = "[ForumServices].[GetTopicPageNumber] : Error during GetTopicPageNumber. TopicId = " + topicId + ". TopicUrl = " + TopicUrl;
                RaiseError(new ApplicationException(message, ex));
            }

            return pageNumbers;
        }

        /// <summary>
        /// Gets the number of pages in a specific forum designed by its Id.
        /// Updated 2015-07-29
        /// </summary>
        private Int32 GetForumPageNumber(HtmlDocument forumPageDocument, int forumId)
        {
            Int32 pageNumbers = 1;

            try
            {
                var allPagesHRef = (from n in forumPageDocument.DocumentNode.SelectNodes("//div[@class='paging-wrapper']/descendant::a[@data-pagenum]")
                                    select n).ToList();

                if (allPagesHRef.Count == 0)
                    return pageNumbers;

                foreach (var hrefNode in allPagesHRef)
                {
                    var hrefText = hrefNode.Attributes["data-pagenum"].Value;
                    int tmpPage = 0;

                    Int32.TryParse(hrefNode.InnerText, out tmpPage);

                    if (tmpPage > pageNumbers)
                        pageNumbers = tmpPage;
                }
            }
            catch (Exception ex)
            {

                String message = "[ForumServices].[GetTopicPageNumber] : Error during GetForumPageNumber. TopicId = " + forumId + ". TopicUrl = " + TopicUrl;
                RaiseError(new ApplicationException(message, ex));

            }

            return pageNumbers;

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
