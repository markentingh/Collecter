CREATE PROCEDURE [dbo].[Articles_GetList]
	@subjectIds nvarchar(MAX),
	@search nvarchar(MAX),
	@isActive int = 2,
	@isDeleted bit = 0,
	@minImages int = 0,
	@dateStart nvarchar(50),
	@dateEnd nvarchar(50),
	@orderby int = 1,
	@start int = 1,
	@length int = 50,
	@bugsonly bit = 0
AS
	/* set default dates */
	IF (@dateStart IS NULL OR @dateStart = '') BEGIN SET @dateStart = DATEADD(YEAR, -100, GETDATE()) END
	IF (@dateEnd IS NULL OR @dateEnd = '') BEGIN SET @dateEnd = DATEADD(YEAR, 100, GETDATE()) END

	/* get subjects from array */
	SELECT * INTO #subjects FROM dbo.SplitArray(@subjectIds, ',')
	SELECT articleId INTO #subjectarticles FROM ArticleSubjects
	WHERE subjectId IN (SELECT CONVERT(int, value) FROM #subjects)
	AND datecreated >= CONVERT(datetime, @dateStart) AND datecreated <= CONVERT(datetime, @dateEnd)

	/* get articles that match a search term */
	SELECT * INTO #search FROM dbo.SplitArray(@search, ',')
	SELECT wordid INTO #wordids FROM Words WHERE word IN (SELECT value FROM #search)
	SELECT articleId INTO #searchedarticles FROM ArticleWords
	WHERE wordId IN (SELECT * FROM #wordids)

	/* get list of articles that match filter */
	SELECT * FROM (
		SELECT ROW_NUMBER() OVER(ORDER BY 
			CASE WHEN @orderby = 1 THEN a.datecreated END ASC,
			CASE WHEN @orderby = 2 THEN a.datecreated END DESC,
			CASE WHEN @orderby = 3 THEN a.score END ASC,
			CASE WHEN @orderby = 4 THEN a.score END DESC
		) AS rownum, a.*,
		s.breadcrumb, s.hierarchy, s.title AS subjectTitle
		FROM Articles a 
		LEFT JOIN Subjects s ON s.subjectId=a.subjectId
		WHERE
		(
			a.articleId IN (SELECT * FROM #subjectarticles)
			OR a.articleId IN (SELECT * FROM #searchedarticles)
			OR a.articleId = CASE WHEN @subjectIds = '' THEN a.articleId ELSE 0 END
			OR a.title LIKE '%' + @search + '%'
			OR a.summary LIKE '%' + @search + '%'
		) 
		AND a.active = CASE WHEN @isActive = 2 THEN a.active ELSE @isActive END
		AND a.deleted=@isDeleted
		AND a.images >= @minImages
		AND a.datecreated >= CONVERT(datetime, @dateStart) AND a.datecreated <= CONVERT(datetime, @dateEnd)
	) AS tbl WHERE rownum >= @start AND rownum < @start + @length
