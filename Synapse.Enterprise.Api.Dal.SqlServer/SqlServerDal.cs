using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public class SqlServerDal : IEnterpriseDal
    {
        public PlanItem GetPlanByUId(Guid planUId)
        {
            throw new NotImplementedException();
        }

        public List<PlanItem> GetPlanByAny(Guid? planUId, string name, string uniqueName, string planFile, bool? planFileIsUri, Guid? planContainerUId, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime)
        {
            throw new NotImplementedException();
        }

        public PlanItem UpsertPlan(PlanItem plan)
        {
            throw new NotImplementedException();
        }

        public void DeletePlan(Guid planUId)
        {
            throw new NotImplementedException();
        }



        public List<PlanContainer> GetPlanContainerByUId(Guid planContainerUId)
        {
            throw new NotImplementedException();
        }

        public PlanContainer GetPlanContainerByAny(Guid? planContainerUId, string name, string nodeUri, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime)
        {
            throw new NotImplementedException();
        }

        public PlanContainer UpsertPlanContainer(PlanContainer planContainer)
        {
            throw new NotImplementedException();
        }

        public void DeletePlanContainer(Guid planUId)
        {
            throw new NotImplementedException();
        }
    }
}