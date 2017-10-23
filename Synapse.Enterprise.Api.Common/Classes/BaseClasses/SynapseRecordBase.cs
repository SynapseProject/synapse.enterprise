using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Services.Enterprise.Api.Common
{
    public abstract partial class SynapseRecordBase : ISynapseRecord
    {
        public virtual Guid UId { get; set; }

        /// <summary>
        /// Item friendly name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The user to create the item.
        /// </summary>
        [Description( "The last user to modify the item." )]
        public virtual string AuditCreatedBy { get; set; }

        /// <summary>
        /// Reflects system create time.
        /// </summary>
        [Description( "Reflects system create time." )]
        public virtual DateTime AuditCreatedTime { get; set; }

        /// <summary>
        /// The last user to modify the item.
        /// </summary>
        [Description( "The last user to modify the item." )]
        public virtual string AuditModifiedBy { get; set; }

        /// <summary>
        /// Reflects system edit time.
        /// </summary>
        [Description( "Reflects system edit time." )]
        public virtual DateTime AuditModifiedTime { get; set; }


        [Description( "Indicates if the record already exists in the database. IsNew == true: the record does not exist." )]
        public virtual bool IsNew { get { return UId == Guid.Empty; } }
        [Description( "The hash of the record when it was created/selected." )]
        public virtual int InitialHashCode { get; set; }
        [Description( "The current hash of the record.  CurrentHashCode != InitialHashCode: IsDirty = true." )]
        public abstract int CurrentHashCode { get; }
        [Description( "Compares CurrentHashCode to InitialHashCode to determine if the record has beem modified.  CurrentHashCode != InitialHashCode: IsDirty = true." )]
        public virtual bool IsDirty { get { return CurrentHashCode != InitialHashCode; } }
        [Description( "Indicates if the record is marked for deletion.  IsDeleted == true: the record will be deleted from the database during Upsert actions." )]
        public virtual bool IsDeleted { get; set; }

        protected int GetStringHashCode(string s)
        {
            return s != null ? s.GetHashCode() : string.Empty.GetHashCode();
        }
        protected int GetGuidHashCode(Guid? g)
        {
            return g.HasValue ? g.GetHashCode() : 0;
        }

        public virtual void SetOwner(Guid ownerUId) { }


        public override string ToString()
        {
            return string.Format( "UId: {0}, Name: {1}, IsDirty: {2}", UId, Name, IsDirty );
        }
    }
}