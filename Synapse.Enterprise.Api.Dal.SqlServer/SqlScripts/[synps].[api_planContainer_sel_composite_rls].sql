CREATE PROCEDURE [synps].[api_planContainer_sel_composite_rls]

	@PlanContainerUId	uniqueidentifier = null
	,@RlsMask			varbinary(max)

AS
BEGIN

	--make sure temp table not there, drop it if so
	IF OBJECT_ID(N'tempdb..#PlanContainers',N'U') IS NOT NULL
		DROP TABLE #PlanContainers


	CREATE TABLE #PlanContainers (
		[PlanContainerUId]  UNIQUEIDENTIFIER NOT NULL,
		[Name]              VARCHAR (250)    NOT NULL,
		[Description]       VARCHAR (500)    NULL,
		[NodeUri]           VARCHAR (250)    NULL,
		[RlsOwner]          UNIQUEIDENTIFIER NULL,
		[RlsMask]           VARBINARY (MAX)  NULL,
		[ParentUId]         UNIQUEIDENTIFIER NULL,
		[AuditCreatedBy]    VARCHAR (50)     NOT NULL,
		[AuditCreatedTime]  DATETIME         NOT NULL,
		[AuditModifiedBy]   VARCHAR (50)     NOT NULL,
		[AuditModifiedTime] DATETIME         NOT NULL
	);

	IF @PlanContainerUId IS NULL
		INSERT INTO #PlanContainers
			SELECT * FROM [synps].[PlanContainer]
			WHERE
				ParentUId IS NULL;
	ELSE
		INSERT INTO #PlanContainers
			SELECT * FROM [synps].[PlanContainer]
			WHERE
				PlanContainerUId = @PlanContainerUId;


	WITH ContainerHier( PlanContainerUId, [Name], [Description], NodeUri, RlsOwner, RlsMask, ParentUId, AuditCreatedBy, AuditCreatedTime, AuditModifiedBy, AuditModifiedTime )
	AS
	(
		SELECT * FROM #PlanContainers
				
		UNION ALL
			
		SELECT pc.* FROM [synps].[PlanContainer] pc
		INNER JOIN ContainerHier
			ON pc.ParentUId = ContainerHier.PlanContainerUId
	)
	SELECT
		ch.PlanContainerUId
		,ch.[Name] as PlanContainerName
		,ch.[Description] as PlanContainerDescription
		,ch.NodeUri
		,ch.RlsOwner
		,ch.RlsMask
		,ch.ParentUId
		,ch.AuditCreatedBy as PlanContainerAuditCreatedBy
		,ch.AuditCreatedTime as PlanContainerAuditCreatedTime
		,ch.AuditModifiedBy as PlanContainerAuditModifiedBy
		,ch.AuditModifiedTime as PlanContainerAuditModifiedTime
		,p.PlanUId
		,p.[Name] PlanName
		,p.[Description] PlanDescription
		,p.UniqueName
		,p.IsActive
		,p.PlanFile
		,p.PlanFileIsUri
		,p.AuditCreatedBy as PlanAuditCreatedBy
		,p.AuditCreatedTime as PlanAuditCreatedTime
		,p.AuditModifiedBy as PlanAuditModifiedBy
		,p.AuditModifiedTime as PlanAuditModifiedTime
	FROM ContainerHier ch
	LEFT OUTER JOIN [synps].[Plan] p
		ON ch.PlanContainerUId = p.PlanContainerUId
	WHERE
		[splx].[splx_containsone](RlsMask, @RlsMask) > 0

	--done with temp table, drop it
	IF OBJECT_ID(N'tempdb..#PlanContainers',N'U') IS NOT NULL
		DROP TABLE #PlanContainers

END