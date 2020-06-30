IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'AO_GetAllCategories')
	BEGIN
		DROP Procedure AO_GetAllCategories
	END
GO

CREATE Procedure dbo.AO_GetAllCategories
AS
	SELECT
		*
		FROM
		Categories
GO

GRANT EXEC ON AO_GetAllCategories TO PUBLIC
GO