SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 18 Feb 2017
-- Description:	Plan Select
-- =============================================
ALTER PROCEDURE [synps].[api_plan_sel]
	@PlanUId uniqueidentifier = null
	,@Name varchar(250) = null
	,@UniqueName varchar(250) = null
	,@IsActive bit = null
	,@PlanFile varchar(max) = null
	,@PlanFileIsUri bit = null
	,@PlanContainerUId uniqueidentifier = null
	,@AuditCreatedBy varchar(50) = null
	,@AuditCreatedTime datetime = null
	,@AuditModifiedBy varchar(50) = null
	,@AuditModifiedTime datetime = null
AS
BEGIN

SELECT [PlanUId]
      ,[Name]
      ,[Description]
      ,[UniqueName]
	  ,[IsActive]
      ,[PlanFile]
      ,[PlanFileIsUri]
      ,[PlanContainerUId]
	  ,[AuditCreatedBy]
	  ,[AuditCreatedTime]
	  ,[AuditModifiedBy]
	  ,[AuditModifiedTime]
  FROM [synps].[Plan]
  WHERE
		[PlanUId] = COALESCE(@PlanUId,[PlanUId])
		AND [Name] = COALESCE(@Name,[Name])
		AND [UniqueName] = COALESCE(@UniqueName,[UniqueName])
		AND [IsActive] = COALESCE(@IsActive,[IsActive])
		AND [PlanFile] = COALESCE(@PlanFile,[PlanFile])
		AND [PlanFileIsUri] = COALESCE(@PlanFileIsUri,[PlanFileIsUri])
		AND [PlanContainerUId] = COALESCE(@PlanContainerUId,[PlanContainerUId])
		AND [AuditCreatedBy] = COALESCE(@AuditCreatedBy,[AuditCreatedBy])
		AND [AuditCreatedTime] = COALESCE(@AuditCreatedTime,[AuditCreatedTime])
		AND [AuditModifiedBy] = COALESCE(@AuditModifiedBy,[AuditModifiedBy])
		AND [AuditModifiedTime] = COALESCE(@AuditModifiedTime,[AuditModifiedTime])

END