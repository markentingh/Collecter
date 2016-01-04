CREATE PROCEDURE [dbo].[RemoveArticle]
	@articleId int = 0
AS
	DELETE FROM ArticleWords WHERE articleId=@articleId
	DELETE FROM ArticleSubjects WHERE articleId=@articleId
	DELETE FROM ArticleStatistics WHERE articleId=@articleId
	DELETE FROM Articles WHERE articleId=@articleId
RETURN 0
