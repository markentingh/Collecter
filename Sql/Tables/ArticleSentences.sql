CREATE TABLE [dbo].[ArticleSentences]
(
	[articleId] INT NOT NULL, 
    [index] SMALLINT NULL, 
	[isbulleted] BIT NOT NULL DEFAULT 0 ,
	[isparastart] BIT NOT NULL DEFAULT 0 ,
	[isheader] BIT NOT NULL DEFAULT 0 ,
	[hasdate] BIT NOT NULL DEFAULT 0,
	[hasnames] SMALLINT NOT NULL DEFAULT 0,
	[hasnumbers] SMALLINT NOT NULL DEFAULT 0,
	[hasquote] BIT NOT NULL DEFAULT 0,
	[importance] SMALLINT NOT NULL DEFAULT 0,
    [sentence] NVARCHAR(MAX) NULL
)
