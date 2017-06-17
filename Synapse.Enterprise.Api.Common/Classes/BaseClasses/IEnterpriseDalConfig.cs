using System;
using System.Collections.Generic;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public interface IEnterpriseDalConfig
    {
        object Config { get; set; }
        string LdapRoot { get; set; }
        string Type { get; set; }
    }
}