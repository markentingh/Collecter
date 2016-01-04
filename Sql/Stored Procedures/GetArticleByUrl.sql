CREATE PROCEDURE [dbo].[GetArticleByUrl]
	@url nvarchar(250)
AS
	SELECT * FROM Articles WHERE url=@url
RETURN 0
