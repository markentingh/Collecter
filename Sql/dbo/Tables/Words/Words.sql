CREATE TABLE [dbo].[Words]
(
	[wordId] INT NOT NULL PRIMARY KEY,
	[word] NVARCHAR(50) NOT NULL, 
    [grammartype] INT NULL, 
    [score] INT NULL
)
