using System;

using Suplex.Data;
using Suplex.Forms.ObjectModel.Api;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        SuplexDataAccessLayer _splx = null;
        DataAccessor _da = null;

        public SqlServerDal(string databaseServerName, string databaseName, string username, string password)
        {
            ConnectionProperties cp = (!string.IsNullOrWhiteSpace( username ) && !string.IsNullOrWhiteSpace( password )) ?
                new ConnectionProperties( databaseServerName, databaseName ) :
                new ConnectionProperties( databaseServerName, databaseName, username, password );

            _da = new DataAccessor( cp.ConnectionString );
            _splx = new SuplexDataAccessLayer( cp.ConnectionString );
        }
    }
}