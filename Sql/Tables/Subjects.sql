CREATE TABLE [dbo].[Subjects]
(
	[subjectId] INT NOT NULL PRIMARY KEY, 
    [parentId] INT NULL, 
    [title] NVARCHAR(50) NULL, 
    [breadcrumb] NVARCHAR(MAX) NULL, 
    [hierarchy] NVARCHAR(50) NULL
)
