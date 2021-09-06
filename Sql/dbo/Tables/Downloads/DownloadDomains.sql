CREATE TABLE [dbo].[DownloadDomains]
(
	[domainId] INT NOT NULL PRIMARY KEY, 
    [domain] NVARCHAR(64) NOT NULL,
    [lastchecked] DATETIME2 NOT NULL DEFAULT GETUTCDATE() -- last scraped a URL from the domain
)

GO

CREATE INDEX [IX_DownloadDomains_Domain] ON [dbo].[DownloadDomains] ([domain])
