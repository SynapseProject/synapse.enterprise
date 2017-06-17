-- =============================================
-- Author:		Steve Shortt
-- Create date: 17 Jun 2017
-- Description:	Adjunct Suplex Helper sp
-- [synps].[api_splxElement_AceGroups_sel] @SPLX_UI_ELEMENT_ID = '5745E677-FABC-4247-905E-D82E2FD0D8C9'
-- =============================================
create PROCEDURE [synps].[api_splxElement_AceGroups_sel]

	@SPLX_UI_ELEMENT_ID	uniqueidentifier = null

AS

	SELECT
		ACES.*
		,IS_AUDIT_ACE =
			CASE 
				WHEN ACE_ACCESS_TYPE2 IS NULL THEN 0
				ELSE 1
			END
		,GROUPS.*
	FROM
		splx.SPLX_ACES ACES INNER JOIN splx.SPLX_GROUPS GROUPS 
		ON ACES.ACE_TRUSTEE_USER_GROUP_ID = GROUPS.SPLX_GROUP_ID
	WHERE
		SPLX_UI_ELEMENT_ID = @SPLX_UI_ELEMENT_ID
		and
		GROUPS.GROUP_ENABLED = 1
		and
		ACE_ACCESS_TYPE1 = 1 -- IsAllowed = true (not a Deny)
		and
		ACE_ACCESS_TYPE2 IS NULL -- is not an AuditAce

go