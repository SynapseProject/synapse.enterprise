using System;
using System.Collections.Generic;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public interface IEnterpriseDal
    {
        PlanContainer GetPlanContainerByUId(Guid planContainerUId, bool autoManageSqlConnection = true, bool validateRls = false);
        List<PlanContainer> GetPlanContainerByAny(Guid? planContainerUId, string name, string nodeUri, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime,
            bool returnAsHierarchy = false, Guid? startUId = null, bool validateRls = false);
        PlanContainer UpsertPlanContainer(PlanContainer planContainer);
        void DeletePlanContainer(Guid planUId);

        PlanItem GetPlanByUId(Guid planUId, bool autoManageSqlConnection = true, bool validateRls = false);
        List<PlanItem> GetPlanByAny(Guid? planUId, string name, string uniqueName, string planFile, bool? planFileIsUri, Guid? planContainerUId, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime);
        PlanItem UpsertPlan(PlanItem plan);
        void DeletePlan(Guid planUId);
    }
}