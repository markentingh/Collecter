CREATE PROCEDURE [dbo].[Topic_CreateFromBreadcrumb]
	@breadcrumb nvarchar(MAX) = '', /* e.g. Science>Physics>Quantum Physics */
	@subject nvarchar(100) = '', /* e.g. String Theory */
	@geolat float = 0,
	@geolong float = 0,
	@title nvarchar(250),
	@location nvarchar(250) = '',
	@summary nvarchar(MAX) = '',
	@media nvarchar(MAX) = ''

AS
	DECLARE @topicId int = NEXT VALUE FOR SequenceTopics
	DECLARE @subjectId int
	SELECT @subjectId=subjectId FROM Subjects WHERE breadcrumb = @breadcrumb AND title=@subject

	DECLARE @tmp TABLE (id int)

	INSERT INTO @tmp EXEC Topic_Create @subjectId=@subjectId, @title=@title, @summary=@summary, @media=@media
	SELECT id FROM @tmp AS topicId