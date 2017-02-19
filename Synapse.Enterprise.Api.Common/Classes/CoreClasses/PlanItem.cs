using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Synapse.Services.Enterprise.Api.Common;

namespace Synapse.Services.Enterprise.Api
{
    public class PlanItem : SynapseRecordBase
    {

        public string UniqueName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public override int CurrentHashCode
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object PlanFile { get; set; }
        public object PlanFileIsUri { get; set; }
        public object PlanContainerUId { get; set; }
    }
}