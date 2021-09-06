CREATE PROCEDURE [dbo].[DownloadQueue_Add]
	@urls nvarchar(MAX) = '', --comma delimited list
	@domain nvarchar(64) = '',
	@feedId int = 0
AS
SELECT * INTO #urls FROM dbo.SplitArray(@urls, ',')
DECLARE @cursor CURSOR, @url nvarchar(MAX), @domainId INT, @qid INT, @count INT = 0
IF EXISTS(SELECT * FROM DownloadDomains WHERE domain=@domain) BEGIN
	SELECT @domainId = domainId FROM DownloadDomains WHERE domain=@domain
END ELSE BEGIN
	SET @domainId = NEXT VALUE FOR SequenceDownloadDomains
	INSERT INTO DownloadDomains (domainId, domain, lastchecked) VALUES (@domainId, @domain, DATEADD(HOUR, -1, GETUTCDATE()))
END
SET @cursor = CURSOR FOR
SELECT [value] FROM #urls
OPEN @cursor
FETCH NEXT FROM @cursor INTO @url
WHILE @@FETCH_STATUS = 0 BEGIN
	IF (SELECT COUNT(*) FROM DownloadQueue WHERE url=@url) = 0 BEGIN
		IF (SELECT COUNT(*) FROM Articles WHERE url=@url) = 0 BEGIN
			SET @qid = NEXT VALUE FOR SequenceDownloadQueue
			INSERT INTO DownloadQueue (qid, url, feedId, domainId, [status], datecreated) 
			VALUES (@qid, @url, @feedId, @domainId, 0, GETDATE())
			SET @count += 1
		END
	END
	FETCH NEXT FROM @cursor INTO @url
END
CLOSE @cursor
DEALLOCATE @cursor
SELECT @count AS [count]

	