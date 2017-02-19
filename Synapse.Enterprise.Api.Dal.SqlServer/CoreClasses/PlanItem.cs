using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        public PlanItem GetPlanByUId(Guid planUId, bool autoManageSqlConnection = true, bool validateRls = false)
        {
            PlanItem planItem = null;
            SortedList parms = new sSortedList( "PlanUId", planUId );

            DataTable t = _da.GetDataSet( "[synps].[api_plan_sel]", parms, autoManageSqlConnection ).Tables[0];
            if( t.Rows.Count > 0 )
            {
                PlanItemFactory factory = new PlanItemFactory();
                planItem = factory.CreateRecord( t.Rows[0] );

                if( validateRls )
                {
                    PlanContainer planContainer = GetPlanContainerByUId( planItem.PlanContainerUId );
                    if( !HasRlsAccess( planContainer, _securityContext ) )
                        throw new Exception( "You do not have rights to this record." );
                }
            }

            return planItem;
        }

        public List<PlanItem> GetPlanByAny(Guid? planUId, string name, string uniqueName, string planFile, bool? planFileIsUri, Guid? planContainerUId, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime)
        {
            return new List<PlanItem>();
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

                _da.ExecuteSP( "[synps].[api_plan_dml_ins]", parms, ref outparms, trans == null, trans );

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
                    _da.ExecuteSP( "[synps].[api_plan_dml_upd]", parms, trans == null, trans );

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
            SortedList parms = new SortedList();

            if( !forCreate ) parms.Add( "@PlanUId", plan.UId );
            parms.Add( "@Name", plan.Name );
            if( string.IsNullOrWhiteSpace( plan.Description ) ) parms.Add( "@Description", Convert.DBNull ); else parms.Add( "@Description", plan.Description );
            parms.Add( "@UniqueName", plan.UniqueName );
            parms.Add( "@IsActive", plan.IsActive );
            parms.Add( "@PlanFile", plan.PlanFile );
            parms.Add( "@PlanFileIsUri", plan.PlanFileIsUri );
            parms.Add( "@PlanContainerUId", plan.PlanContainerUId );

            if( forCreate )
                parms.Add( "@AuditCreatedBy", plan.AuditCreatedBy );
            else
                parms.Add( "@AuditModifiedBy", plan.AuditModifiedBy );

            return parms;
        }
    }

    public class PlanItemFactory : SynapseRecordFactoryBase<PlanItem>
    {
        public override PlanItem CreateRecord(DataRow r)
        {
            PlanItem planItem = new PlanItem();

            planItem.UId = r.GetColumnValueAsGuid( "PlanUId" );
            planItem.Name = r.GetColumnValueAsString( "Name" );
            planItem.Description = r.GetColumnValueAsString( "Description" );
            planItem.UniqueName = r.GetColumnValueAsString( "UniqueName" );
            planItem.IsActive = r.GetColumnValueAsBool( "IsActive" );
            planItem.PlanFile = r.GetColumnValueAsString( "PlanFile" );
            planItem.PlanFileIsUri = r.GetColumnValueAsBool( "PlanFileIsUri" );
            planItem.PlanContainerUId = r.GetColumnValueAsGuid( "PlanContainerUId" );
            planItem.AuditCreatedBy = r.GetColumnValueAsString( "AuditCreatedBy" );
            planItem.AuditCreatedTime = r.GetColumnValueAsDateTime( "AuditCreatedTime" );
            planItem.AuditModifiedBy = r.GetColumnValueAsString( "AuditModifiedBy" );
            planItem.AuditModifiedTime = r.GetColumnValueAsDateTime( "AuditModifiedTime" );

            planItem.InitialHashCode = planItem.CurrentHashCode;

            return planItem;
        }
    }
}