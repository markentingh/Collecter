CREATE PROCEDURE [dbo].[GetDownloadDistributionList]
	@serverId int = 0
AS
	SELECT * FROM DownloadQueue WHERE serverId=@serverId AND status=0 ORDER BY rndid ASC
RETURN 0
