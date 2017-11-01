CREATE PROCEDURE [dbo].[FeedCheckedLog_Add]
	@feedId int = 0,
	@links int = 0
AS
	INSERT INTO FeedsCheckedLog (feedId, links, datechecked)
	VALUES (@feedId, @links, GETDATE())
	UPDATE Feeds SET lastChecked = GETDATE()