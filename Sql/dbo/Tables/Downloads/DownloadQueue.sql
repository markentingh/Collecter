CREATE TABLE [dbo].[DownloadQueue]
(
	[qid] INT NOT NULL,
    [feedId] INT NULL, 
    [domainId] INT NULL, 
    [status] INT NOT NULL DEFAULT 0, 
	[url] NVARCHAR(255) NOT NULL, 
    [datecreated] DATETIME NULL, 
    CONSTRAINT [PK_DownloadQueue] PRIMARY KEY ([qid])
)

GO

CREATE INDEX [IX_DownloadQueue_Url] ON [dbo].[DownloadQueue] ([url])
