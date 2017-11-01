CREATE PROCEDURE [dbo].[DownloadServers_GetList]

AS
	SELECT * FROM DownloadServers ORDER BY serverId ASC
