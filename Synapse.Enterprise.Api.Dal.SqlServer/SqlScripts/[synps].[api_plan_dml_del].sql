SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Shortt
-- Create date: 18 Feb 2017
-- Description:	Plan Delete
-- =============================================
ALTER PROCEDURE [synps].[api_plan_dml_del]
	@PlanUId uniqueidentifier
AS
BEGIN

	DELETE FROM [synps].[Plan] WHERE [PlanUId] = @PlanUId

END

GO