CREATE PROCEDURE [dbo].[Downloads_GetCount]
	
AS
	SELECT COUNT(*) FROM DownloadQueue WHERE [status]=0
	UPDATE VarDates SET value=GETDATE() WHERE id=1
