CREATE PROCEDURE [dbo].[DownloadDistribution_Add]
	@serverId int = 0
AS
	/* adds 10 random downloads to the queue for a specific server */
	UPDATE DownloadQueue SET serverId=@serverId 
	WHERE qid IN (SELECT TOP 10 qid FROM DownloadQueue WHERE [status]=0 ORDER BY rndid ASC)
RETURN 0
