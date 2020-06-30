 /*
   17 December 200700:24:25
   User: 
   Server: JUPITER
   Database: MotoFeed
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Sources ADD
	FailedGetCount tinyint NOT NULL CONSTRAINT DF_Sources_FailedGetCount DEFAULT 0
GO
COMMIT
