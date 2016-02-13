CREATE TABLE [dbo].[DownloadQueue]
(
	[url] NVARCHAR(MAX) NOT NULL PRIMARY KEY, 
    [feedId] INT NULL
)
