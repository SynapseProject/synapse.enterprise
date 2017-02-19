SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 18 Feb 2017
-- Description:	PlanContainer Select
-- =============================================
ALTER PROCEDURE [synps].[api_planContainer_sel]
	@PlanContainerUId uniqueidentifier = null
	,@Name varchar(250) = null
	,@NodeUri varchar(250) = null
	,@AuditCreatedBy varchar(50) = null
	,@AuditCreatedTime datetime = null
	,@AuditModifiedBy varchar(50) = null
	,@AuditModifiedTime datetime = null
AS
BEGIN

SELECT [PlanContainerUId]
      ,[Name]
      ,[Description]
      ,[NodeUri]
      ,[RlsMask]
      ,[RlsOwner]
      ,[ParentUId]
	  ,[AuditCreatedBy]
	  ,[AuditCreatedTime]
	  ,[AuditModifiedBy]
	  ,[AuditModifiedTime]
  FROM [synps].[PlanContainer]
  WHERE
		[PlanContainerUId] = COALESCE(@PlanContainerUId,[PlanContainerUId])
		AND [Name] = COALESCE(@Name,[Name])
		AND [NodeUri] = COALESCE(@NodeUri,[NodeUri])
		AND [AuditCreatedBy] = COALESCE(@AuditCreatedBy,[AuditCreatedBy])
		AND [AuditCreatedTime] = COALESCE(@AuditCreatedTime,[AuditCreatedTime])
		AND [AuditModifiedBy] = COALESCE(@AuditModifiedBy,[AuditModifiedBy])
		AND [AuditModifiedTime] = COALESCE(@AuditModifiedTime,[AuditModifiedTime])

END