CREATE PROCEDURE [dbo].[AddFeed]
	@title nvarchar(100) = '',
	@url nvarchar(100) = '',
	@filter nvarchar(MAX) = ''
AS
	DECLARE @feedId int = NEXT VALUE FOR SequenceFeeds
	INSERT INTO Feeds (feedId, title, url, filter) VALUES (@feedId, @title, @url, @filter)
RETURN 0
