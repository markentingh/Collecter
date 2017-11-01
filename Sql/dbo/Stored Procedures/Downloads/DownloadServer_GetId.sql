CREATE PROCEDURE [dbo].[DownloadServer_GetId]
	@host nvarchar(MAX)
AS
	SELECT serverId FROM DownloadServers WHERE settings=@host
RETURN 0
