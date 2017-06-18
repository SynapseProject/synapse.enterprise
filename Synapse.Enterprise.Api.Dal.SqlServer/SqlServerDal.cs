using System;
using System.Collections.Generic;
using Suplex.Data;
using Suplex.Forms.ObjectModel.Api;
using Synapse.Core.Utilities;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        public string ContainerRootUniqueName { get; set; } = "SynapseRoot";
        //public string ContainerUniqueNamePrefix { get; set; }
        public string LdapRoot { get; set; }
        public string GlobalExternalGroupsCsv { get; set; }


        SuplexApiClient _splxApi = null;
        SuplexDataAccessLayer _splxDal = null;
        DataAccessor _da = null;
        string _securityContext = null;
        static SqlServerDalConfig Config = null;

        public SqlServerDal(string databaseServerName, string databaseName, string username = null, string password = null)
        {
            ConnectionProperties cp = (!string.IsNullOrWhiteSpace( username ) && !string.IsNullOrWhiteSpace( password )) ?
                new ConnectionProperties( databaseServerName, databaseName, username, password ) :
                new ConnectionProperties( databaseServerName, databaseName );

            _da = new DataAccessor( cp.ConnectionString );
            _splxDal = new SuplexDataAccessLayer( _da );
            _splxApi = new SuplexApiClient( cp.ConnectionString );
        }

        public string SecurityContext
        {
            get
            {
                if( string.IsNullOrWhiteSpace( _securityContext ) )
                    throw new NullReferenceException( "SecurityContext is Empty" );

                return _securityContext;
            }
            set { _securityContext = value; }
        }

        public object GetDefaultConfig()
        {
            return new SqlServerDalConfig();
        }


        public Dictionary<string, string> Configure(IEnterpriseDalConfig conifg)
        {
            if( conifg != null )
            {
                string s = YamlHelpers.Serialize( conifg.Config );
                Config = YamlHelpers.Deserialize<SqlServerDalConfig>( s );

                LdapRoot = conifg.LdapRoot;
                GlobalExternalGroupsCsv = GlobalExternalGroupsCsv;
            }
            else
            {
                ConfigureDefaults();
            }

            Dictionary<string, string> props = new Dictionary<string, string>();
            string name = nameof( SqlServerDal );
            //props.Add( name, CurrentPath );
            props.Add( $"{name} LdapRoot", LdapRoot );
            props.Add( $"{name} GlobalExternalGroupsCsv", GlobalExternalGroupsCsv );
            return props;
        }

        private void ConfigureDefaults()
        {
        }
    }
}