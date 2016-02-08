CREATE PROCEDURE [dbo].[UpdateArticle]
	@articleId int = 0,
	@subjects int = 0,
	@title nvarchar(250),
	@summary nvarchar(250),
	@images int = 0,
	@datepublished datetime,
	@relavance smallint = 1,
	@importance smallint = 1,
	@fiction smallint = 1
AS
	UPDATE Articles SET subjects=@subjects, title=@title, summary=@summary, images=@images, datepublished=@datepublished, 
						relavance=@relavance, importance=@importance, fiction=@fiction
WHERE articleId=@articleId
