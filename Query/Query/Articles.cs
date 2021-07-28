using System;
using System.Collections.Generic;
using System.Linq;

namespace Query
{
    public static class Articles
    {
        public static int Add(Models.Article article)
        {
            try
            {
                return Sql.ExecuteScalar<int>("Article_Add", new { 
                    article.feedId,
                    article.subjects,
                    article.subjectId,
                    article.score,
                    article.domain,
                    article.url,
                    article.title,
                    article.summary,
                    article.filesize,
                    article.wordcount,
                    article.sentencecount,
                    article.paragraphcount,
                    article.importantcount,
                    article.yearstart,
                    article.yearend,
                    article.years,
                    article.images,
                    article.datepublished,
                    article.relavance,
                    article.importance,
                    article.fiction,
                    article.analyzed,
                    article.active 
                });
            } catch(Exception ex)
            {
                throw ex;
            }
            
        }

        public static void Clean(int articleId)
        {
            Sql.ExecuteNonQuery("Article_Clean", new { articleId });
        }

        public static bool Exists(string url)
        {
            return Sql.ExecuteScalar<int>("Article_Exists", new { url }) > 0;
        }

        public static Models.Article GetByUrl(string url)
        {
            return Sql.Populate<Models.Article>("Article_GetByUrl", new { url }).FirstOrDefault();
        }

        public static void Remove(int articleId)
        {
            Sql.ExecuteNonQuery("Article_Remove", new { articleId });
        }

        public static void Update(Models.Article article)
        {
            Sql.ExecuteNonQuery("Article_Update", new {
                article.articleId,
                article.subjects,
                article.subjectId,
                article.score,
                article.title,
                article.summary,
                article.filesize,
                article.wordcount,
                article.sentencecount,
                article.paragraphcount,
                article.importantcount,
                article.yearstart,
                article.yearend,
                article.years,
                article.images,
                article.datepublished,
                article.relavance,
                article.importance,
                article.fiction,
                article.analyzed 
                });
        }

        public static void AddDate(int articleId, DateTime date, bool hasYear, bool hasMonth, bool hasDay)
        {
            Sql.ExecuteNonQuery("ArticleDate_Add", new { articleId, date, hasYear, hasMonth, hasDay }); 
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
                new {
                    subjectIds = subjectId.Length == 0 ? "" : string.Join(",", subjectId),
                    search,
                    isActive = (int)isActive,
                    isDeleted,
                    minImages,
                    dateStart = dateStart == null ? "" : dateStart.Value.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    dateEnd = dateEnd == null ? "" : dateEnd.Value.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    orderby = (int)orderBy,
                    start,
                    length,
                    bugsOnly
                });
        }

        public static List<Models.ArticleDetails> GetListForFeeds(int[] subjectId, int feedId = -1, string search = "", IsActive isActive = IsActive.Both, bool isDeleted = false, int minImages = 0, DateTime? dateStart = null, DateTime? dateEnd = null, SortBy orderBy = SortBy.oldest, int start = 1, int length = 50, bool bugsOnly = false)
        {
            return Sql.Populate<Models.ArticleDetails>(
                "Articles_GetListForFeeds",
                new {
                    subjectIds = subjectId.Length == 0 ? "" : string.Join(",", subjectId),
                    feedId,
                    search,
                    isActive = (int)isActive,
                    isDeleted,
                    minImages,
                    dateStart = dateStart == null ? "" : dateStart.Value.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    dateEnd = dateEnd == null ? "" : dateEnd.Value.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    orderby = (int)orderBy,
                    start,
                    length,
                    bugsOnly
                });
        }

        public static List<Models.ArticleDetails> GetListForSubjects(int[] subjectId, string search = "", IsActive isActive = IsActive.Both, bool isDeleted = false, int minImages = 0, DateTime? dateStart = null, DateTime? dateEnd = null, SortBy orderBy = SortBy.oldest, int start = 1, int length = 50, int subjectStart = 1, int subjectLength = 10, bool bugsOnly = false)
        {
            return Sql.Populate<Models.ArticleDetails>(
                "Articles_GetListForSubjects",
                new {
                    subjectIds = subjectId.Length == 0 ? "" : string.Join(",", subjectId),
                    search,
                    isActive = (int)isActive,
                    isDeleted,
                    minImages,
                    dateStart = dateStart == null ? "" : dateStart.Value.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    dateEnd = dateEnd == null ? "" : dateEnd.Value.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    orderby = (int)orderBy,
                    start,
                    length,
                    subjectStart,
                    subjectLength,
                    bugsOnly
                });
        }

        public static void AddSentence(int articleId, int index, string sentence)
        {
            Sql.ExecuteNonQuery("ArticleSentence_Add", new { articleId, index, sentence });
        }

        public static void RemoveSentences(int articleId)
        {
            Sql.ExecuteNonQuery("ArticleSentences_Remove", new { articleId });
        }

        public static void AddSubject(int articleId, int subjectId, DateTime? datePublished = null, int score = 0)
        {
            Sql.ExecuteNonQuery("ArticleSubject_Add", new { articleId, subjectId, datePublished, score });
        }

        public static void RemoveSubjects(int articleId, int subjectId = 0)
        {
            Sql.ExecuteNonQuery("ArticleSubjects_Remove", new { articleId, subjectId });
        }

        public static void AddWord(int articleId, int wordId, int count)
        {
            Sql.ExecuteNonQuery("ArticleWord_Add", new { articleId, wordId, count });
        }

        public static void RemoveWords(int articleId, string word = "")
        {
            Sql.ExecuteNonQuery("ArticleWords_Remove", new { articleId, word });
        }
    }
}
