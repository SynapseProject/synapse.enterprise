using System;
using NUnit.Framework;
using Synapse.Services.Enterprise.Api;
using Synapse.Services.Enterprise.Api.Dal;

namespace Synapse.Enterprise.UnitTests
{
    [TestFixture]
    public class UnitTests
    {
        const string __root = "SynapseRoot";
        const string __unit = "SynapseUnitTest";
        SqlServerDal _dal = new SqlServerDal( "(localdb)\\MSSQLLocalDB", "synapse" );// new SqlServerDal( ".\\sqlexpress", "synapse" );

        [OneTimeSetUp]
        public void Init()
        {
            _dal.SecurityContext = "steve";
        }

        [Test]
        [Category( "PlanContainer" )]
        void WarpFactorLove()
        {
            Guid rootId = Guid.NewGuid();
            PlanContainer root = UpsertPlanContainer( rootId, __root, null );
            PlanContainer groot = _dal.GetPlanContainerByUId( root.UId );

            Assert.AreEqual( root.CurrentHashCode, groot.CurrentHashCode );


            Guid childId = Guid.NewGuid();
            PlanContainer child = UpsertPlanContainer( childId, null, root.UId );
            child.Name += "_foo";
            child = _dal.UpsertPlanContainer( child );
            PlanContainer quiddo = _dal.GetPlanContainerByUId( child.UId );

            Assert.AreEqual( child.CurrentHashCode, quiddo.CurrentHashCode );
            Assert.AreEqual( root.UId, quiddo.ParentUId.GetValueOrDefault() );


            //UpdateSecurityRecord( child );
            //PlanItem pi = CreatePlanItem( child );
        }

        [Test]
        [Category( "PlanContainer" )]
        [TestCase( __root )]
        PlanContainer UpsertPlanContainer(Guid containerId, string name = null, Guid? parentId = null)
        {
            PlanContainer pc = new PlanContainer()
            {
                UId = containerId,
                Name = string.IsNullOrWhiteSpace( name ) ? $"Root_0_{DateTime.Now.Ticks}" : name,
                Description = __unit,
                NodeUri = "http://foo",
                AuditCreatedBy = "steve",
                AuditModifiedBy = "stevo",
                RlsOwner = Guid.Parse( "a61f281e-d80f-4ddf-856d-5f6eb7a20caa" )
            };
            if( parentId.HasValue )
                pc.ParentUId = parentId.Value;

            pc = _dal.UpsertPlanContainer( pc );

            return pc;
        }

        [Test]
        [Category( "PlanContainer" )]
        void UpdateSecurityRecord(PlanContainer pc)
        {
            PermissionItem perm = new PermissionItem()
            {
                GroupId = Guid.Parse( "8E786183-592F-4322-A6B6-E4453E84A3D7" ),
                State = RecordState.Added,
                Rights = PermissionUtility.RightsFromRole( PermissionRole.ReadWrite )
            };
            PlanContainerSecurity csr = new PlanContainerSecurity()
            {
                PlanContainerUId = pc.UId
            };
            csr.Permissions.Add( perm );
            _dal.UpdatePlanContainerSecurity( csr );

            PlanContainerSecurity sec = _dal.GetPlanContainerSecurity( pc.UId );
        }

        [Test]
        [Category( "PlanItem" )]
        private PlanItem CreatePlanItem(PlanContainer pc)
        {
            PlanItem pi = new PlanItem()
            {
                Name = "foo",
                Description = "poo",
                UniqueName = "uniqua",
                IsActive = true,
                PlanFile = "http://moo",
                PlanFileIsUri = true,
                PlanContainerUId = pc.UId,
                AuditCreatedBy = "steve",
                AuditModifiedBy = "stevo"
            };

            pi = _dal.UpsertPlan( pi );

            pi = _dal.GetPlanByUId( pi.UId );

            pi.Name += "foo";
            pi = _dal.UpsertPlan( pi );

            return pi;
        }
    }
}