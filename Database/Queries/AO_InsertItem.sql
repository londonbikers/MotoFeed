IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AO_InsertItem')
	BEGIN
		DROP Procedure AO_InsertItem
	END
GO

CREATE Procedure dbo.AO_InsertItem
(
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
	INSERT INTO
		Items
		(
			SourceID,
			ImportTime,
			PublicationTime,
			Title,
			Description,
			ContentUrl,
			CategoryID,
			ImageFilename,
			Tags
		)
		VALUES
		(
			@SourceID,
			@ImportTime,
			@PublicationTime,
			@Title,
			@Description,
			@ContentUrl,
			@CategoryID,
			@ImageFilename,
			@Tags
		)
		
		SELECT SCOPE_IDENTITY()
GO

GRANT EXEC ON AO_InsertItem TO PUBLIC
GO