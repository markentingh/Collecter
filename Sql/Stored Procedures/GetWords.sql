CREATE PROCEDURE [dbo].[GetWords]
	@words nvarchar(MAX)
AS
SELECT * INTO #words FROM dbo.SplitArray(@words, ',')
SELECT * FROM Words WHERE word IN (SELECT value FROM #words)
