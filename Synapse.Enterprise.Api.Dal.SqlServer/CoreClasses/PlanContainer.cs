using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;

using Suplex.Forms.ObjectModel.Api;
using ss = Suplex.Security;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public partial class SqlServerDal : IEnterpriseDal
    {
        #region crud
        public PlanContainer GetPlanContainerByUId(Guid planContainerUId, bool autoManageSqlConnection = true, bool validateRls = false)
        {
            PlanContainer planContainer = null;
            SortedList parms = new sSortedList( "PlanContainerUId", planContainerUId );

            DataTable t = _da.GetDataSet( "[synps].[api_planContainer_sel]", parms, autoManageSqlConnection ).Tables[0];
            if( t.Rows.Count > 0 )
            {
                PlanContainerFactory factory = new PlanContainerFactory();
                planContainer = factory.CreateRecord( t.Rows[0] );

                if( validateRls )
                    if( !HasRlsAccess( planContainer, _securityContext ) )
                        throw new Exception( "You do not have rights to this record." );
            }

            return planContainer;
        }

        public List<PlanContainer> GetPlanContainerByAny(Guid? planContainerUId = null, string name = null, string nodeUri = null, string createdBy = null, DateTime? createdTime = null, string modifiedBy = null, DateTime? modifiedTime = null)
        {
            return new List<PlanContainer>();
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

                //create Suplex security element
                UIElement uie = GetSuplexUIElementFromPlanContainer( planContainer );
                uie = _splxDal.UpsertUIElement( uie, trans );

                //assign the splx element id to the planContainer.id.
                planContainer.UId = uie.Id;
                SortedList parms = GetPlanContainerParms( planContainer, true );
                _da.ExecuteSP( "[synps].[api_planContainer_dml_ins]", parms, trans == null, trans );

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
            GetPlanContainerByUId( planContainer.UId );

            SqlTransaction trans = null;

            try
            {
                _da.OpenConnection();
                trans = _da.Connection.BeginTransaction();

                //update Suplex security element
                UIElement uie = GetSuplexUIElementFromPlanContainer( planContainer );
                uie = _splxDal.UpsertUIElement( uie, trans );

                //update the planContainer, no hook to the splx element on this pass
                SortedList parms = GetPlanContainerParms( planContainer, false );
                _da.ExecuteSP( "[synps].[api_planContainer_dml_upd]", parms, trans == null, trans );

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

        //todo: make recursive
        public void DeletePlanContainer(Guid planContainerUId)
        {
            //check RLS
            //--> GetPlanContainerByUId( planContainerUId, validateRls: true );

            SqlTransaction trans = null;

            try
            {
                _da.OpenConnection();
                trans = _da.Connection.BeginTransaction();

                SortedList parms = new sSortedList( "@PlanContainerUId", planContainerUId );
                _da.ExecuteSP( "[synps].[api_planContainer_dml_del]", parms, trans == null, trans );

                trans.Commit();
            }
            catch( Exception ex )
            {
                trans.Rollback();

                throw;
            }
            finally
            {
                _da.CloseConnection();
            }
        }


        UIElement GetSuplexUIElementFromPlanContainer(PlanContainer planContainer)
        {
            UIElement uie = new UIElement()
            {
                Name = planContainer.Name,
                UniqueName = planContainer.Name,
                ControlType = "SplxFileSystemManager",
                ParentId = planContainer.ParentUId.GetValueOrDefault()
            };

            if( !planContainer.IsNew )
                uie.Id = planContainer.UId;

            uie.SecurityDescriptor.DaclInherit =
                uie.SecurityDescriptor.SaclInherit = true;
            uie.SecurityDescriptor.SaclAuditTypeFilter = (ss.AuditType)31;

            return uie;
        }

        private SortedList GetPlanContainerParms(PlanContainer planContainer, bool forCreate)
        {
            SortedList parms = new SortedList
            {
                { "@PlanContainerUId", planContainer.UId },
                { "@Name", planContainer.Name },
                { "@Description", string.IsNullOrWhiteSpace( planContainer.Description ) ? Convert.DBNull : planContainer.Description },
                { "@NodeUri", string.IsNullOrWhiteSpace( planContainer.NodeUri ) ? Convert.DBNull : planContainer.NodeUri },
                { "@RlsOwner", planContainer.RlsOwner == Guid.Empty ? Convert.DBNull : planContainer.RlsOwner },
                { "@RlsMask" , planContainer.RlsMask ?? PlanContainerSecurity.GetEmptyRlsMask() },
                { "@ParentUId", !planContainer.ParentUId.HasValue || planContainer.ParentUId == Guid.Empty ? Convert.DBNull : planContainer.ParentUId }
            };


            if( forCreate )
                parms.Add( "@AuditCreatedBy", planContainer.AuditCreatedBy );
            else
                parms.Add( "@AuditModifiedBy", planContainer.AuditModifiedBy );

            return parms;
        }
        #endregion


        #region security
        public PlanContainerSecurity GetPlanContainerSecurity(Guid planContainerUId)
        {
            PlanContainer planContainer = GetPlanContainerByUId( planContainerUId );

            TrySecurityOrException( planContainer.Name,
                ss.AceType.FileSystem, ss.FileSystemRight.ReadPermissions, "PlanContainer", planContainer.RlsOwner );

            PlanContainerSecurity sec = new PlanContainerSecurity();
            sec.PlanContainerUId = planContainer.UId;
            sec.RlsOwner = planContainer.RlsOwner.ToString();

            List<SuplexAce> aces = GetPlanContainerAces( planContainerUId );
            foreach( SuplexAce ace in aces )
                sec.Permissions.Add( PermissionItem.FromAce( ace ) );

            return sec;
        }

        public void UpdatePlanContainerSecurity(PlanContainerSecurity sec)
        {
            PlanContainer planContainer = GetPlanContainerByUId( sec.PlanContainerUId );

            TrySecurityOrException( planContainer.Name,
                ss.AceType.FileSystem, ss.FileSystemRight.ChangePermissions, "PlanContainer", planContainer.RlsOwner );

            foreach( PermissionItem perm in sec.Permissions )
            {
                UpdatePlanContainerPermission( sec.PlanContainerUId, perm );
                //todo : make updaterls part of updatepermissionset.  execute under same sqltransaction
                UpdatePlanContainerRls( sec.PlanContainerUId );
            }
        }

        /// <summary>
        /// Assesses the Dacl from a PlanContainer and updates RlsMask to match
        /// </summary>
        /// <param name="planContainerUId">The PlanContainer to update; pass [null] for all PlanContainers.</param>
        /// <returns></returns>
        public int RecalculatePlanContainerRlsMask(Guid? planContainerUId = null)
        {
            if( planContainerUId.HasValue )
            {
                UpdatePlanContainerRls( planContainerUId.Value );
                return 1;
            }
            else
            {
                List<PlanContainer> pcs = GetPlanContainerByAny();
                foreach( PlanContainer pc in pcs )
                    UpdatePlanContainerRls( pc.UId );

                return pcs.Count;
            }
        }


        #region utility methods
        //todo: this really, really needs a transaction
        void UpdatePlanContainerPermission(Guid planContainerUId, PermissionItem perm)
        {
            SortedList parms = new sSortedList();

            if( perm.State == RecordState.Added || perm.State == RecordState.Modified )
            {
                parms.Add( "@SPLX_ACE_ID", perm.Id );
                parms.Add( "@ACE_TRUSTEE_USER_GROUP_ID", perm.GroupId );
                parms.Add( "@SPLX_UI_ELEMENT_ID", planContainerUId );
                parms.Add( "@ACE_ACCESS_MASK", perm.Rights );
                parms.Add( "@ACE_ACCESS_TYPE1", 1 );
                parms.Add( "@ACE_INHERIT", 1 );
                parms.Add( "@ACE_TYPE", "FileSystemAce" );

                _da.ExecuteSP( "splx.splx_api_upsert_ace", parms );
            }
            else if( perm.State == RecordState.Deleted )
            {
                parms.Add( "@SPLX_ACE_ID", perm.Id );
                _da.ExecuteSP( "splx.splx_api_del_ace", parms );
            }
        }

        byte[] UpdatePlanContainerRls(Guid planContainerUId)
        {
            PlanContainer planContainer = GetPlanContainerByUId( planContainerUId );

            //Calculate the combined bitmask value for all the Groups for the Aces
            List<SuplexAce> aces = GetPlanContainerAces( planContainerUId );
            List<byte[]> masks = new List<byte[]>();
            foreach( SuplexAce ace in aces )
                masks.Add( ace.GroupMask );

            planContainer.RlsMask = PlanContainerSecurity.CalculateMask( masks );

            UpdatePlanContainer( planContainer );

            return planContainer.RlsMask;
        }

        List<SuplexAce> GetPlanContainerAces(Guid planContainerUId)
        {
            //get the Aces for this PlanContainer, including the Groups for the Aces
            // - note: the SP below only selects valid aces: group.Enabled = true, ace.Allowed = true, ace.IsAuditAce = false
            SortedList parms = new SortedList();
            parms.Add( "@SPLX_UI_ELEMENT_ID", planContainerUId );
            DataSet ds = _da.GetDataSet( "[synps].[api_splxElement_AceGroups_sel]", parms );
            return SuplexAceFactory.LoadTable( ds.Tables[0] );
        }
        #endregion


        #region obsolete?
        [Obsolete( "not in use" )]
        void AddPermissionSet(Guid planContainerUId, Guid groupId, PermissionRole role)
        {
            PlanContainerSecurity sec = GetPlanContainerSecurity( planContainerUId );

            PermissionItem perm = new PermissionItem();
            perm.State = RecordState.Added;
            perm.GroupId = groupId;
            perm.Rights = PermissionUtility.RightsFromRole( role );

            sec.Permissions.Add( perm );

            UpdatePlanContainerSecurity( sec );
        }

        [Obsolete( "not in use" )]
        void ModifyPermissionSet(Guid planContainerUId, Guid groupId, PermissionRole role)
        {
            bool permFound = false;
            PlanContainerSecurity sec = GetPlanContainerSecurity( planContainerUId );
            foreach( PermissionItem perm in sec.Permissions )
            {
                if( perm.GroupId == groupId )
                {
                    perm.State = RecordState.Modified;
                    perm.Rights = PermissionUtility.RightsFromRole( role );
                    permFound = true;
                }
            }

            if( permFound )
                UpdatePlanContainerSecurity( sec );
            else
                throw new Exception( "Group Id [" + groupId + "] Does Not Have Any Permissions On PlanContainer Id [" + planContainerUId + "]" );
        }

        [Obsolete( "not in use" )]
        void DeletePermissionSet(Guid planContainerUId, Guid groupId)
        {
            bool permFound = false;
            PlanContainerSecurity sec = GetPlanContainerSecurity( planContainerUId );
            foreach( PermissionItem perm in sec.Permissions )
            {
                if( perm.GroupId == groupId )
                {
                    perm.State = RecordState.Deleted;
                    permFound = true;
                }
            }

            if( permFound )
                UpdatePlanContainerSecurity( sec );
            else
                throw new Exception( "Group Id [" + groupId + "] Does Not Have Any Permissions On PlanContainer Id [" + planContainerUId + "]" );
        }

        [Obsolete( "not in use" )]
        bool ExistsInActiveDirectory(string userName, string ldapRoot = null, string ldapAuthUser = null, string ldapAuthPassword = null)
        {
            bool exists = false;
            if( String.IsNullOrWhiteSpace( ldapRoot ) )
                ldapRoot = LdapRoot;

            String[] user = userName.Split( '\\' );
            String name = user[0];
            if( user.Length > 1 )
                name = user[1];

            if( !String.IsNullOrWhiteSpace( ldapRoot ) )
            {
                DirectoryEntry root = new DirectoryEntry( ldapRoot );
                if( !String.IsNullOrWhiteSpace( ldapAuthUser ) )
                {
                    root.Username = ldapAuthUser;
                    root.Password = ldapAuthPassword;
                }

                DirectorySearcher search = new DirectorySearcher( root );
                search.Filter = "sAMAccountName=" + name;
                search.PropertiesToLoad.Add( "sAMAccountName" );

                SearchResult sr = search.FindOne();
                if( sr != null )
                    exists = true;
            }

            return exists;
        }

        [Obsolete( "not in use" )]
        List<PlanContainer> GetPlanContainersBySuplexGroupRls(Guid splxGroupId)
        {
            SortedList parms = new sSortedList( "@SPLX_GROUP_ID", splxGroupId );
            DataTable t = _da.GetDataSet( "synps.api_planContainer_sel_group_rls", parms ).Tables[0];

            PlanContainerFactory factory = new PlanContainerFactory();
            List<PlanContainer> items = new List<PlanContainer>();
            foreach( DataRow r in t.Rows )
                items.Add( factory.CreateRecord( r ) );

            return items;
        }

        [Obsolete( "not in use" )]
        void DeletePlanContainerSuplexGroupRls(Guid splxGroupId)
        {
            this.TrySecurityOrException( "api_securityprincipaldlg_props", ss.AceType.Record, ss.RecordRight.Delete, "PlanContainer" );

            SortedList parms = new sSortedList( "@SPLX_GROUP_ID", splxGroupId );
            _da.ExecuteSP( "synps.api_planContainer_dml_del_group_rls", parms );
        }
        #endregion
        #endregion
    }

    public class PlanContainerFactory : SynapseRecordFactoryBase<PlanContainer>
    {
        public override PlanContainer CreateRecord(DataRow r)
        {
            PlanContainer planContainer = new PlanContainer
            {
                UId = r.GetColumnValueAsGuid( "PlanContainerUId" ),
                Name = r.GetColumnValueAsString( "Name" ),
                Description = r.GetColumnValueAsString( "Description" ),
                NodeUri = r.GetColumnValueAsString( "NodeUri" ),
                RlsOwner = r.GetColumnValueAsGuid( "RlsOwner" ),
                RlsMask = r.GetColumnValueAsByteArray( "RlsMask" ),
                ParentUId = r.IsDBNullOrValue<Guid?>( "ParentUId", null ),
                AuditCreatedBy = r.GetColumnValueAsString( "AuditCreatedBy" ),
                AuditCreatedTime = r.GetColumnValueAsDateTime( "AuditCreatedTime" ),
                AuditModifiedBy = r.GetColumnValueAsString( "AuditModifiedBy" ),
                AuditModifiedTime = r.GetColumnValueAsDateTime( "AuditModifiedTime" )
            };

            planContainer.InitialHashCode = planContainer.CurrentHashCode;

            return planContainer;
        }
    }
}