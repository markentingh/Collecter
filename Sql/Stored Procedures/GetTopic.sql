CREATE PROCEDURE [dbo].[GetTopic]
	@topicId int = 0
AS
	SELECT t.*, s.breadcrumb, s.hierarchy, s.title AS subjectTitle FROM Topics t 
	LEFT JOIN Subjects s ON s.subjectId=t.subjectId
	WHERE t.topicId=@topicId
RETURN 0
