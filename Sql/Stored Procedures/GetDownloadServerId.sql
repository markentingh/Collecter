CREATE PROCEDURE [dbo].[GetDownloadServerId]
	@host nvarchar(MAX)
AS
	SELECT serverId FROM DownloadServers WHERE settings=@host
RETURN 0
