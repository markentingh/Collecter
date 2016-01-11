CREATE PROCEDURE [dbo].[AddArticleSentence]
	@articleId int = 0,
	@index int = 0,
	@sentence nvarchar(MAX) = ''
AS
	INSERT INTO ArticleSentences (articleId, [index], sentence)
	VALUES (@articleId, @index, @sentence)
RETURN 0
