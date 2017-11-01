CREATE PROCEDURE [dbo].[Topic_AddByBreadcrumb]
	@title nvarchar(250),
	@summary nvarchar(MAX) = '',
	@media nvarchar(MAX) = '',
	@breadcrumb nvarchar(MAX) = '',
	@subject nvarchar(100) = ''

AS
	DECLARE @topicId int = NEXT VALUE FOR SequenceTopics
	DECLARE @subjectId int
	SELECT @subjectId=subjectId FROM Subjects WHERE breadcrumb = @breadcrumb AND title=@subject

	EXEC AddTopic @subjectId=@subjectId, @title=@title, @summary=@summary, @media=@media