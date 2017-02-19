SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 18 Feb 2017
-- Description:	PlanContainer Insert
-- =============================================
ALTER PROCEDURE [synps].[api_planContainer_dml_ins]
	@PlanContainerUId uniqueidentifier = null output
	,@Name varchar(250)
	,@Description varchar(500)
	,@NodeUri varchar(250)
	,@RlsOwner uniqueidentifier
	,@RlsMask varbinary(max)
	,@ParentUId uniqueidentifier
	,@AuditCreatedBy varchar(50)
AS
BEGIN

	set @PlanContainerUId = ISNULL( @PlanContainerUId, newid() )

	INSERT INTO [synps].[PlanContainer]
			   ([PlanContainerUId]
			   ,[Name]
			   ,[Description]
			   ,[NodeUri]
			   ,[RlsOwner]
			   ,[RlsMask]
			   ,[ParentUId]
			   ,[AuditCreatedBy]
			   ,[AuditCreatedTime]
			   ,[AuditModifiedBy]
			   ,[AuditModifiedTime])
		 VALUES
		       (@PlanContainerUId
			   ,@Name
			   ,@Description
			   ,@NodeUri
			   ,@RlsOwner
			   ,@RlsMask
			   ,@ParentUId
			   ,@AuditCreatedBy
			   ,GETUTCDATE()
			   ,@AuditCreatedBy
			   ,GETUTCDATE());

	SELECT @PlanContainerUId

END