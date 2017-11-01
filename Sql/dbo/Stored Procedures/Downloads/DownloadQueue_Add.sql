CREATE PROCEDURE [dbo].[DownloadQueue_Add]
	@url nvarchar(MAX) = '',
	@feedId int = 0
AS
IF (SELECT COUNT(*) FROM DownloadQueue WHERE url=@url) = 0 BEGIN
	IF (SELECT COUNT(*) FROM Articles WHERE url=@url) = 0 BEGIN
		DECLARE @qid INT = NEXT VALUE FOR SequenceDownloadQueue
		INSERT INTO DownloadQueue (qid, rndid, url, feedId, serverId, [status], datecreated) VALUES (@qid, FLOOR(RAND() * 999999), @url, @feedId, 0, 0, GETDATE())
		SELECT 1
	END ELSE BEGIN SELECT 0 END
END ELSE BEGIN SELECT 0 END
	