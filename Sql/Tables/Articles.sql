CREATE TABLE [dbo].[Articles]
(
	[articleId] INT NOT NULL PRIMARY KEY, 
    [feedId] INT NULL, 
	[subjects] int NULL,
    [images] INT NULL, 
    [datecreated] DATETIME NULL, 
    [datepublished] DATETIME NULL, 
    [relavance] SMALLINT NULL, 
    [importance] SMALLINT NULL, 
    [fiction] SMALLINT NULL, 
    [domain] NVARCHAR(50) NULL, 
    [url] NVARCHAR(250) NULL, 
    [title] NVARCHAR(250) NULL, 
    [summary] NVARCHAR(250) NULL,
	[analyzed] FLOAT DEFAULT 0, 
	[unread] BIT NULL DEFAULT 0, 
	[cached] BIT NULL DEFAULT 0, 
    [active] BIT NULL DEFAULT 0, 
    [deleted] BIT NULL DEFAULT 0
)
