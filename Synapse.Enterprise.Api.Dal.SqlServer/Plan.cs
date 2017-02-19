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
        public PlanItem GetPlanByUId(Guid planUId)
        {
            throw new NotImplementedException();
        }

        public List<PlanItem> GetPlanByAny(Guid? planUId, string name, string uniqueName, string planFile, bool? planFileIsUri, Guid? planContainerUId, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime)
        {
            throw new NotImplementedException();
        }

        public PlanItem UpsertPlan(PlanItem plan)
        {
            if( plan.IsDirty  )
                if( plan.IsNew )
                    plan = CreatePlanItem( plan );
                else
                    plan = UpdatePlanItem( plan );

            return plan;
        }

        public PlanItem CreatePlanItem(PlanItem plan)
        {
            SqlTransaction trans = null;

            try
            {
                SortedList parms = GetPlanParms( plan, true );

                SqlParameter id = new SqlParameter( "PlanUId", SqlDbType.UniqueIdentifier );
                id.Value = Convert.DBNull;
                id.Direction = ParameterDirection.InputOutput;
                SortedList outparms = new sSortedList( "PlanUId", id );

                _da.ExecuteSP( "[snyps].[api_plan_dml_ins]", parms, ref outparms, trans == null, trans );

                plan.UId = (Guid)id.Value;
                plan.InitialHashCode = plan.CurrentHashCode;
            }
            catch( Exception ex )
            {
                if( ex is SqlException )
                    if( ((SqlException)ex).Message.ToLower().Contains( "unique key" ) )
                        throw new Exception( "Validation Failed: One or more of the values provided are duplicate, please change." ); //ValidationException

                throw;
            }
            finally
            {
                _da.CloseConnection();
            }

            return plan;
        }

        public PlanItem UpdatePlanItem(PlanItem plan)
        {
            //throws exception on rls failure
            GetPlanByUId( plan.UId );

            SqlTransaction trans = null;

            try
            {
                if( plan.IsDirty )
                {
                    SortedList parms = GetPlanParms( plan, false );
                    _da.ExecuteSP( "[snyps].[api_plan_dml_upd]", parms, trans == null, trans );

                    plan.InitialHashCode = plan.CurrentHashCode;
                }
            }
            catch( Exception ex )
            {
                if( ex is SqlException )
                    if( ((SqlException)ex).Message.ToLower().Contains( "unique key" ) )
                        throw new Exception( "Validation Failed: One or more of the values provided are duplicate, please change." ); //ValidationException

                throw;
            }
            finally
            {
                _da.CloseConnection();
            }

            return plan;
        }

        public void DeletePlan(Guid planUId)
        {
            throw new NotImplementedException();
        }


        private SortedList GetPlanParms(PlanItem plan, bool forCreate)
        {
            throw new NotImplementedException();
        }
    }
}