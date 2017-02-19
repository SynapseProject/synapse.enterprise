CREATE SCHEMA [synps]
GO

/****** Object:  Table [synps].[Plan]    Script Date: 2/19/2017 3:43:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [synps].[Plan](
	[PlanUId] [uniqueidentifier] NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[Description] [varchar](500) NULL,
	[UniqueName] [varchar](250) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[PlanFile] [varchar](max) NOT NULL,
	[PlanFileIsUri] [bit] NOT NULL,
	[PlanContainerUId] [uniqueidentifier] NOT NULL,
	[AuditCreatedBy] [varchar](50) NOT NULL,
	[AuditCreatedTime] [datetime] NOT NULL,
	[AuditModifiedBy] [varchar](50) NOT NULL,
	[AuditModifiedTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Plan] PRIMARY KEY CLUSTERED 
(
	[PlanUId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [synps].[PlanContainer]    Script Date: 2/19/2017 3:43:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [synps].[PlanContainer](
	[PlanContainerUId] [uniqueidentifier] NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[Description] [varchar](500) NULL,
	[NodeUri] [varchar](250) NULL,
	[RlsOwner] [uniqueidentifier] NULL,
	[RlsMask] [varbinary](max) NULL,
	[ParentUId] [uniqueidentifier] NULL,
	[AuditCreatedBy] [varchar](50) NOT NULL,
	[AuditCreatedTime] [datetime] NOT NULL,
	[AuditModifiedBy] [varchar](50) NOT NULL,
	[AuditModifiedTime] [datetime] NOT NULL,
 CONSTRAINT [PK_PlanContainer] PRIMARY KEY CLUSTERED 
(
	[PlanContainerUId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [synps].[PlanInstance]    Script Date: 2/19/2017 3:43:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [synps].[PlanInstance](
	[PlanInstanceId] [bigint] IDENTITY(1,1) NOT NULL,
	[PlanUId] [uniqueidentifier] NOT NULL,
	[Status] [varchar](max) NULL,
	[ResultPlan] [varchar](max) NULL,
 CONSTRAINT [PK_PlanInstance] PRIMARY KEY CLUSTERED 
(
	[PlanInstanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [synps].[Plan]  WITH CHECK ADD  CONSTRAINT [FK_Plan_PlanContainer] FOREIGN KEY([PlanContainerUId])
REFERENCES [synps].[PlanContainer] ([PlanContainerUId])
GO

ALTER TABLE [synps].[Plan] CHECK CONSTRAINT [FK_Plan_PlanContainer]
GO

ALTER TABLE [synps].[PlanContainer]  WITH CHECK ADD  CONSTRAINT [FK_PlanContainer_PlanContainer] FOREIGN KEY([ParentUId])
REFERENCES [synps].[PlanContainer] ([PlanContainerUId])
GO

ALTER TABLE [synps].[PlanContainer] CHECK CONSTRAINT [FK_PlanContainer_PlanContainer]
GO

ALTER TABLE [synps].[PlanInstance]  WITH CHECK ADD  CONSTRAINT [FK_PlanInstance_Plan] FOREIGN KEY([PlanUId])
REFERENCES [synps].[Plan] ([PlanUId])
GO

ALTER TABLE [synps].[PlanInstance] CHECK CONSTRAINT [FK_PlanInstance_Plan]
GO


