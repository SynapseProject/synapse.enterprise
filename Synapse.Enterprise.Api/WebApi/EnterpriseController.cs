using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;

using Synapse.Core;
using Synapse.Common.WebApi;
//using Synapse.Services.Enterprise.Api.Dal;
using Synapse.Core.Utilities;

namespace Synapse.Services
{
    [RoutePrefix( "synapse/enterprise" )]
    public class EnterpriseController : ApiController
    {
        //IEnterpriseDal _dal = null;

        //void LoadDal()
        //{
        //    if( SynapseServer.Config.ServerIsController )
        //    {
        //        string defaultType = "Synapse.Controller.Dal.FileSystem:Synapse.Services.Controller.Dal.FileSystemDal";
        //        _dal = AssemblyLoader.Load<IEnterpriseDal>( SynapseServer.Config.Controller.Dal, defaultType );
        //    }
        //}



        [HttpGet]
        [Route( "hello" )]
        public string Hello()
        {
            string context = GetContext( nameof( Hello ) );

            try
            {
                SynapseEnterprise.Logger.Debug( context );
                return "Hello from SynapseController, World!";
            }
            catch( Exception ex )
            {
                SynapseEnterprise.Logger.Error(
                    Utilities.UnwindException( context, ex, asSingleLine: true ) );
                throw;
            }
        }

        [HttpGet]
        [Route( "hello/whoami" )]
        public string WhoAmI()
        {
            string context = GetContext( nameof( WhoAmI ) );

            try
            {
                SynapseEnterprise.Logger.Debug( context );
                return CurrentUser;
            }
            catch( Exception ex )
            {
                SynapseEnterprise.Logger.Error(
                    Utilities.UnwindException( context, ex, asSingleLine: true ) );
                throw;
            }
        }

        #region utility methods
        string GetContext(string context, params object[] parms)
        {
            StringBuilder c = new StringBuilder();
            c.Append( $"{context}(" );
            for( int i = 0; i < parms.Length; i += 2 )
                c.Append( $"{parms[i]}: {parms[i + 1]}, " );

            return $"{c.ToString().TrimEnd( ',', ' ' )})";
        }

        string GetContext(string context, Dictionary<string, object> d)
        {
            StringBuilder c = new StringBuilder();
            c.Append( $"{context}(" );
            foreach( string key in d.Keys )
                c.Append( $"{key}: {d[key]}, " );

            return $"{c.ToString().TrimEnd( ',', ' ' )})";
        }

        string CurrentUser
        {
            get
            {
                return User != null && User.Identity != null ? User.Identity.Name : "Anonymous";
            }
        }
        #endregion
    }
}

//////[Route( "{domainUId:Guid}" )]
//////[HttpGet]
////// GET api/demo 
//[Route( "foo/" )]
//public IEnumerable<string> Get()
//{
//    return new string[] { "Hello", "World", CurrentUser };
//}

//// GET api/demo/5 
//public string Get(int id)
//{
//    return "Hello, World!";
//}

////[Route( "" )]
////[Route( "byrls/" )]
////[HttpPost]
//// POST api/demo 
//public void Post([FromBody]string value) { }
//// PUT api/demo/5 
//public void Put(int id, [FromBody]string value) { }
//// DELETE api/demo/5 
//public void Delete(int id) { }