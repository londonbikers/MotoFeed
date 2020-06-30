IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AO_UpdateItem')
	BEGIN
		DROP Procedure AO_UpdateItem
	END
GO

CREATE Procedure dbo.AO_UpdateItem
(
	@ID bigint,
	@SourceID int,
	@ImportTime datetime,
	@PublicationTime datetime,
	@Title nvarchar(512),
	@Description ntext,
	@ContentUrl varchar(512),
	@CategoryID int,
	@ImageFilename varchar(512),
	@Tags text
)
AS
	UPDATE
		Items
		SET
		SourceID = @SourceID,
		ImportTime = @ImportTime,
		PublicationTime = @PublicationTime,
		Title = @Title,
		Description = @Description,
		ContentUrl = @ContentUrl,
		CategoryID = @CategoryID,
		ImageFilename = @ImageFilename,
		Tags = @Tags
GO

GRANT EXEC ON AO_UpdateItem TO PUBLIC
GO