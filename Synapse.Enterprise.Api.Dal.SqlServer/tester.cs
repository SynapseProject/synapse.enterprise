using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public class Tester
    {
        SqlServerDal _dal = new SqlServerDal( ".\\devo", "synapse" );// new SqlServerDal( ".\\sqlexpress", "synapse" );

        static void Main(string[] args)
        {
            Tester t = new Tester();
            PlanContainer pc = t.CreatePlanContainer();
            PlanItem pi = t.CreatePlanItem( pc );
        }

        PlanContainer CreatePlanContainer()
        {
            PlanContainer pc = new PlanContainer()
            {
                Name = "Root_0",
                Description = "first",
                NodeUri = "http://foo",
                AuditCreatedBy = "steve",
                AuditModifiedBy = "stevo"
            };

            pc = _dal.UpsertPlanContainer( pc );

            pc = _dal.GetPlanContainerByUId( pc.UId );

            pc.Name += "_foo";
            pc = _dal.UpsertPlanContainer( pc );

            return pc;
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