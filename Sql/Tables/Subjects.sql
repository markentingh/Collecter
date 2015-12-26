CREATE TABLE [dbo].[Subjects]
(
	[subjectId] INT NOT NULL PRIMARY KEY, 
    [parentId] INT NULL, 
    [grammartype] INT NULL, 
    [score] INT NULL, 
    [title] NVARCHAR(50) NULL, 
    [hierarchy] NVARCHAR(50) NULL, 
    [breadcrumb] NVARCHAR(MAX) NULL
)
