CREATE PROCEDURE [dbo].[RemoveArticleSentences]
	@articleId int = 0
AS
	DELETE FROM ArticleSentences WHERE articleId=@articleId
RETURN 0
