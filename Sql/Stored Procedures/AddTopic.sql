CREATE PROCEDURE [dbo].[AddTopic]
	@subjectId int = 0,
	@geolat float = 0,
	@geolong float = 0,
	@title nvarchar(250),
	@location nvarchar(250),
	@summary nvarchar(MAX),
	@media nvarchar(MAX) = ''

AS
	DECLARE @topicId int = NEXT VALUE FOR SequenceTopics
	INSERT INTO Topics (topicId, subjectId, geolat, geolong, title, [location], datecreated, summary, media)
	VALUES (@topicId, @subjectId, @geolat, @geolong, @title, @location, GETDATE(), @summary, @media)