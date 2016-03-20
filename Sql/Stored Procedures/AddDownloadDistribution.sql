CREATE PROCEDURE [dbo].[AddDownloadDistribution]
	@serverId int = 0
AS
	UPDATE DownloadQueue SET serverId=@serverId 
	WHERE qid IN (SELECT TOP 10 qid FROM DownloadQueue WHERE [status]=0 ORDER BY rndid ASC)
RETURN 0
