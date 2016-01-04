CREATE PROCEDURE [dbo].[ArticleExists]
	@url nvarchar(250)
AS
	SELECT COUNT(*) FROM Articles WHERE url=@url
