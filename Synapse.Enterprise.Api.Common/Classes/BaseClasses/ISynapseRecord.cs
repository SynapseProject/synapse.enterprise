using System;

namespace Synapse.Services.Enterprise.Api.Common
{
    public interface ISynapseRecord
    {
        Guid UId { get; set; }

        string Name { get; set; }

        string AuditCreatedBy { get; set; }
        DateTime AuditCreatedTime { get; set; }
        string AuditModifiedBy { get; set; }
        DateTime AuditModifiedTime { get; set; }


        bool IsNew { get; }
        int InitialHashCode { get; set; }
        int CurrentHashCode { get; }
        bool IsDirty { get; }
        bool IsDeleted { get; set; }

        void SetOwner(Guid ownerUId);
    }
}