CREATE PROCEDURE [dbo].[Topic_GetDetails]
	@topicId int = 0
AS
	/* get most important subject for this topic */
	DECLARE @subjectId int = 0
	SELECT TOP 1 @subjectId = subjectId FROM Subjects 
	WHERE subjectId IN (SELECT subjectId FROM TopicSubjects WHERE topicId=@topicId) 
	ORDER BY score DESC, hierarchy ASC
	
	/* get info for topic */
	SELECT t.*, s.breadcrumb, s.hierarchy, s.title AS subjectTitle, @subjectId AS subjectId
	FROM Topics t LEFT JOIN Subjects s ON s.subjectId=@subjectId
	WHERE t.topicId=@topicId
RETURN 0
