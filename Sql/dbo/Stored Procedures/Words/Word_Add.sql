CREATE PROCEDURE [dbo].[Word_Add]
	@word nvarchar(50),
	@subjectId int = 0,
	@grammartype int = 0,
	@score int = 1
AS
	IF(SELECT COUNT(*) FROM Words WHERE word=@word AND grammartype=@grammartype) > 0 BEGIN
		/* word exists */
		DECLARE @subjects nvarchar(50)
		SELECT @subjects=subjects FROM words WHERE word=@word AND grammartype=@grammartype
		SET @subjects = @subjects + ', ' + CONVERT(nvarchar(50), @subjectId)
		UPDATE Words SET subjects=@subjects WHERE word=@word AND grammartype=@grammartype
	END ELSE BEGIN
		/* create word */
		INSERT INTO Words (wordId, word, subjects, grammartype, score) 
		VALUES (NEXT VALUE FOR SequenceWords, @word, CONVERT(nvarchar(50), @subjectId), @grammartype, @score)
	END
RETURN 0
