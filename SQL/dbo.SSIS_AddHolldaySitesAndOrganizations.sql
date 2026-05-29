USE [CK_Reporting]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ==============================================================
-- Author:		Enterprise_Build_Team - Eric SCott
-- Date:		2025-09-16
-- Description: Inserts new sites into config tables for data extract to PHXDBDPW1601
-- ==============================================================
ALTER PROCEDURE [dbo].[SSIS_AddHolidaySitesAndOrganizations]
(
	@siteList varchar(max)
)
AS
BEGIN

	------------------------------------------------------------------------

	DECLARE @pdiSites TABLE (Site_ID int primary key, Site_Key decimal(15,0), Organization_Key decimal(15,0))
	DECLARE @holidaySites TABLE ( Site_ID int );
	DECLARE @newHolidaySites TABLE (Site_ID int );
	DECLARE @numPdiSites SMALLINT;
	SET @numPdiSites = 0;

	-- Get our list of sites that exist in the list but are not already in the HolidaySites table
	INSERT INTO @holidaySites
	SELECT [Value] FROM dbo._Split_String_FN( @siteList, ',')
	WHERE 
	NOT EXISTS
	(
		SELECT 1 FROM HolidaySites s WHERE [Value] = s.Site_ID
	);

	-- Now, from this list, get the sites that exist in the missing list that also exist in PDI
	INSERT INTO @pdiSites
	SELECT s.Site_ID, s.Site_Key, o.Organization_Key 
	FROM @holidaySites h
	INNER JOIN PDI_Reporting.dbo.Sites s ON h.Site_ID = s.Site_ID
	INNER JOIN PDI_Warehouse_Reporting.dbo.Organization o ON h.Site_ID = o.Location_ID;

	SET @numPdiSites = (SELECT COUNT(Site_ID) FROM @pdiSites);

	IF @numPdiSites > 0
	BEGIN

		-- Add sites to HolidayOrganization table
		INSERT INTO [dbo].[HolidayOrganization] ( [Organization_Key],[Location_ID])
		SELECT Organization_Key, Site_ID FROM @pdiSites;

		-- Add sites to HolidaySites table
		INSERT INTO [dbo].[HolidaySites] ( [Site_Key],[Site_ID] )
		SELECT Site_Key, Site_ID FROM @pdiSites;

	END

	-- Our result is the new sites we added
	SELECT Site_ID From @pdiSites;

	SET NOCOUNT ON;


	----------------------------------------------------------------------------------------
END
GO


