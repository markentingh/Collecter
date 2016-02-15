CREATE PROCEDURE [dbo].[AddToDownloadQueue]
	@url nvarchar(100) = '',
	@feedId int = 0
AS
IF (SELECT COUNT(*) FROM DownloadQueue WHERE url=@url) = 0 BEGIN
IF (SELECT COUNT(*) FROM Articles WHERE url=@url) = 0 BEGIN
	DECLARE @qid INT = NEXT VALUE FOR SequenceDownloadQueue
	INSERT INTO DownloadQueue (qid, url, feedId) VALUES (@qid, @url, @feedId)
	SELECT 1
END ELSE BEGIN SELECT 0 END
END ELSE BEGIN SELECT 0 END
	