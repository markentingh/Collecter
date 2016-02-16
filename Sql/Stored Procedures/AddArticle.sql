CREATE PROCEDURE [dbo].[AddArticle]
	@feedId int = 0,
	@subjects int = 0,
	@domain nvarchar(50),
	@url nvarchar(250),
	@title nvarchar(250),
	@summary nvarchar(250),
	@wordcount int = 0,
	@sentencecount int = 0,
	@paragraphcount int = 0,
	@importantcount int = 0,
	@yearstart int = 0,
	@yearend int = 0,
	@years nvarchar(50),
	@images int = 0,
	@datepublished datetime,
	@relavance smallint = 1,
	@importance smallint = 1,
	@fiction smallint = 1
AS
	DECLARE @articleId int = NEXT VALUE FOR SequenceArticles
	INSERT INTO Articles 
	(articleId, feedId, subjects, domain, url, title, summary, wordcount, sentencecount, paragraphcount, importantcount, 
	yearstart, yearend, years, images, datecreated, datepublished, relavance, importance, fiction)
	VALUES 
	(@articleId, @feedId, @subjects, @domain, @url, @title, @summary, @wordcount, @sentencecount, @paragraphcount, @importantcount, 
	@yearstart, @yearend, @years, @images, GETDATE(), @datepublished, @relavance, @importance, @fiction)

	SELECT @articleId
