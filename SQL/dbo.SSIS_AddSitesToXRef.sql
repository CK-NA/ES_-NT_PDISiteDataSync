USE [Common]
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
ALTER PROCEDURE [dbo].[SSIS_AddSitesToSiteXRef]
(
	@siteList varchar(max)
)
AS
BEGIN

	------------------------------------------------------------------------

	DECLARE @sites TABLE (Site_ID varchar(7) PRIMARY KEY);
	DECLARE @missingSites TABLE( Site_ID varchar(7) PRIMARY KEY, Holiday_Site_ID varchar(4) );
	DECLARE @numPDISites int = 0;

	SET NOCOUNT ON;

	INSERT INTO @sites
	SELECT dataValues from dbo.HLDY_SplitDelimitedString_FN(@siteList,',');

	INSERT INTO @missingSites
	SELECT 
		Site_ID, 
		CASE
			WHEN LEFT(Site_ID,4) = '2746' THEN RIGHT(Site_ID,3)	-- Holiday Corporate Site IDs are typically 3 digits 
			ELSE RIGHT(Site_ID,4)								-- Holiday Franchise site IDs are 4 digits
		END
	FROM @sites t
	-- Sanity check to ensure we don't insert dupes
	WHERE NOT EXISTS
	(
		SELECT 1 FROM dbo.SiteXRef x
		WHERE x.CK_Site_ID = t.Site_ID
	);

	-- Get our total # of missing sites
	SELECT @numPDISites = COUNT( Site_ID ) FROM @missingSites;

	IF @numPdiSites > 0
	BEGIN

		-- Insert missing sites
		INSERT INTO dbo.SiteXRef (CK_Site_ID, Holiday_Site_ID)
		SELECT m.Site_ID, m.Holiday_Site_ID
		FROM @missingSites m;

	END

	-- Our result is the number of sites we added
	SELECT @numPDISites As NumSitesInserted;

	SET NOCOUNT ON;

	----------------------------------------------------------------------------------------
END
GO


