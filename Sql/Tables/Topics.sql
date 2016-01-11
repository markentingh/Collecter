CREATE TABLE [dbo].[Topics]
(
	[topicId] INT NOT NULL PRIMARY KEY, 
    [subjectId] INT NULL, 
    [title] NVARCHAR(250) NULL, 
    [datecreated] DATETIME NULL, 
    [summary] NVARCHAR(MAX) NULL
)
