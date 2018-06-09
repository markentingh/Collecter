CREATE PROCEDURE [dbo].[Articles_GetListForSubjects]
	@subjectIds nvarchar(MAX),
	@search nvarchar(MAX),
	@isActive int = 2,
	@isDeleted bit = 0,
	@minImages int = 0,
	@dateStart nvarchar(50),
	@dateEnd nvarchar(50),
	@orderby int = 1,
	@start int = 1,
	@length int = 10,
	@subjectStart int = 1,
	@subjectLength int = 10,
	@bugsonly bit = 0
AS
	
	/* create results table */
	DECLARE @results TABLE(
		rownum int,
		feedId INT NULL DEFAULT 0, 
		articleId INT NULL DEFAULT 0, 
		subjects TINYINT NULL DEFAULT 0,
		images TINYINT NULL DEFAULT 0, 
		filesize FLOAT NULL DEFAULT 0,
		wordcount INT NULL DEFAULT 0, 
		sentencecount SMALLINT NULL DEFAULT 0, 
		paragraphcount SMALLINT NULL DEFAULT 0,
		importantcount SMALLINT NULL DEFAULT 0, 
		analyzecount SMALLINT NULL DEFAULT 0,
		yearstart SMALLINT NULL, 
		yearend SMALLINT NULL, 
		years NVARCHAR(50),
		datecreated DATETIME NULL, 
		datepublished DATETIME NULL, 
		relavance SMALLINT NULL DEFAULT 0, 
		importance SMALLINT NULL DEFAULT 0, 
		fiction SMALLINT NULL DEFAULT 1, 
		domain NVARCHAR(50) NULL DEFAULT '', 
		url NVARCHAR(250) NULL DEFAULT '', 
		title NVARCHAR(250) NULL DEFAULT '', 
		summary NVARCHAR(250) NULL DEFAULT '',
		breadcrumb NVARCHAR(250) NULL DEFAULT '',
		hierarchy NVARCHAR(50) NULL DEFAULT '',
		subjectId INT NULL DEFAULT 0,
		issubject BIT NULL DEFAULT 0,
		subjectTitle NVARCHAR(50) NULL DEFAULT '',
		score INT NULL DEFAULT 0,
		analyzed FLOAT NULL DEFAULT 0,
		cached BIT NULL DEFAULT 0, 
		active BIT NULL DEFAULT 0, 
		deleted BIT NULL DEFAULT 0, 
		bugsopen SMALLINT NULL DEFAULT 0, 
		bugsresolved SMALLINT NULL DEFAULT 0
	)

	DECLARE 
	@cursor1 CURSOR, 
	@cursor2 CURSOR, 
	@rownum int,
	@feedId int,
	@articleId INT,
	@subjects TINYINT,
    @images TINYINT, 
	@filesize FLOAT,
    @wordcount INT, 
    @sentencecount SMALLINT, 
    @paragraphcount SMALLINT,
    @importantcount SMALLINT, 
	@analyzecount SMALLINT,
    @yearstart SMALLINT, 
    @yearend SMALLINT, 
	@years NVARCHAR(50),
    @datecreated DATETIME, 
    @datepublished DATETIME, 
    @relavance SMALLINT, 
    @importance SMALLINT, 
    @fiction SMALLINT, 
    @domain NVARCHAR(50), 
    @url NVARCHAR(250), 
    @title NVARCHAR(250), 
    @summary NVARCHAR(250),
	@breadcrumb NVARCHAR(500),
	@hierarchy NVARCHAR(50),
	@subjectId INT,
	@subjectTitle nvarchar(50),
	@score INT,
	@analyzed FLOAT,
	@cached BIT, 
    @active BIT, 
    @deleted BIT, 
    @bugsopen SMALLINT, 
    @bugsresolved SMALLINT

	/* set default dates */
	IF (@dateStart IS NULL OR @dateStart = '') BEGIN SET @dateStart = DATEADD(YEAR, -100, GETDATE()) END
	IF (@dateEnd IS NULL OR @dateEnd = '') BEGIN SET @dateEnd = DATEADD(YEAR, 100, GETDATE()) END

	/* get subjects from array */

	SELECT subjectId, title INTO #subjects FROM 
		(SELECT ROW_NUMBER() OVER(ORDER BY breadcrumb) AS rownum, subjectId, title FROM Subjects
		WHERE subjectId IN (SELECT valueInt FROM dbo.SplitArray(@subjectIds, ',')) 
		OR subjectId = CASE WHEN @subjectIds = '' THEN subjectId ELSE -1 END) AS tbl
	WHERE rownum >= @subjectStart AND rownum < @subjectStart + @subjectLength
	
	/* get articles that match a search term */
	SELECT * INTO #search FROM dbo.SplitArray(@search, ',')
	SELECT wordId INTO #wordids FROM Words WHERE word IN (SELECT value FROM #search)
	SELECT articleId INTO #searchedarticles FROM ArticleWords WHERE wordId IN (SELECT * FROM #wordids)
	
	/* first, get subjects list //////////////////////////////////////////////////////////////////////////////////////////// */
	SET @cursor1 = CURSOR FOR
	SELECT * FROM #subjects
	OPEN @cursor1
	FETCH FROM @cursor1 INTO
	@subjectId, @subjectTitle

	WHILE @@FETCH_STATUS = 0 BEGIN
		/* get a list of subjects */
		INSERT INTO @results (subjectId, issubject, subjectTitle)
		VALUES (@subjectId, 1, @subjectTitle)
		
		FETCH FROM @cursor1 INTO
		@subjectId, @subjectTitle
	END
	CLOSE @cursor1
	DEALLOCATE @cursor1

	/* next, loop through subjects list to get articles for each subject ////////////////////////////////////////////////////// */
	SET @cursor1 = CURSOR FOR
	SELECT subjectId FROM #subjects
	OPEN @cursor1
	FETCH FROM @cursor1 INTO @subjectId

	WHILE @@FETCH_STATUS = 0 BEGIN
		/* get 10 articles for each subject */
		SET @cursor2 = CURSOR FOR
		SELECT * FROM (
			SELECT ROW_NUMBER() OVER(ORDER BY 
			CASE WHEN @orderby = 1 THEN a.datecreated END ASC,
			CASE WHEN @orderby = 2 THEN a.datecreated END DESC,
			CASE WHEN @orderby = 3 THEN a.score END ASC,
			CASE WHEN @orderby = 4 THEN a.score END DESC
			) AS rownum, a.*,
			(SELECT COUNT(*) FROM ArticleBugs WHERE articleId=a.articleId AND status=0) AS bugsopen,
			(SELECT COUNT(*) FROM ArticleBugs WHERE articleId=a.articleId AND status=1) AS bugsresolved,
			s.breadcrumb, s.hierarchy, s.title AS subjectTitle
			FROM Articles a 
			LEFT JOIN ArticleSubjects asub ON asub.articleId=a.articleId AND asub.subjectId=a.subjectId
			LEFT JOIN Subjects s ON s.subjectId=a.subjectId
			WHERE a.subjectId=@subjectId
			AND 
			(
				a.articleId IN (SELECT * FROM #searchedarticles)
				OR a.articleId = CASE WHEN @subjectIds = '' THEN a.articleId ELSE 0 END
			) 
			AND a.active = CASE WHEN @isActive = 2 THEN a.active ELSE @isActive END
			AND a.deleted=@isDeleted
			AND a.images >= @minImages
			AND a.datecreated >= CONVERT(datetime, @dateStart) AND a.datecreated <= CONVERT(datetime, @dateEnd)

			AND (
				a.articleId = CASE 
				WHEN @search = '' THEN a.articleId 
				ELSE (SELECT articleId FROM #searchedarticles WHERE articleId=a.articleId)
				END
				OR a.title LIKE CASE WHEN @search <> '' THEN '%' + @search + '%' ELSE '_81!!{}!' END /*either return search term or find an impossibly random text*/
				OR a.summary LIKE CASE WHEN @search <> '' THEN '%' + @search + '%' ELSE '_81!!{}!' END /*either return search term or find an impossibly random text*/
				)
			AND a.articleId = CASE 
			WHEN @bugsonly=1 THEN (SELECT TOP 1 articleId FROM ArticleBugs WHERE articleId=a.articleId) 
			ELSE a.articleId END
		) AS tbl WHERE rownum >= @start AND rownum < @start + @length
		OPEN @cursor2
		FETCH FROM @cursor2 INTO
		@rownum, @articleId, @feedId, @subjects, @subjectId, @score, @images, @filesize, @wordcount, @sentencecount, 
		@paragraphcount, @importantcount, @analyzecount, @yearstart, @yearend, @years, @datecreated, @datepublished, 
		@relavance, @importance, @fiction, @domain, @url, @title, @summary, @analyzed, @cached, @active, @deleted,
		@bugsopen, @bugsresolved, @breadcrumb, @hierarchy, @subjectTitle

		WHILE @@FETCH_STATUS = 0 BEGIN
			INSERT INTO @results (rownum, articleId, feedId, subjects, subjectId, score, images, filesize, wordcount, sentencecount, 
			paragraphcount, importantcount, analyzecount, yearstart, yearend, years, datecreated, datepublished, 
			relavance, importance, fiction, domain, url, title, summary, analyzed, cached,  active, deleted,
			bugsopen, bugsresolved, breadcrumb, hierarchy, subjectTitle)
			VALUES (@rownum, @articleId, @feedId, @subjects, @subjectId, @score, @images, @filesize, @wordcount, @sentencecount, 
			@paragraphcount, @importantcount, @analyzecount, @yearstart, @yearend, @years, @datecreated, @datepublished, 
			@relavance, @importance, @fiction, @domain, @url, @title, @summary, @analyzed, @cached, @active, @deleted,
			@bugsopen, @bugsresolved, @breadcrumb, @hierarchy, @subjectTitle)

			FETCH FROM @cursor2 INTO
			@rownum, @articleId, @feedId, @subjects, @subjectId, @score, @images, @filesize, @wordcount, @sentencecount, 
			@paragraphcount, @importantcount, @analyzecount, @yearstart, @yearend, @years, @datecreated, @datepublished, 
			@relavance, @importance, @fiction, @domain, @url, @title, @summary, @analyzed, @cached, @active, @deleted,
		@bugsopen, @bugsresolved, @breadcrumb, @hierarchy, @subjectTitle
		END
		CLOSE @cursor2
		DEALLOCATE @cursor2
		
		FETCH FROM @cursor1 INTO @subjectId
	END
	CLOSE @cursor1
	DEALLOCATE @cursor1

	SELECT * FROM @results ORDER BY issubject DESC, subjectTitle ASC
