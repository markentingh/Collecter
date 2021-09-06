CREATE PROCEDURE [dbo].[Feeds_Check]
	
AS
	SELECT f.*, c.title AS category
	FROM Feeds f 
	JOIN FeedCategories c ON c.categoryId = f.categoryId
	WHERE f.lastChecked < DATEADD(HOUR, -24, GETUTCDATE())
