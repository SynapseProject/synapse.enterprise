using System;
using System.Collections;
using System.Collections.Generic;

using Synapse.Services.Enterprise.Api.Common;


namespace Synapse.Services.Enterprise.Api
{
    public class PlanContainer : SynapseRecordBase, ISynapseSecureRecord, ISynapseHierRecord<PlanContainer>
    {
        public string Description { get; set; }
        public string NodeUri { get; set; }
        public Guid? ParentUId { get; set; }
        public Guid RlsOwner { get; set; }
        public byte[] RlsMask { get; set; }

        public override int CurrentHashCode
        {
            get
            {
                return UId.GetHashCode() + GetStringHashCode( Name ) + GetStringHashCode( Description ) +
                    GetStringHashCode( NodeUri ) + GetGuidHashCode( ParentUId );
            }
        }

        public List<PlanContainer> Children { get; set; } = new List<PlanContainer>();
        Guid ISynapseHierRecord.ParentUId { get { return ParentUId.GetValueOrDefault(); } set { ParentUId = value; } }
        IList ISynapseHierRecord.Children
        {
            get { return Children; }
            set { Children = value as List<PlanContainer>; }
        }
    }
}