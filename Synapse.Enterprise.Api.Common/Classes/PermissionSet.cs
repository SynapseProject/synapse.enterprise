using System;

using Suplex.Security;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public enum RecordState
    {
        Unchanged,
        Added,
        Modified,
        Deleted
    };

    public enum PermissionRole
    {
        ListOnly,
        ListExecute,
        ReadExecute,
        ReadOnly,
        ReadWrite,
        ReadWriteExecute,
        Admin
    };

    public class PermissionUtility
    {
        public static FileSystemRight RightsFromRole(PermissionRole role)
        {
            switch( role )
            {
                default: { return FileSystemRight.List; }
                case PermissionRole.ListExecute: { return FileSystemRight.List | FileSystemRight.Execute; }
                case PermissionRole.ReadExecute: { return FileSystemRight.List | FileSystemRight.Read | FileSystemRight.Execute; }
                case PermissionRole.ReadOnly: { return FileSystemRight.List | FileSystemRight.Read; }
                case PermissionRole.ReadWrite: { return FileSystemRight.List | FileSystemRight.Read |
                        FileSystemRight.Write | FileSystemRight.Create | FileSystemRight.Delete; }
                case PermissionRole.ReadWriteExecute: { return FileSystemRight.List | FileSystemRight.Read |
                        FileSystemRight.Write | FileSystemRight.Create | FileSystemRight.Delete | FileSystemRight.Execute; }
                case PermissionRole.Admin: { return FileSystemRight.FullControl; }
            }
        }
    }

    public class PermissionSet
    {
        public long Id { get; set; }                     // Primary Key from Suplex Table
        public Guid GroupId { get; set; }               // Unique Id Of Container Security Group
        public string GroupName { get; set; }           // Name of Container Security Group
        public FileSystemRight Rights { get; set; }     // Group Rights to the Container
        public RecordState State { get; set; }          // Current State of the Record

        public static PermissionSet FromAce(Suplex.Forms.ObjectModel.Api.AccessControlEntryBase ace)
        {
            PermissionSet perm = new PermissionSet();

            perm.Id = ace.Id;
            perm.GroupId = Guid.Parse( ace.SecurityPrincipal.Id );
            perm.GroupName = ace.SecurityPrincipal.Name;
            perm.Rights = (FileSystemRight)ace.Right;

            return perm;
        }
    }
}