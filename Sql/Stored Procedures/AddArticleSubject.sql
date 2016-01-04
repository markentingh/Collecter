CREATE PROCEDURE [dbo].[AddArticleSubject]
	@articleId int = 0,
	@subjectId int = 0,
	@datepublished datetime = null,
	@importance int = 1
AS
	IF (SELECT COUNT(*) FROM ArticleSubjects WHERE articleId=@articleId AND subjectId=@subjectId) = 0 BEGIN
		INSERT INTO ArticleSubjects (articleId, subjectId, datecreated, datepublished, importance) 
		VALUES (@articleId, @subjectId, GETDATE(), @datepublished, @importance)
	END
