IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'Reaper_IsItemNew')
	BEGIN
		DROP Procedure Reaper_IsItemNew
	END
GO

CREATE Procedure Reaper_IsItemNew
(
	@Link varchar(512)
)
AS
	IF (SELECT COUNT(0) FROM Items WHERE ContentUrl = @Link) = 0
	BEGIN
		SELECT CAST(1 AS BIT)
	END
	ELSE
	BEGIN
		SELECT CAST(0 AS BIT)
	END
GO