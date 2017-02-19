SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Steve Shortt
-- Create date: 18 Feb 2017
-- Description:	Plan Insert
-- =============================================
ALTER PROCEDURE [synps].[api_plan_dml_ins]
	@PlanUId uniqueidentifier = null output
	,@Name varchar(250)
	,@Description varchar(500) = null
	,@UniqueName varchar(250)
	,@IsActive bit
	,@PlanFile varchar(max)
	,@PlanFileIsUri bit = 0
	,@PlanContainerUId uniqueidentifier
	,@AuditCreatedBy varchar(50)
AS
BEGIN

	set @PlanUId = ISNULL( @PlanUId, newid() )

	INSERT INTO [synps].[Plan]
			   ([PlanUId]
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
			   ,[AuditModifiedTime])
		 VALUES
			   (@PlanUId
			   ,@Name
			   ,@Description
			   ,@UniqueName
			   ,@IsActive
			   ,@PlanFile
			   ,@PlanFileIsUri
			   ,@PlanContainerUId
			   ,@AuditCreatedBy
			   ,GETUTCDATE()
			   ,@AuditCreatedBy
			   ,GETUTCDATE());

	SELECT @PlanUId
END