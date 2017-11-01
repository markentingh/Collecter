CREATE TABLE [dbo].[DownloadServers]
(
	[serverId] INT NOT NULL PRIMARY KEY, 
	[type] INT NOT NULL DEFAULT 1,
    [title] NVARCHAR(50) NULL,
	[settings] NVARCHAR(MAX)
)
