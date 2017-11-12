CREATE PROCEDURE [dbo].[Topic_UpdateMedia]
	@topicId int = 0,
	@media nvarchar(MAX) = ''
AS
	UPDATE Topics SET media=@media WHERE topicId=@topicId