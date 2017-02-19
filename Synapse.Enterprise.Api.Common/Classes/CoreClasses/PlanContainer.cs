using System;
using System.Collections.Generic;

using Synapse.Services.Enterprise.Api.Common;


namespace Synapse.Services.Enterprise.Api
{
    public class PlanContainer : SynapseRecordBase
    {
        public string Description { get; set; }
        public string NodeUri { get; set; }
        public Guid ParentUId { get; set; }
        public Guid RlsOwner { get; set; }
        public byte[] RlsMask { get; set; }

        public override int CurrentHashCode
        {
            get
            {
                throw new Exception( "need to do this next" );
            }
        }
    }
}