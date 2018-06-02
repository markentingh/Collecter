CREATE PROCEDURE [dbo].[Feed_Add]
	@title nvarchar(100) = '',
	@url nvarchar(100) = '',
	@filter nvarchar(MAX) = '',
	@checkIntervals int = 720
AS
	DECLARE @feedId int = NEXT VALUE FOR SequenceFeeds
	INSERT INTO Feeds (feedId, title, url, checkIntervals, filter) VALUES (@feedId, @title, @url, @checkIntervals, @filter)
	SELECT @feedId
