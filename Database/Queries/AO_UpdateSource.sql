IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AO_UpdateItem')
	BEGIN
		DROP  Procedure  AO_UpdateItem
	END
GO

CREATE Procedure dbo.AO_UpdateItem
(
	@ID int,
	@Name nvarchar(255),
	@HomepageUrl varchar(512),
	@FeedUrl varchar(512),
	@LastScanTime datetime,
	@ContentTimeStamp datetime,
	@Status tinyint,
	@ImageFilename varchar(512),
	@Description ntext,
	@FailedGetCount tinyint
)
AS
	IF (SELECT COUNT(0) FROM Sources WHERE ID = @ID) > 0
	BEGIN
		UPDATE
			Sources
			SET
			[Name] = @Name,
			HomepageUrl = @HomepageUrl,
			FeedUrl = @FeedUrl,
			LastScanTime = @LastScanTime,
			ContentTimeStamp = @ContentTimeStamp,
			Status = @Status,
			ImageFilename = @ImageFilename,
			Description = @Description,
			FailedGetCount = @FailedGetCount
			WHERE
			ID = @ID
			
			SELECT @ID
	END
	ELSE
	BEGIN
		INSERT INTO
			Sources
			(
				ID,
				[Name],
				HomepageUrl,
				FeedUrl,
				LastScanTime,
				ContentTimeStamp,
				Status,
				ImageFilename,
				Description,
				FailedGetCount
			)
			VALUES
			(
				@ID,
				@Name,
				@HomepageUrl,
				@FeedUrl,
				@LastScanTime,
				@ContentTimeStamp,
				@Status,
				@ImageFilename,
				@Description,
				@FailedGetCount
			)
			
		SELECT SCOPE_IDENTITY()
	END
GO

GRANT EXEC ON AO_UpdateItem TO PUBLIC
GO