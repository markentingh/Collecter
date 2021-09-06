CREATE PROCEDURE [dbo].[Downloads_GetCount]
	
AS
	SELECT COUNT(*) FROM DownloadQueue WHERE [status]=0
