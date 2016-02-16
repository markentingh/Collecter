CREATE PROCEDURE [dbo].[GetFeedsAndLogs]
	@days int = 7,
	@dateStart date
AS
	DECLARE 
	@cursor1 CURSOR,
	@cursor2 CURSOR,
	@feedId int,
	@title nvarchar(100),
	@url nvarchar(100),
	@lastChecked datetime,
	@filter nvarchar(MAX),
	@logfeedId INT,
	@loglinks smallint,
	@logdatechecked datetime

	DECLARE @tblresults TABLE (
		feedId int NOT NULL,
		title nvarchar(100) NULL,
		url nvarchar(100) NULL,
		lastChecked datetime NULL,
		filter nvarchar(MAX) NULL,
		loglinks smallint NULL,
		logdatechecked datetime NULL
	)


	SET @cursor1 = CURSOR FOR 
	SELECT * FROM feeds
	OPEN @cursor1
	FETCH FROM @cursor1 INTO
	@feedId, @title, @url, @lastChecked, @filter
	WHILE @@FETCH_STATUS = 0 BEGIN
		/*add feed to results table */
		INSERT INTO @tblresults (feedId, title, url, lastChecked, filter)
		VALUES (@feedId, @title, @url, @lastChecked, @filter)

		/* get log data for each feed */
		SET @cursor2 = CURSOR FOR 
		SELECT * FROM FeedsCheckedLog 
		WHERE feedId=@feedId 
		AND datechecked >= @dateStart
		AND datechecked <= DATEADD(DAY, @days, @dateStart)
		ORDER BY datechecked ASC
		OPEN @cursor2
		FETCH FROM @cursor2 INTO
		@logfeedId, @loglinks, @logdatechecked
		WHILE @@FETCH_STATUS = 0 BEGIN
			/* add feed log record to results table */
			INSERT INTO @tblresults (feedId, loglinks, logdatechecked)
			VALUES(@feedId, @loglinks, @logdatechecked)

			FETCH FROM @cursor2 INTO
			@logfeedId, @loglinks, @logdatechecked
		END
		CLOSE @cursor2
		DEALLOCATE @cursor2

		FETCH FROM @cursor1 INTO
		@feedId, @title, @url, @lastChecked, @filter
	END
	CLOSE @cursor1
	DEALLOCATE @cursor1

	/* finally, return results */
	SELECT * FROM @tblresults