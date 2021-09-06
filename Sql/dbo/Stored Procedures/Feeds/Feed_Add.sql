CREATE PROCEDURE [dbo].[Feed_Add]
	@categoryId int,
	@title nvarchar(100) = '',
	@url nvarchar(100) = '',
	@filter nvarchar(MAX) = '',
	@checkIntervals int = 720 --(12 hours)
AS
	DECLARE @feedId int = NEXT VALUE FOR SequenceFeeds
	INSERT INTO Feeds (feedId, categoryId, title, url, checkIntervals, filter, lastChecked) 
	VALUES (@feedId, @categoryId, @title, @url, @checkIntervals, @filter, DATEADD(HOUR, -24, GETUTCDATE()))
	SELECT @feedId
