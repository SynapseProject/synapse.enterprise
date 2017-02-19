using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
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