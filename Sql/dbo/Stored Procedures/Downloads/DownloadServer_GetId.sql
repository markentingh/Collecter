CREATE PROCEDURE [dbo].[DownloadServer_GetId]
	@settings nvarchar(MAX)
AS
	SELECT serverId FROM DownloadServers WHERE settings=@settings
RETURN 0
