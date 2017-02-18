﻿using System;

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
        None,
        Reader,
        Writer,
        Admin
    };

    public class PermissionSet
    {
        public int Id { get; set; }                     // Primary Key from Suplex Table
        public Guid GroupId { get; set; }               // Unique Id Of Container Security Group
        public String GroupName { get; set; }           // Name of Container Security Group
        public FileSystemRight Rights { get; set; }     // Group Rights to the Container
        public RecordState State { get; set; }          // Current State of the Record
    }
}