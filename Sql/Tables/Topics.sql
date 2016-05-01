CREATE TABLE [dbo].[Topics]
(
	[topicId] INT NOT NULL PRIMARY KEY, 
    [subjectId] INT NULL, 
    [datecreated] DATETIME NULL, 
    [title] NVARCHAR(250) NULL, 
    [summary] NVARCHAR(MAX) NULL, 
    [media] NVARCHAR(MAX) NULL
)
