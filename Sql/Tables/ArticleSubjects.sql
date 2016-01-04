CREATE TABLE [dbo].[ArticleSubjects]
(
	[subjectId] INT NOT NULL, 
    [articleId] INT NULL, 
    [importance] SMALLINT NULL, 
    [datecreated] DATETIME NULL, 
    [datepublished] DATETIME NULL
)
