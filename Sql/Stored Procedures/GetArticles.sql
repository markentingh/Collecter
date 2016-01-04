CREATE PROCEDURE [dbo].[GetArticles]
	@subjectIds nvarchar(MAX),
	@search nvarchar(MAX),
	@isActive int = 2,
	@isDeleted bit = 0,
	@minImages int = 0,
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
	SELECT * INTO #subjects FROM dbo.SplitArray(@subjectIds, ',')
	SELECT articleId INTO #subjectarticles FROM ArticleSubjects
	WHERE subjectId IN (SELECT CONVERT(int, value) FROM #subjects)
	AND datecreated >= CONVERT(datetime, @dateStart) AND datecreated <= CONVERT(datetime, @dateEnd)

	/* get articles that match a search term */
	SELECT * INTO #search FROM dbo.SplitArray(@search, ',')
	SELECT wordid INTO #wordids FROM Words WHERE word IN (SELECT value FROM #search)
	SELECT articleId INTO #searchedarticles FROM ArticleWords
	WHERE wordId IN (SELECT * FROM #wordids)



	SELECT * FROM (
		SELECT ROW_NUMBER() OVER(ORDER BY 
		CASE WHEN @orderby = 1 THEN a.datecreated END ASC,
		CASE WHEN @orderby = 2 THEN a.datecreated END DESC
		) AS rownum, a.* 
		FROM Articles a 
		WHERE
		(
			articleId IN (SELECT * FROM #subjectarticles)
			OR articleId IN (SELECT * FROM #searchedarticles)
			OR articleId = CASE WHEN @subjectIds = '' THEN articleId ELSE 0 END
		) 
		AND active = CASE WHEN @isActive = 2 THEN active ELSE @isActive END
		AND deleted=@isDeleted
		AND images >= @minImages
		AND datecreated >= CONVERT(datetime, @dateStart) AND datecreated <= CONVERT(datetime, @dateEnd)
	) AS tbl WHERE rownum >= @start AND rownum <= @start + @length
