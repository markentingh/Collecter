CREATE PROCEDURE [dbo].[GetArticlesForFeeds]
	@subjectIds nvarchar(MAX),
	@search nvarchar(MAX),
	@isActive int = 2,
	@isDeleted bit = 0,
	@minImages int = 0,
	@dateStart nvarchar(50),
	@dateEnd nvarchar(50),
	@orderby int = 1,
	@start int = 1,
	@length int = 10
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
	SELECT wordId INTO #wordids FROM Words WHERE word IN (SELECT value FROM #search)
	SELECT articleId INTO #searchedarticles FROM ArticleWords
	WHERE wordId IN (SELECT * FROM #wordids)

	/* create results table */
	DECLARE @results TABLE(
		rownum int,
		feedId INT NULL DEFAULT 0, 
		isfeed BIT NULL DEFAULT 0,
		feedTitle NVARCHAR(100) NULL DEFAULT '', 
		feedUrl NVARCHAR(100) NULL DEFAULT '', 
		feedLastChecked DATETIME NULL, 
		feedFilter NVARCHAR(MAX) NULL DEFAULT '',
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
		analyzed FLOAT NULL DEFAULT 0,
		cached BIT NULL DEFAULT 0, 
		active BIT NULL DEFAULT 0, 
		deleted BIT NULL DEFAULT 0
	)

	DECLARE 
	@cursor1 CURSOR, 
	@cursor2 CURSOR, 
	@rownum int,
	@feedId int,
	@feedId2 int,
	@feedTitle nvarchar(100),
	@feedUrl nvarchar(100),
	@feedLastChecked datetime,
	@feedFilter nvarchar(MAX),
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
	@analyzed FLOAT,
	@cached BIT, 
    @active BIT, 
    @deleted BIT 
	
	/* first, get feeds list //////////////////////////////////////////////////////////////////////////////////////////// */
	SET @cursor1 = CURSOR FOR
	SELECT * FROM Feeds
	OPEN @cursor1
	FETCH FROM @cursor1 INTO
	@feedId, @feedTitle, @feedUrl, @feedLastChecked, @feedFilter

	WHILE @@FETCH_STATUS = 0 BEGIN
		/* get 10 articles for each feed */
		INSERT INTO @results (feedId, isfeed, feedTitle, feedUrl, feedLastChecked, feedFilter)
		VALUES (@feedId, 1, @feedTitle, @feedUrl, @feedLastChecked, @feedFilter)
		
		FETCH FROM @cursor1 INTO
		@feedId, @feedTitle, @feedUrl, @feedLastChecked, @feedFilter
	END
	CLOSE @cursor1
	DEALLOCATE @cursor1

	/* next, loop through feeds list to get articles for each feed ////////////////////////////////////////////////////// */
	SET @cursor1 = CURSOR FOR
	SELECT feedId FROM Feeds
	OPEN @cursor1
	FETCH FROM @cursor1 INTO @feedId

	WHILE @@FETCH_STATUS = 0 BEGIN
		/* get 10 articles for each feed */
		SET @cursor2 = CURSOR FOR
		SELECT * FROM (
			SELECT ROW_NUMBER() OVER(ORDER BY 
			CASE WHEN @orderby = 1 THEN a.datecreated END ASC,
			CASE WHEN @orderby = 2 THEN a.datecreated END DESC
			) AS rownum, a.* 
			FROM Articles a 
			WHERE feedId=@feedId
			AND 
			(
				articleId IN (SELECT * FROM #subjectarticles)
				OR articleId IN (SELECT * FROM #searchedarticles)
				OR articleId = CASE WHEN @subjectIds = '' THEN articleId ELSE 0 END
			) 
			AND active = CASE WHEN @isActive = 2 THEN active ELSE @isActive END
			AND deleted=@isDeleted
			AND images >= @minImages
			AND datecreated >= CONVERT(datetime, @dateStart) AND datecreated <= CONVERT(datetime, @dateEnd)
		) AS tbl WHERE rownum >= @start AND rownum <= @start + @length - 1
		OPEN @cursor2
		FETCH FROM @cursor2 INTO
		@rownum, @articleId, @feedId2, @subjects, @images, @filesize, @wordcount, @sentencecount, 
		@paragraphcount, @importantcount, @analyzecount, @yearstart, @yearend, @years, @datecreated, @datepublished, 
		@relavance, @importance, @fiction, @domain, @url, @title, @summary, @analyzed, @cached, @active, @deleted

		WHILE @@FETCH_STATUS = 0 BEGIN
			INSERT INTO @results (rownum, articleId, feedId, subjects, images, filesize, wordcount, sentencecount, 
			paragraphcount, importantcount, analyzecount, yearstart, yearend, years, datecreated, datepublished, 
			relavance, importance, fiction, domain, url, title, summary, analyzed, cached,  active, deleted)
			VALUES (@rownum, @articleId, @feedId, @subjects, @images, @filesize, @wordcount, @sentencecount, 
			@paragraphcount, @importantcount, @analyzecount, @yearstart, @yearend, @years, @datecreated, @datepublished, 
			@relavance, @importance, @fiction, @domain, @url, @title, @summary, @analyzed, @cached, @active, @deleted)

			FETCH FROM @cursor2 INTO
			@rownum, @articleId, @feedId2, @subjects, @images, @filesize, @wordcount, @sentencecount, 
			@paragraphcount, @importantcount, @analyzecount, @yearstart, @yearend, @years, @datecreated, @datepublished, 
			@relavance, @importance, @fiction, @domain, @url, @title, @summary, @analyzed, @cached, @active, @deleted
		END
		CLOSE @cursor2
		DEALLOCATE @cursor2
		
		FETCH FROM @cursor1 INTO @feedId
	END
	CLOSE @cursor1
	DEALLOCATE @cursor1

	SELECT * FROM @results ORDER BY isfeed DESC, feedId ASC, articleId ASC
