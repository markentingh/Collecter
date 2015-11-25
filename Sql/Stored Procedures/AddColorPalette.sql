CREATE PROCEDURE [dbo].[AddColorPalette]
	@name nvarchar(25) = '',
	@orgId int = 0,
	@templateId int = 0,
	@blockId int = 0,
	@palette nvarchar(MAX)
AS
	DECLARE
		@date datetime = GETDATE(),
		@colorId int = NEXT VALUE FOR SequenceColors

	INSERT INTO ColorPalettes 
	(paletteId, orgId, templateId, blockId, name, palette, datecreated)
	VALUES (@colorId, @orgId, @templateId, @blockId, @name, @palette, @date)
