CREATE PROCEDURE [dbo].[AddDownloadDistribution]
	@serverId int = 0
AS
	UPDATE DownloadQueue SET serverId=@serverId 
	WHERE qid IN (SELECT TOP 10 qid FROM DownloadQueue WHERE [status]=0 ORDER BY qid ASC)
RETURN 0
