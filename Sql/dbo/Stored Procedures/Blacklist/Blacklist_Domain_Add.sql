CREATE PROCEDURE [dbo].[Blacklist_Domain_Add]
	@domain nvarchar(64)
AS
	DECLARE @domainId int
	BEGIN TRY
	INSERT INTO Blacklist_Domains (domain) VALUES (@domain)
	END TRY
	BEGIN CATCH
	END CATCH
	SELECT @domainId=domainId FROM DownloadDomains WHERE domain=@domain

	-- delete all articles related to domain
	DECLARE @cursor CURSOR, @articleId int
	SET @cursor = CURSOR FOR
	SELECT articleId FROM Articles WHERE url LIKE '%' + @domain + '/%'
	OPEN @cursor
	FETCH NEXT FROM @cursor INTO @articleId
	WHILE @@FETCH_STATUS = 0 BEGIN
		DELETE FROM ArticleBugs WHERE articleId=@articleId
		DELETE FROM ArticleDates WHERE articleId=@articleId
		DELETE FROM ArticleSentences WHERE articleId=@articleId
		DELETE FROM ArticleSubjects WHERE articleId=@articleId
		DELETE FROM ArticleWords WHERE articleId=@articleId
		DELETE FROM Articles WHERE articleId=@articleId
		FETCH NEXT FROM @cursor INTO @articleId
	END
	CLOSE @cursor
	DEALLOCATE @cursor

	--delete all download queue related to domain
	DELETE FROM DownloadQueue WHERE domainId=@domainId
	DELETE FROM DownloadDomains WHERE domainId=@domainId