CREATE PROCEDURE [dbo].[Feeds_GetList]
AS
SELECT * FROM Feeds WHERE feedId > 0 ORDER BY title ASC
