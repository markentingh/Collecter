CREATE PROCEDURE [dbo].[Topics_GetList]
	@subjectIds nvarchar(MAX),
	@search nvarchar(MAX),
	@dateStart nvarchar(50),
	@dateEnd nvarchar(50),
	@orderby int = 1,
	@start int = 1,
	@length int = 50
AS
	/* set default dates */
	IF (@dateStart IS NULL) BEGIN SET @dateStart = DATEADD(YEAR, -100, GETDATE()) END
	IF (@dateEnd IS NULL) BEGIN SET @dateEnd = DATEADD(YEAR, 100, GETDATE()) END

	/* get subjects from array */
	DECLARE @subjectId int = 0
	SELECT CONVERT(int, [value]) AS subjectId INTO #subjects FROM dbo.SplitArray(@subjectIds, ',')
	SELECT topicId INTO #subjecttopics FROM TopicSubjects WHERE subjectId IN (SELECT * FROM #subjects)

	/* get most important subject for this topic */
	SELECT @subjectId = subjectId FROM (
		SELECT TOP 1 subjectId, score, hierarchy FROM Subjects 
		WHERE subjectId IN (SELECT * FROM #subjects) 
		ORDER BY score DESC, hierarchy ASC
	) AS tbl
	
	/* get list of topics */
	SELECT * FROM (
		SELECT ROW_NUMBER() OVER(ORDER BY 
		CASE WHEN @orderby = 1 THEN t.datecreated END ASC,
		CASE WHEN @orderby = 2 THEN t.datecreated END DESC
		) AS rownum, t.* , s.title AS subjectTitle, s.breadcrumb, s.hierarchy, s.parentId
		FROM Topics t
		LEFT JOIN Subjects s ON s.subjectId = @subjectId
		WHERE
		(
			t.topicId IN (SELECT * FROM #subjecttopics)
			OR t.topicId = CASE WHEN @subjectIds = '' THEN t.topicId ELSE 0 END
		)
		AND t.datecreated >= CONVERT(datetime, @dateStart) AND t.datecreated <= CONVERT(datetime, @dateEnd)
	) AS tbl WHERE rownum >= @start AND rownum < @start + @length