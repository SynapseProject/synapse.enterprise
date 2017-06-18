using System;
using System.Collections.Generic;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public class Tester
    {
        const string __root = "SynapseRoot";
        SqlServerDal _dal = new SqlServerDal( ".\\devo", "synapse" );// new SqlServerDal( ".\\sqlexpress", "synapse" );

        static void Main(string[] args)
        {
            Tester t = new Tester();
            t._dal.SecurityContext = "steve";

            PlanContainer root = t.CreatePlanContainer( __root, null );
            PlanContainer pc = t.CreatePlanContainer( null, root.UId );
            t.UpdateSecurityRecord( pc );
            PlanItem pi = t.CreatePlanItem( pc );
        }

        PlanContainer CreatePlanContainer(string name = null, Guid? parentId = null)
        {
            PlanContainer pc = new PlanContainer()
            {
                Name = string.IsNullOrWhiteSpace( name ) ? $"Root_0_{DateTime.Now.Ticks}" : name,
                Description = "first",
                NodeUri = "http://foo",
                AuditCreatedBy = "steve",
                AuditModifiedBy = "stevo",
                RlsOwner = Guid.Parse( "a61f281e-d80f-4ddf-856d-5f6eb7a20caa" )
            };
            if( parentId.HasValue )
                pc.ParentUId = parentId.Value;

            pc = _dal.UpsertPlanContainer( pc );

            pc = _dal.GetPlanContainerByUId( pc.UId );

            if( name != __root )
            {
                pc.Name += "_foo";
                pc = _dal.UpsertPlanContainer( pc );
            }

            return pc;
        }

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