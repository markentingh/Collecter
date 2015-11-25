CREATE TABLE [dbo].[Articles]
(
	[articleId] INT NOT NULL PRIMARY KEY, 
    [feedId] INT NULL, 
    [subjectId] INT NULL, 
    [domain] NVARCHAR(50) NULL, 
    [url] NVARCHAR(250) NULL, 
    [title] NVARCHAR(100) NULL, 
    [keywords] NVARCHAR(250) NULL, 
    [summary] NVARCHAR(250) NULL, 
    [body] NVARCHAR(MAX) NULL, 
    [images] INT NULL, 
    [datecreated] DATETIME NULL, 
    [datepublished] DATETIME NULL, 
    [relavance] SMALLINT NULL, 
    [importance] SMALLINT NULL
)
