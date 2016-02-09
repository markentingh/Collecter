CREATE PROCEDURE [dbo].[CheckedFeed]
	@feedId int = 0
AS
	UPDATE Feeds SET lastChecked=GETDATE() WHERE feedId=@feedId
RETURN 0
