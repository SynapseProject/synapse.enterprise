using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public class Tester
    {
        SqlServerDal _dal = new SqlServerDal( ".\\sqlexpress", "synapse" );

        static void Main(string[] args)
        {
            Tester t = new Tester();
            t.CreatePlanContainer();
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
    }
}