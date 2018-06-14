using System;
using System.Collections;
using System.Collections.Generic;

using Suplex.Security.AclModel;

using Synapse.Services.Enterprise.Api.Common;


namespace Synapse.Services.Enterprise.Api
{
    public class PlanContainer : SynapseRecordBase, ISynapseSecureRecord, ISynapseHierRecord<PlanContainer>, ISecureObject, ICloneable<PlanContainer>
    {
        public string Description { get; set; }
        public string NodeUri { get; set; }
        public Guid? ParentUId { get; set; }
        public Guid RlsOwner { get; set; }
        public byte[] RlsMask { get; set; }

        public string UniqueName { get; set; }
        public bool IsEnabled { get; set; }

        public virtual SecurityDescriptor Security { get; set; } = new SecurityDescriptor();
        ISecurityDescriptor ISecureObject.Security { get => Security; set => Security = value as SecurityDescriptor; }

        Guid? ISecureObject.UId { get => UId; set => UId = value.GetValueOrDefault(); }
        Guid ISynapseHierRecord.ParentUId { get { return ParentUId.GetValueOrDefault(); } set { ParentUId = value; } }

        public virtual PlanContainer Parent { get; set; }
        ISecureObject ISecureObject.Parent { get => Parent; set => Parent = value as PlanContainer; }


        public List<PlanContainer> Children { get; set; } = new List<PlanContainer>();
        IList ISynapseHierRecord.Children
        {
            get { return Children; }
            set { Children = value as List<PlanContainer>; }
        }
        IList ISecureObject.Children
        {
            get { return Children; }
            set { Children = value as List<PlanContainer>; }
        }


        public override int CurrentHashCode
        {
            get
            {
                return UId.GetHashCode() + GetStringHashCode( Name ) + GetStringHashCode( Description ) +
                    GetStringHashCode( NodeUri ) + GetGuidHashCode( ParentUId );
            }
        }

        object ICloneable.Clone() { return Clone( true ); }
        ISecureObject ICloneable<ISecureObject>.Clone(bool shallow) { return Clone( shallow ); }
        public PlanContainer Clone(bool shallow)
        {
            return MemberwiseClone() as PlanContainer;
        }
    }
}