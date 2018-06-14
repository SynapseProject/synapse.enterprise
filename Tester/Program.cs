using System;
using System.Collections.Generic;
using System.Linq;

using Suplex.Security.AclModel;
using Synapse.Services.Enterprise.Api;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            PlanContainer top = new PlanContainer() { UniqueName = "top" };
            DiscretionaryAcl topdacl = new DiscretionaryAcl
            {
                new AccessControlEntry<FileSystemRight> { Allowed = true, Right = FileSystemRight.FullControl },
                new AccessControlEntry<FileSystemRight> { Allowed = false, Right = FileSystemRight.Execute | FileSystemRight.List, Inheritable = false },
                new AccessControlEntry<UIRight> { Right= UIRight.Operate | UIRight.Visible }
            };
            top.Security.Dacl = topdacl;

            PlanContainer child = new PlanContainer() { UniqueName = "child" };
            DiscretionaryAcl childdacl = new DiscretionaryAcl
            {
                new AccessControlEntry<FileSystemRight> { Allowed = true, Right = FileSystemRight.FullControl },
                new AccessControlEntry<FileSystemRight> { Allowed = false, Right = FileSystemRight.ReadPermissions | FileSystemRight.ChangePermissions, Inheritable = false },
                new AccessControlEntry<UIRight> { Right= UIRight.Enabled | UIRight.Visible }
            };
            child.Security.Dacl = childdacl;

            top.Children.Add( child );

            top.EvalSecurity();
        }
    }
}