CREATE TABLE [dbo].[ArticleStatistics]
(
	[statId] INT NOT NULL PRIMARY KEY, 
    [articleId] INT NULL, 
	[wordIndex] INT NULL,
    [startdate] DATETIME NULL, 
	[enddate] DATETIME NULL, 
	[value1] INT,
    [metric1] NVARCHAR(50) NULL, 
	[value2] INT,
    [metric2] NVARCHAR(50) NULL, 
	[value3] INT,
    [metric3] NVARCHAR(50) NULL, 
	[value4] INT,
    [metric4] NVARCHAR(50) NULL,
	[value5] INT,
    [metric5] NVARCHAR(50) NULL,
    [description] NVARCHAR(MAX) NULL
)
