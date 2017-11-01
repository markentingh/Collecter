CREATE PROCEDURE [dbo].[DownloadServer_Add]
	@type int = 1,
	@title nvarchar(50) = '',
	@settings nvarchar(MAX) = ''
AS
	DECLARE @serverId int = NEXT VALUE FOR SequenceDownloadServers
	INSERT INTO DownloadServers (serverId, [type], title, settings)
	VALUES (@serverId, @type, @title, @settings)
RETURN 0
