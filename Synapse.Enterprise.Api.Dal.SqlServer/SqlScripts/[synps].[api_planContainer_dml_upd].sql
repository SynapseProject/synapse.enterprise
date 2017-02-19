SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 18 Feb 2017
-- Description:	PlanContainer Update
-- =============================================
ALTER PROCEDURE [synps].[api_planContainer_dml_upd]
	@PlanContainerUId uniqueidentifier
	,@Name varchar(250)
	,@Description varchar(500) = null
	,@NodeUri varchar(250)
	,@RlsOwner uniqueidentifier = null
	,@RlsMask varbinary(max) = null
	,@ParentUId uniqueidentifier = null
	,@AuditModifiedBy varchar(50)
AS
BEGIN

	UPDATE [synps].[PlanContainer]
	   SET [PlanContainerUId] = @PlanContainerUId
		  ,[Name] = @Name
		  ,[Description] = @Description
		  ,[NodeUri] = @NodeUri
		  ,[RlsOwner] = @RlsOwner
		  ,[RlsMask] = @RlsMask
		  ,[ParentUId] = @ParentUId
		  ,[AuditModifiedBy] = @AuditModifiedBy
		  ,[AuditModifiedTime] = GETUTCDATE()
	   WHERE [PlanContainerUId] = @PlanContainerUId

END