using System;

using Synapse.Services.Enterprise.Api.Common;

namespace Synapse.Services.Enterprise.Api
{
    public class PlanItem : SynapseRecordBase
    {
        public string UniqueName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string PlanFile { get; set; }
        public bool PlanFileIsUri { get; set; }
        public Guid PlanContainerUId { get; set; }


        public override int CurrentHashCode
        {
            get
            {
                return UId.GetHashCode() + GetStringHashCode( Name ) + GetStringHashCode( Description ) +
                    GetStringHashCode( UniqueName ) + IsActive.GetHashCode() + GetStringHashCode( PlanFile ) +
                    PlanFileIsUri.GetHashCode() + PlanContainerUId.GetHashCode();
            }
        }
    }
}