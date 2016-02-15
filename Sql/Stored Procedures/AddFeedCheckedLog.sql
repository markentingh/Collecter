CREATE PROCEDURE [dbo].[AddFeedCheckedLog]
	@feedId int = 0,
	@links int = 0
AS
	INSERT INTO FeedsCheckedLog (feedId, links, datechecked)
	VALUES (@feedId, @links, GETDATE())