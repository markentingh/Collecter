CREATE PROCEDURE [dbo].[Topic_Create]
	@subjectId int = 0,
	@geolat float = 0,
	@geolong float = 0,
	@title nvarchar(250),
	@location nvarchar(250) = '',
	@summary nvarchar(MAX) = '',
	@media nvarchar(MAX) = ''

AS
	DECLARE @topicId int = NEXT VALUE FOR SequenceTopics
	INSERT INTO Topics (topicId, geolat, geolong, title, [location], datecreated, summary, media)
	VALUES (@topicId, @geolat, @geolong, @title, @location, GETDATE(), @summary, @media)

	INSERT INTO TopicSubjects (topicId, subjectId) VALUES (@topicId, @subjectId)

	SELECT @topicId AS topicId