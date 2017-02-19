using Suplex.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        DataAccessor _da = null;

        public SqlServerDal(string databaseServerName, string databaseName)
        {
            ConnectionProperties cp = new ConnectionProperties( databaseServerName, databaseName );

            _da = new DataAccessor( cp.ConnectionString );
        }
    }
}