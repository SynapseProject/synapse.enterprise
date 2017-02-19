using System;

using Suplex.Data;
using Suplex.Forms.ObjectModel.Api;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        SuplexDataAccessLayer _splx = null;
        DataAccessor _da = null;

        public SqlServerDal(string databaseServerName, string databaseName, string username = null, string password = null)
        {
            ConnectionProperties cp = (!string.IsNullOrWhiteSpace( username ) && !string.IsNullOrWhiteSpace( password )) ?
                new ConnectionProperties( databaseServerName, databaseName, username, password ) :
                new ConnectionProperties( databaseServerName, databaseName );

            _da = new DataAccessor( cp.ConnectionString );
            _splx = new SuplexDataAccessLayer( _da );
        }
    }
}