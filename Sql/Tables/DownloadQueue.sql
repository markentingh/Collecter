CREATE TABLE [dbo].[DownloadQueue]
(
	[qid] INT NOT NULL,
	[url] NVARCHAR(MAX) NOT NULL, 
    [feedId] INT NULL, 
    CONSTRAINT [PK_DownloadQueue] PRIMARY KEY ([qid])
)
