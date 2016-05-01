CREATE PROCEDURE [dbo].[AddTopicByBreadcrumb]
	@title nvarchar(250),
	@summary nvarchar(MAX) = '',
	@media nvarchar(MAX) = '',
	@breadcrumb nvarchar(MAX) = '',
	@subject nvarchar(100) = ''

AS
	DECLARE @topicId int = NEXT VALUE FOR SequenceTopics
	DECLARE @subjectId int
	SELECT @subjectId=subjectId FROM Subjects WHERE breadcrumb = @breadcrumb AND title=@subject

	INSERT INTO Topics (topicId, subjectId, title, datecreated, summary, media)
	VALUES (@topicId, @subjectId, @title, GETDATE(), @summary, @media)