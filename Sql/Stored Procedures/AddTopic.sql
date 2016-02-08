CREATE PROCEDURE [dbo].[AddTopic]
	@subjectId int = 0,
	@title nvarchar(250),
	@summary nvarchar(MAX)

AS
	DECLARE @topicId int = NEXT VALUE FOR SequenceTopics
	INSERT INTO Topics (topicId, subjectId, title, datecreated, summary)
	VALUES (@topicId, @subjectId, @title, GETDATE(), @summary)