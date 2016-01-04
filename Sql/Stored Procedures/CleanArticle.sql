CREATE PROCEDURE [dbo].[CleanArticle]
	@articleId int = 0
AS
	EXEC RemoveArticleSubjects @articleId=@articleId
	EXEC RemoveArticleWords @articleId=@articleId
RETURN 0
