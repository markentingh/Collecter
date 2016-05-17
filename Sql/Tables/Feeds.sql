CREATE TABLE [dbo].[Feeds]
(
	[feedId] INT NOT NULL PRIMARY KEY, 
    [title] NVARCHAR(100) NULL, 
	[url] NVARCHAR(100) NULL, 
    [checkIntervals] INT NULL, 
    [lastChecked] DATETIME NULL, 
    [filter] NVARCHAR(MAX) NULL
)
