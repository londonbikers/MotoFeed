IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AO_UpdateCategory')
	BEGIN
		DROP Procedure AO_UpdateCategory
	END
GO

CREATE Procedure dbo.AO_UpdateCategory
(
	@ID int,
	@Name varchar(512),
	@Synonyms text,
	@Tags text,
	@ParentCategoryID int,
	@Status tinyint	
)
AS
	IF (SELECT COUNT(0) FROM Categories WHERE ID = @ID) > 0
	BEGIN
		UPDATE
			Categories
			SET
			[Name] = @Name,
			Synonyms = @Synonyms,
			Tags = @Tags,
			ParentCategoryID = @ParentCategoryID,
			Status = @Status
			WHERE
			ID = @ID
			
			SELECT @ID
	END
	ELSE
	BEGIN
		INSERT INTO
			Categories
			(
				[Name],
				Synonyms,
				Tags,
				ParentCategoryID,
				Status
			)
			VALUES
			(
				@Name,
				@Synonyms,
				@Tags,
				@ParentCategoryID,
				@Status
			)
			
			SELECT SCOPE_IDENTITY()
	END
GO

GRANT EXEC ON AO_UpdateCategory TO PUBLIC
GO