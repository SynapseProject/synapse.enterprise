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
        SqlServerDal _dal = new SqlServerDal( "(localdb)\\MSSQLLocalDB", "synapse" );// new SqlServerDal( ".\\sqlexpress", "synapse" );

        [OneTimeSetUp]
        public void Init()
        {
            _dal.SecurityContext = "steve";
        }

        [Test]
        [Category( "PlanContainer" )]
        [TestCase( true )]
        [TestCase( false )]
        public void WarpFactorLove(bool deleteData)
        {
            Guid rlsOwner = _dal.GetSuplexUser( true, false ).IdToGuid();
            PlanContainer root = ObjectFactory.MakePlanContainer( name: __root, rlsOwner: rlsOwner );
            root = _dal.UpsertPlanContainer( root );
            PlanContainer groot = _dal.GetPlanContainerByUId( root.UId );
            Assert.AreEqual( root.CurrentHashCode, groot.CurrentHashCode );


            PlanContainer child = ObjectFactory.MakePlanContainer( rlsOwner: rlsOwner, parentId: root.UId );
            child = _dal.UpsertPlanContainer( child );
            child.Name += "_foo";
            child = _dal.UpsertPlanContainer( child );
            PlanContainer quiddo = _dal.GetPlanContainerByUId( child.UId );
            Assert.AreEqual( child.CurrentHashCode, quiddo.CurrentHashCode );
            Assert.AreEqual( root.UId, quiddo.ParentUId.GetValueOrDefault() );


            PlanItem pi = ObjectFactory.MakePlanItem( child );
            pi = _dal.UpsertPlan( pi );
            PlanItem rpi = _dal.GetPlanByUId( pi.UId );
            Assert.AreEqual( pi.CurrentHashCode, rpi.CurrentHashCode );

            pi.Name += "_foo";
            pi = _dal.UpsertPlan( pi );
            rpi = _dal.GetPlanByUId( pi.UId );
            Assert.AreEqual( pi.CurrentHashCode, rpi.CurrentHashCode );


            UpdateSecurityRecord( child.UId );


            if( !deleteData )
                return;

            _dal.DeletePlan( pi.UId );
            rpi = _dal.GetPlanByUId( pi.UId );
            Assert.IsNull( rpi );


            _dal.DeletePlanContainer( child.UId );
            quiddo = _dal.GetPlanContainerByUId( child.UId );
            Assert.IsNull( quiddo );


            _dal.DeletePlanContainer( root.UId );
            groot = _dal.GetPlanContainerByUId( groot.UId );
            Assert.IsNull( groot );
        }

        [Test]
        [Category( "PlanContainer" )]
        public void SelectPlanContainerByUser()
        {
            _dal.GetPlanContainerHierarchy( null );
        }

        [Test]
        [Category( "Security" )]
        public void UpdateSecurityRecord(Guid planContainerUId)
        {
            PermissionItem perm = new PermissionItem()
            {
                GroupId = Guid.Parse( "4f89a474-b841-47ce-a438-dded1f9b742e" ),
                State = RecordState.Added,
                ////Rights = PermissionUtility.RightsFromRole( PermissionRole.ReadWrite )
            };
            PlanContainerSecurity csr = new PlanContainerSecurity()
            {
                PlanContainerUId = planContainerUId
            };

            csr.Permissions.Add( perm );
            _dal.UpdatePlanContainerSecurity( csr );

            PlanContainerSecurity sec = _dal.GetPlanContainerSecurity( planContainerUId );
        }
    }

    partial class ObjectFactory
    {
        const string __unit = "SynapseUnitTest";

        public static PlanContainer MakePlanContainer(Guid? containerId = null, string name = null, Guid? rlsOwner = null, Guid? parentId = null)
        {
            PlanContainer pc = new PlanContainer()
            {
                UId = containerId ?? Guid.Empty,
                Name = string.IsNullOrWhiteSpace( name ) ? $"Container_{DateTime.Now.Ticks}" : name,
                Description = __unit,
                NodeUri = "http://foo",
                AuditCreatedBy = "steve",
                AuditModifiedBy = "stevo",
                RlsOwner = rlsOwner.Value
            };

            if( parentId.HasValue )
                pc.ParentUId = parentId.Value;

            return pc;
        }

        public static PlanItem MakePlanItem(PlanContainer pc, Guid? planItemUId = null)
        {
            PlanItem pi = new PlanItem()
            {
                UId = planItemUId ?? Guid.Empty,
                Name = "foo",
                Description = "bar",
                UniqueName = "uniqua",
                IsActive = true,
                PlanFile = "http://moo",
                PlanFileIsUri = true,
                PlanContainerUId = pc.UId,
                AuditCreatedBy = "steve",
                AuditModifiedBy = "stevo"
            };

            return pi;
        }

    }
}