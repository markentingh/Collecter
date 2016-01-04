CREATE PROCEDURE [dbo].[AddSubject]
	@parentId int = 0,
	@grammartype int = 0,
	@score int = 0,
	@title nvarchar(50),
	@breadcrumb nvarchar(MAX) = ''
AS
	DECLARE @create bit = 1, @hierarchy nvarchar(50) = ''
	IF @parentId > 0 BEGIN
		IF (SELECT COUNT(*) FROM Subjects WHERE breadcrumb = @breadcrumb AND title=@title) > 0 BEGIN
			/* subject already exists */
			SET @create = 0
		END ELSE BEGIN
			/* get hierarchy indexes */
			SELECT @hierarchy = hierarchy FROM Subjects WHERE subjectId=@parentId
			if @hierarchy <> '' BEGIN
			 SET @hierarchy = @hierarchy  + '>' + CONVERT(nvarchar(10),@parentId)
			END ELSE BEGIN
			 SET @hierarchy =  CONVERT(nvarchar(10),@parentId)
			END
		END
	END ELSE BEGIN
		IF (SELECT COUNT(*) FROM Subjects WHERE parentId=0 AND title=@title) > 0 BEGIN
			/* root subject already exists */
			SET @create = 0
		END
	END

	IF @create = 1 BEGIN
		/* finally, create subject */
		INSERT INTO Subjects (subjectId, parentId, grammartype, score, title, breadcrumb, hierarchy)
		VALUES (NEXT VALUE FOR SequenceSubjects, @parentId, @grammartype, @score, @title, @breadcrumb, @hierarchy)
	END
