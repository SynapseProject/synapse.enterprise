using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Suplex.Forms.ObjectModel.Api;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        public List<PlanContainer> GetPlanContainerByUId(Guid planContainerUId)
        {
            throw new NotImplementedException();
        }

        public PlanContainer GetPlanContainerByAny(Guid? planContainerUId, string name, string nodeUri, string createdBy, DateTime? createdTime, string modifiedBy, DateTime? modifiedTime)
        {
            throw new NotImplementedException();
        }

        public PlanContainer UpsertPlanContainer(PlanContainer planContainer)
        {
            if( planContainer.IsDirty )
                if( planContainer.IsNew )
                    planContainer = CreatePlanContainer( planContainer );
                else
                    planContainer = UpdatePlanContainer( planContainer );

            return planContainer;
        }

        public PlanContainer CreatePlanContainer(PlanContainer planContainer)
        {
            SqlTransaction trans = null;

            try
            {
                _da.OpenConnection();
                trans = _da.Connection.BeginTransaction();

                UIElement uie = GetUIElementFromPlanContainer( planContainer );
                uie = _splx.UpsertUIElement( uie, trans );

                planContainer.UId = uie.Id;
                SortedList parms = GetPlanContainerParms( planContainer, true );
                _da.ExecuteSP( "[snyps].[api_planContainer_dml_ins]", parms, trans == null, trans );

                trans.Commit();

                planContainer.InitialHashCode = planContainer.CurrentHashCode;
            }
            catch( Exception ex )
            {
                trans.Rollback();

                if( ex is SqlException )
                    if( ((SqlException)ex).Message.ToLower().Contains( "unique key" ) )
                        throw new Exception( "Validation Failed: One or more of the values provided are duplicate, please change." ); //ValidationException

                throw;
            }
            finally
            {
                _da.CloseConnection();
            }

            return planContainer;
        }

        public PlanContainer UpdatePlanContainer(PlanContainer planContainer)
        {
            //throws exception on rls failure
            GetPlanByUId( planContainer.UId );

            SqlTransaction trans = null;

            try
            {
                _da.OpenConnection();
                trans = _da.Connection.BeginTransaction();

                UIElement uie = GetUIElementFromPlanContainer( planContainer );
                uie = _splx.UpsertUIElement( uie, trans );

                SortedList parms = GetPlanContainerParms( planContainer, false );
                _da.ExecuteSP( "[snyps].[api_planContainer_dml_upd]", parms, trans == null, trans );

                trans.Commit();

                planContainer.InitialHashCode = planContainer.CurrentHashCode;
            }
            catch( Exception ex )
            {
                trans.Rollback();

                if( ex is SqlException )
                    if( ((SqlException)ex).Message.ToLower().Contains( "unique key" ) )
                        throw new Exception( "Validation Failed: One or more of the values provided are duplicate, please change." ); //ValidationException

                throw;
            }
            finally
            {
                _da.CloseConnection();
            }

            return planContainer;
        }

        public void DeletePlanContainer(Guid planUId)
        {
            throw new NotImplementedException();
        }


        UIElement GetUIElementFromPlanContainer(PlanContainer planContainer)
        {
            UIElement uie = new UIElement()
            {
                Name = planContainer.Name,
                UniqueName = planContainer.Name,
                ControlType = "SplxFileSystemManager",
                ParentId = planContainer.ParentUId
            };

            uie.SecurityDescriptor.DaclInherit =
                uie.SecurityDescriptor.SaclInherit = true;
            uie.SecurityDescriptor.SaclAuditTypeFilter = (Suplex.Security.AuditType)31;

            return uie;
        }

        private SortedList GetPlanContainerParms(PlanContainer planContainer, bool forCreate)
        {
            SortedList parms = new SortedList();

            parms.Add( "@PlanContainerUId", planContainer.UId );
            parms.Add( "@Name", planContainer.Name );
            if( string.IsNullOrWhiteSpace( planContainer.Description ) ) parms.Add( "@Description", Convert.DBNull ); else parms.Add( "@Description", planContainer.Description );
            if( string.IsNullOrWhiteSpace( planContainer.NodeUri ) ) parms.Add( "@NodeUri", Convert.DBNull ); else parms.Add( "@NodeUri", planContainer.NodeUri );
            if( planContainer.RlsOwner == Guid.Empty ) parms.Add( "@RlsOwner", Convert.DBNull ); else parms.Add( "@RlsOwner", planContainer.RlsOwner );
            if( planContainer.RlsMask == null ) parms.Add( "@RlsMask", Convert.DBNull ); else parms.Add( "@RlsMask", planContainer.RlsMask );
            if( planContainer.ParentUId == Guid.Empty ) parms.Add( "@ParentUId", Convert.DBNull ); else parms.Add( "@ParentUId", planContainer.ParentUId );

            if( forCreate )
                parms.Add( "@AuditCreatedBy", planContainer.AuditCreatedBy );
            else
                parms.Add( "@AuditModifiedBy", planContainer.AuditModifiedBy );

            return parms;
        }
    }
}