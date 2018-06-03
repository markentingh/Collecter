using System;
using System.Collections.Generic;

namespace Query
{
    public static class Articles
    {
        public static int Add(Models.Article article)
        {
            return Sql.ExecuteScalar<int>("Article_Add",
                new Dictionary<string, object>()
                {
                    {"feedId", article.feedId },
                    {"subjects", article.subjects },
                    {"subjectId", article.subjectId },
                    {"score", article.score },
                    {"domain", article.domain },
                    {"url", article.url },
                    {"title", article.title },
                    {"summary", article.summary },
                    {"filesize", article.filesize },
                    {"wordcount", article.wordcount },
                    {"sentencecount", article.sentencecount },
                    {"paragraphcount", article.paragraphcount },
                    {"importantcount", article.importantcount },
                    {"yearstart", article.yearstart },
                    {"yearend", article.yearend },
                    {"years", article.years },
                    {"images", article.images },
                    {"datepublished", article.datepublished },
                    {"relavance", article.relavance },
                    {"importance", article.importance },
                    {"fiction", article.fiction },
                    {"analyzed", article.analyzed }
                }
            );
        }

        public static void Clean(int articleId)
        {
            Sql.ExecuteNonQuery("Article_Clean",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId }
                }
            );
        }

        public static bool Exists(string url)
        {
            return Sql.ExecuteScalar<int>("Article_Exists",
                new Dictionary<string, object>()
                {
                    {"url", url }
                }
            ) > 0;
        }

        public static Models.Article GetByUrl(string url)
        {
            var results = Sql.Populate<Models.Article>(
                "Article_GetByUrl",
                new Dictionary<string, object>()
                {
                    {"url", url }
                }
            );
            if(results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        public static void Remove(int articleId)
        {
            Sql.ExecuteNonQuery("Article_Remove",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId }
                }
            );
        }

        public static void Update(Models.Article article)
        {
            Sql.ExecuteNonQuery("Article_Update",
                new Dictionary<string, object>()
                {
                    {"articleId", article.articleId },
                    {"subjects", article.subjects },
                    {"subjectId", article.subjectId },
                    {"score", article.score },
                    {"title", article.title },
                    {"summary", article.summary },
                    {"filesize", article.filesize },
                    {"wordcount", article.wordcount },
                    {"sentencecount", article.sentencecount },
                    {"paragraphcount", article.paragraphcount },
                    {"importantcount", article.importantcount },
                    {"yearstart", article.yearstart },
                    {"yearend", article.yearend },
                    {"years", article.years },
                    {"images", article.images },
                    {"datepublished", article.datepublished },
                    {"relavance", article.relavance },
                    {"importance", article.importance },
                    {"fiction", article.fiction },
                    {"analyzed", article.analyzed }
                }
            );
        }

        public static void AddDate(int articleId, DateTime date, bool hasYear, bool hasMonth, bool hasDay)
        {
            Sql.ExecuteNonQuery("ArticleDate_Add",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId },
                    {"date", date },
                    {"hasyear", hasYear },
                    {"hasmonth", hasMonth },
                    {"hasday", hasDay },
                }
            ); 
        }

        public enum SortBy
        {
            oldest = 1,
            newest = 2,
            lowestScore = 3,
            highestScore = 4
        }

        public enum IsActive
        {
            notActive = 0,
            Active = 1,
            Both = 2
        }

        public static List<Models.ArticleDetails> GetList(int[] subjectId, string search = "", IsActive isActive = IsActive.Both, bool isDeleted = false, int minImages = 0, DateTime? dateStart = null, DateTime? dateEnd = null, SortBy orderBy = SortBy.oldest, int start = 1, int length = 50, bool bugsOnly = false)
        {
            return Sql.Populate<Models.ArticleDetails>(
                "Articles_GetList",
                new Dictionary<string, object>()
                {
                    {"subjectIds", subjectId.Length == 0 ? "" : string.Join(",", subjectId) },
                    { "search", search },
                    {"isActive", (int)isActive },
                    {"isDeleted", isDeleted },
                    {"minImages", minImages },
                    {"dateStart", dateStart == null ? "" : dateStart.GetValueOrDefault().ToString("yyyy/MM/dd hh:mm:ss tt") },
                    {"dateEnd", dateEnd == null ? "" : dateEnd.GetValueOrDefault().ToString("yyyy/MM/dd hh:mm:ss tt") },
                    {"orderby", (int)orderBy },
                    {"start", start },
                    {"length", length },
                    {"bugsonly", bugsOnly },

                }
            );
        }

        public static List<Models.ArticleDetails> GetListForFeeds(int[] subjectId, int feedId = -1, string search = "", IsActive isActive = IsActive.Both, bool isDeleted = false, int minImages = 0, DateTime? dateStart = null, DateTime? dateEnd = null, SortBy orderBy = SortBy.oldest, int start = 1, int length = 50, bool bugsOnly = false)
        {
            return Sql.Populate<Models.ArticleDetails>(
                "Articles_GetListForFeeds",
                new Dictionary<string, object>()
                {
                    {"subjectIds", subjectId.Length == 0 ? "" : string.Join(",", subjectId) },
                    { "feedId", feedId },
                    { "search", search },
                    {"isActive", (int)isActive },
                    {"isDeleted", isDeleted },
                    {"minImages", minImages },
                    {"dateStart", dateStart == null ? "" : dateStart.GetValueOrDefault().ToString("yyyy/MM/dd hh:mm:ss tt") },
                    {"dateEnd", dateEnd == null ? "" : dateEnd.GetValueOrDefault().ToString("yyyy/MM/dd hh:mm:ss tt") },
                    {"orderby", (int)orderBy },
                    {"start", start },
                    {"length", length },
                    {"bugsonly", bugsOnly },

                }
            );
        }

        public static List<Models.ArticleDetails> GetListForSubjects(int[] subjectId, string search = "", IsActive isActive = IsActive.Both, bool isDeleted = false, int minImages = 0, DateTime? dateStart = null, DateTime? dateEnd = null, SortBy orderBy = SortBy.oldest, int start = 1, int length = 50, int subjectStart = 1, int subjectLength = 10, bool bugsOnly = false)
        {
            return Sql.Populate<Models.ArticleDetails>(
                "Articles_GetListForSubjects",
                new Dictionary<string, object>()
                {
                    {"subjectIds", subjectId.Length == 0 ? "" : string.Join(",", subjectId) },
                    { "search", search },
                    {"isActive", (int)isActive },
                    {"isDeleted", isDeleted },
                    {"minImages", minImages },
                    {"dateStart", dateStart == null ? "" : dateStart.GetValueOrDefault().ToString("yyyy/MM/dd hh:mm:ss tt") },
                    {"dateEnd", dateEnd == null ? "" : dateEnd.GetValueOrDefault().ToString("yyyy/MM/dd hh:mm:ss tt") },
                    {"orderby", (int)orderBy },
                    {"start", start },
                    {"length", length },
                    {"subjectStart", subjectStart },
                    {"subjectLength", subjectLength },
                    {"bugsonly", bugsOnly },

                }
            );
        }

        public static void AddSentence(int articleId, int index, string sentence)
        {
            Sql.ExecuteNonQuery("ArticleSentence_Add",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId },
                    {"index", index },
                    {"sentence", sentence }
                }
            );
        }

        public static void RemoveSentences(int articleId)
        {
            Sql.ExecuteNonQuery("ArticleSentences_Remove",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId }
                }
            );
        }

        public static void AddSubject(int articleId, int subjectId, DateTime? datePublished = null, int score = 0)
        {
            Sql.ExecuteNonQuery("ArticleSubject_Add",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId },
                    {"subjectId", subjectId },
                    {"datepublished", datePublished },
                    {"score", score }
                }
            );
        }

        public static void RemoveSubjects(int articleId, int subjectId = 0)
        {
            Sql.ExecuteNonQuery("ArticleSubjects_Remove",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId },
                    {"subjectId", subjectId }
                }
            );
        }

        public static void AddWord(int articleId, int wordId, int count)
        {
            Sql.ExecuteNonQuery("ArticleWord_Add",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId },
                    {"wordId", wordId },
                    {"count", count }
                }
            );
        }

        public static void RemoveWords(int articleId, string word = "")
        {
            Sql.ExecuteNonQuery("ArticleWords_Remove",
                new Dictionary<string, object>()
                {
                    {"articleId", articleId },
                    {"word", word }
                }
            );
        }
    }
}
