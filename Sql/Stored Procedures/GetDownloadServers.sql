CREATE PROCEDURE [dbo].[GetDownloadServers]

AS
	SELECT * FROM DownloadServers ORDER BY serverId ASC
