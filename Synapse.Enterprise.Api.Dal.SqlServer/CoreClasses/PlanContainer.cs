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

                UIElement uie = GetUIElementFromPlanContainer( planContainer );
                uie = _splxDal.UpsertUIElement( uie, trans );

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

                UIElement uie = GetUIElementFromPlanContainer( planContainer );
                uie = _splxDal.UpsertUIElement( uie, trans );

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

            if( !planContainer.IsNew )
                uie.Id = planContainer.UId;

            uie.SecurityDescriptor.DaclInherit =
                uie.SecurityDescriptor.SaclInherit = true;
            uie.SecurityDescriptor.SaclAuditTypeFilter = (ss.AuditType)31;

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
            if( planContainer.RlsMask == null ) parms.Add( "@RlsMask", ContainerSecurityRecord.GetEmptyRlsMask() ); else parms.Add( "@RlsMask", planContainer.RlsMask );
            if( planContainer.ParentUId == Guid.Empty ) parms.Add( "@ParentUId", Convert.DBNull ); else parms.Add( "@ParentUId", planContainer.ParentUId );

            if( forCreate )
                parms.Add( "@AuditCreatedBy", planContainer.AuditCreatedBy );
            else
                parms.Add( "@AuditModifiedBy", planContainer.AuditModifiedBy );

            return parms;
        }


        #region security
        public ContainerSecurityRecord GetPermissionSets(Guid containerId)
        {
            return GetSecurityRecord( containerId );
        }

        public ContainerSecurityRecord GetSecurityRecord(Guid containerId)
        {
            return GetPlanContainerSecurity( containerId );
        }

        public void UpdateSecurityRecord(ContainerSecurityRecord csr)
        {
            PlanContainer container = GetPlanContainerByUId( csr.ContainerId );
            foreach( PermissionSet perm in csr.Permissions )
            {
                UpdatePlanContainerPermission( container, csr.ContainerId, perm, csr.ContainerId );
                // TODO : Make UpdateRls Part Of UpdatePermissionSet.  Execute Under Same SQLTransaction
                UpdatePlanContainerRls( csr.ContainerId );
            }
        }

        public void AddPermissionSet(Guid containerId, Guid groupId, PermissionRole role)
        {
            ContainerSecurityRecord csr = GetSecurityRecord( containerId );

            PermissionSet perm = new PermissionSet();
            perm.State = RecordState.Added;
            perm.GroupId = groupId;
            perm.Rights = PermissionUtility.RightsFromRole( role );

            csr.Permissions.Add( perm );
            UpdateSecurityRecord( csr );
        }

        public void ModifyPermissionSet(Guid containerId, Guid groupId, PermissionRole role)
        {
            bool permFound = false;
            ContainerSecurityRecord csr = GetSecurityRecord( containerId );
            foreach( PermissionSet perm in csr.Permissions )
            {
                if( perm.GroupId == groupId )
                {
                    perm.State = RecordState.Modified;
                    perm.Rights = PermissionUtility.RightsFromRole( role );
                    permFound = true;
                }
            }

            if( permFound )
                UpdateSecurityRecord( csr );
            else
                throw new Exception( "Group Id [" + groupId + "] Does Not Have Any Permissions On PlanContainer Id [" + containerId + "]" );
        }

        public void DeletePermissionSet(Guid containerId, Guid groupId)
        {
            bool permFound = false;
            ContainerSecurityRecord csr = GetSecurityRecord( containerId );
            foreach( PermissionSet perm in csr.Permissions )
            {
                if( perm.GroupId == groupId )
                {
                    perm.State = RecordState.Deleted;
                    permFound = true;
                }
            }

            if( permFound )
                UpdateSecurityRecord( csr );
            else
                throw new Exception( "Group Id [" + groupId + "] Does Not Have Any Permissions On PlanContainer Id [" + containerId + "]" );
        }

        public int RecalculateAllRlsMasks()
        {
            List<PlanContainer> containers = GetPlanContainerByAny();
            foreach( PlanContainer container in containers )
            {
                Console.WriteLine( ">> Updating Rls Mask For PlanContainer [" + container.UId + "]." );
                RecalculateRlsMask( container.UId );
            }

            return containers.Count;
        }

        public byte[] RecalculateRlsMask(Guid containerId)
        {
            return UpdatePlanContainerRls( containerId );
        }

        public bool ExistsInActiveDirectory(string userName, string ldapRoot = null, string ldapAuthUser = null, string ldapAuthPassword = null)
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


        #region dal
        public ContainerSecurityRecord GetPlanContainerSecurity(Guid containerId)
        {
            ContainerSecurityRecord csr = new ContainerSecurityRecord();

            try
            {
                _da.OpenConnection();

                PlanContainer planContainer = GetPlanContainerByUId( containerId );

                this.TrySecurityOrException( planContainer.Name,
                    ss.AceType.FileSystem, ss.FileSystemRight.ReadPermissions, "PlanContainer", planContainer.RlsOwner );

                UIElement planUie = _splxDal.GetUIElementByIdDeep( containerId.ToString() );
                if( planUie == null )
                    throw new Exception( String.Format( "PlanContainer [{0}] Does Not Exist.", containerId ) );

                csr.ContainerId = planContainer.UId;
                csr.RlsOwner = planContainer.RlsOwner.ToString();

                foreach( AccessControlEntryBase ace in planUie.SecurityDescriptor.Dacl )
                    csr.Permissions.Add( PermissionSet.FromAce( ace ) );

                return csr;
            }
            catch
            {
                throw;
            }
            finally
            {
                _da.CloseConnection();
            }

        }

        public void UpdatePlanContainerPermission(PlanContainer container, Guid uiElementId, PermissionSet perm, Guid containerId)
        {
            this.TrySecurityOrException( container.Name,
                ss.AceType.FileSystem, ss.FileSystemRight.ChangePermissions, "PlanContainer", container.RlsOwner );

            SortedList parms = new sSortedList();

            if( perm.State == RecordState.Added || perm.State == RecordState.Modified )
            {
                parms.Add( "@SPLX_ACE_ID", perm.Id );
                parms.Add( "@ACE_TRUSTEE_USER_GROUP_ID", perm.GroupId );
                parms.Add( "@SPLX_UI_ELEMENT_ID", uiElementId );
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

        public byte[] UpdatePlanContainerRls(Guid containerId)
        {
            PlanContainer planContainer = GetPlanContainerByUId( containerId );

            this.TrySecurityOrException( planContainer.Name,
                ss.AceType.FileSystem, ss.FileSystemRight.ChangePermissions, "PlanContainer", planContainer.RlsOwner );

            //get the Aces for this PlanContainer, including the Groups for the Aces
            // - note: the SP below only selects valid aces: group.Enabled = true, ace.Allowed = true, ace.IsAuditAce = false
            SortedList parms = new SortedList();
            parms.Add( "@SPLX_UI_ELEMENT_ID", containerId );
            DataSet ds = _da.GetDataSet( "[synps].[api_splxElement_AceGroups_sel]", parms );
            List<SuplexAce> aces = SuplexAceFactory.LoadTable( ds.Tables[0] );

            //Calculate the combined bitmask value for all the Groups for the Aces
            List<byte[]> masks = new List<byte[]>();
            foreach( SuplexAce ace in aces )
                masks.Add( ace.GroupMask );

            planContainer.RlsMask = ContainerSecurityRecord.CalculateMask( masks );

            UpdatePlanContainer( planContainer );

            return planContainer.RlsMask;
        }
        #endregion




        #region rls calls
        public List<PlanContainer> GetPlanContainersBySuplexGroupRls(Guid splxGroupId)
        {
            SortedList parms = new sSortedList( "@SPLX_GROUP_ID", splxGroupId );
            DataTable t = _da.GetDataSet( "synps.api_planContainer_sel_group_rls", parms ).Tables[0];

            PlanContainerFactory factory = new PlanContainerFactory();
            List<PlanContainer> items = new List<PlanContainer>();
            foreach( DataRow r in t.Rows )
                items.Add( factory.CreateRecord( r ) );

            return items;
        }

        public void DeletePlanContainerSuplexGroupRls(Guid splxGroupId)
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
            PlanContainer planContainer = new PlanContainer();

            planContainer.UId = r.GetColumnValueAsGuid( "PlanContainerUId" );
            planContainer.Name = r.GetColumnValueAsString( "Name" );
            planContainer.Description = r.GetColumnValueAsString( "Description" );
            planContainer.NodeUri = r.GetColumnValueAsString( "NodeUri" );
            planContainer.RlsOwner = r.GetColumnValueAsGuid( "RlsOwner" );
            planContainer.RlsMask = r.GetColumnValueAsByteArray( "RlsMask" );
            planContainer.ParentUId = r.GetColumnValueAsGuid( "ParentUId" );
            planContainer.AuditCreatedBy = r.GetColumnValueAsString( "AuditCreatedBy" );
            planContainer.AuditCreatedTime = r.GetColumnValueAsDateTime( "AuditCreatedTime" );
            planContainer.AuditModifiedBy = r.GetColumnValueAsString( "AuditModifiedBy" );
            planContainer.AuditModifiedTime = r.GetColumnValueAsDateTime( "AuditModifiedTime" );

            planContainer.InitialHashCode = planContainer.CurrentHashCode;

            return planContainer;
        }
    }
}