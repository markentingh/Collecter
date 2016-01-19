CREATE PROCEDURE [dbo].[GetSubjectById]
	@subjectId int
AS
SELECT * FROM Subjects WHERE subjectId=@subjectId
