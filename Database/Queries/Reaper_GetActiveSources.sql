IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'Reaper_GetActiveSources')
	BEGIN
		DROP  Procedure  Reaper_GetActiveSources
	END
GO

CREATE Procedure dbo.Reaper_GetActiveSources
AS
	-- gets the basic list of active sources in the system for Reaper.
	SELECT
		*
		FROM
		Sources
		WHERE
		[Status] = 1
GO

GRANT EXEC ON dbo.Reaper_GetActiveSources TO PUBLIC
GO