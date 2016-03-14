CREATE TABLE [dbo].[DownloadQueue]
(
	[qid] INT NOT NULL,
	[url] NVARCHAR(MAX) NOT NULL, 
    [feedId] INT NULL, 
    [serverId] INT NULL, 
    [status] INT NOT NULL DEFAULT 0, 
    [datecreated] DATETIME NULL, 
    CONSTRAINT [PK_DownloadQueue] PRIMARY KEY ([qid])
)
