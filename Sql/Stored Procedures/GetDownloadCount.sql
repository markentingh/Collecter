CREATE PROCEDURE [dbo].[GetDownloadCount]
	
AS
	SELECT COUNT(*) FROM DownloadQueue WHERE [status]=0
