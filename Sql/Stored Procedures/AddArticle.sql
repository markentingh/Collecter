CREATE PROCEDURE [dbo].[AddArticle]
	@feedId int = 0,
	@subjects int = 0,
	@domain nvarchar(50),
	@url nvarchar(250),
	@title nvarchar(250),
	@summary nvarchar(250),
	@images int = 0,
	@datepublished datetime,
	@relavance smallint = 1,
	@importance smallint = 1,
	@fiction smallint = 1
AS
	DECLARE @articleId int = NEXT VALUE FOR SequenceArticles
	INSERT INTO Articles (articleId, feedId, subjects, domain, url, title, summary, images, datecreated, datepublished, relavance, importance, fiction)
		VALUES (@articleId, @feedId, @subjects, @domain, @url, @title, @summary, @images, GETDATE(), @datepublished, @relavance, @importance, @fiction)
	SELECT @articleId
RETURN 0
