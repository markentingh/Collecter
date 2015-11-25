CREATE TABLE [dbo].[Feeds]
(
	[feedId] INT NOT NULL PRIMARY KEY, 
    [url] NVARCHAR(100) NULL, 
    [lastChecked] DATETIME NULL
)
