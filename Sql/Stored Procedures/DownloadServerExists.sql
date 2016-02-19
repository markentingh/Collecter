CREATE PROCEDURE [dbo].[DownloadServerExists]
	@settings nvarchar(MAX)
AS
	IF (SELECT COUNT(*) FROM DownloadServers WHERE settings=@settings) > 0 BEGIN
		SELECT 1;
	END ELSE BEGIN
		SELECT 0;
	END
