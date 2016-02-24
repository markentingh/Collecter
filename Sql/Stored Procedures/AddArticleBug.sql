CREATE PROCEDURE [dbo].[AddArticleBug]
	@articleId int = 0,
	@title nvarchar(100) = '',
	@description nvarchar(MAX) = '',
	@status tinyint = 0
AS
	DECLARE @bugId int = NEXT VALUE FOR SequenceArticleBugs
	INSERT INTO ArticleBugs (bugId, articleId, title, [description], [status])
	VALUES (@bugId, @articleId, @title, @description, @status)
