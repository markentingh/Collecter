CREATE PROCEDURE [dbo].[GetSubjects]
	@subjectIds nvarchar(MAX)
AS
SELECT * INTO #subjects FROM dbo.SplitArray(@subjectIds, ',')
SELECT * FROM Subjects WHERE subjectId IN (SELECT CONVERT(int, value) FROM #subjects)
