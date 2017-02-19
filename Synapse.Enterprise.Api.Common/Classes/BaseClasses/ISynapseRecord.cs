using System;
using System.Collections;
using System.Collections.Generic;

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

    public interface ISynapseSecureRecord
    {
        Guid RlsOwner { get; set; }
        byte[] RlsMask { get; set; }
    }

    /// <summary>
    /// Extends ISynapseRecord to include ParentUId and Children
    /// </summary>
    public interface ISynapseHierRecord : ISynapseRecord
    {
        Guid ParentUId { get; set; }

        IList Children { get; set; }
    }

    public interface ISynapseHierRecord<T> : ISynapseHierRecord where T : ISynapseRecord
    {
        new List<T> Children { get; set; }
    }

}