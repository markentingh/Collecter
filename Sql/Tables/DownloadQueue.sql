CREATE TABLE [dbo].[DownloadQueue]
(
	[qid] INT NOT NULL,
	[rndid] INT NULL DEFAULT 0,
    [feedId] INT NULL, 
    [serverId] INT NULL, 
    [status] INT NOT NULL DEFAULT 0, 
	[url] NVARCHAR(MAX) NOT NULL, 
    [datecreated] DATETIME NULL, 
    CONSTRAINT [PK_DownloadQueue] PRIMARY KEY ([qid])
)
