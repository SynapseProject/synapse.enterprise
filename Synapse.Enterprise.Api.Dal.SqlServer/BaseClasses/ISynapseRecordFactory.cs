using System;
using System.Collections;
using System.Data;

using bits = Suplex.BitLib;

using Synapse.Services.Enterprise.Api.Common;

namespace Synapse.Services.Enterprise.Api.Dal
{
    public interface ISynapseRecordFactory
    {
        ISynapseRecord CreateSynapseRecord(DataRow r);
    }

    public interface ISynapseRecordFactory<T> : ISynapseRecordFactory where T : ISynapseRecord
    {
        T CreateRecord(DataRow r);
    }

    public abstract class SynapseRecordFactoryBase<T> : ISynapseRecordFactory<T> where T : ISynapseRecord
    {
        public virtual ISynapseRecord CreateSynapseRecord(DataRow r)
        {
            return CreateRecord( r );
        }

        public abstract T CreateRecord(DataRow r);

        public void LoadListFromTable(IList list, DataTable t, byte[] userMask = null)
        {
            if( userMask != null )
            {
                bits.BitArray usrMask = new bits.BitArray( userMask );
                int[] usrIndexes = usrMask.GetValueIndexes();

                foreach( DataRow row in t.Rows )
                {
                    ISynapseRecord item = CreateRecord( row );

                    bool hasAccess = true;
                    if( item is ISynapseSecureRecord && ((ISynapseSecureRecord)item).RlsMask != null )
                    {
                        bits.BitArray rowMask = new bits.BitArray( ((ISynapseSecureRecord)item).RlsMask );
                        hasAccess = rowMask.MatchesAtValueIndexes( usrMask, usrIndexes );
                    }

                    if( hasAccess )
                        list.Add( item );
                }
            }
            else
            {
                foreach( DataRow row in t.Rows )
                    list.Add( CreateRecord( row ) );
            }
        }

        public void LoadListFromTableRecursive(IList list, DataTable t,
            string parentColumnName, string sortColumnName = null,
            Guid? optionalStartId = null, string optionalStartIdColumnName = null, byte[] userMask = null)
        {
            bits.BitArray usrMask = null;
            int[] usrIndexes = null;
            if( userMask != null )
            {
                usrMask = new bits.BitArray( userMask );
                usrIndexes = usrMask.GetValueIndexes();
            }

            string sortExpression = !string.IsNullOrEmpty( sortColumnName )
                                        ? string.Format( "{0} ASC", sortColumnName )
                                        : string.Empty;

            DataRow[] topNodes = t.Select( parentColumnName + " IS NULL", sortExpression );
            if( topNodes.Length == 0 && optionalStartId != null )
            {
                topNodes = t.Select( string.Format( "{0} = '{1}'",
                    optionalStartIdColumnName, optionalStartId.Value ), sortExpression );
            }
            foreach( DataRow r in topNodes )
            {
                ISynapseHierRecord<T> item = CreateRecord( r ) as ISynapseHierRecord<T>;

                bool hasAccess = true;
                if( item is ISynapseSecureRecord && usrMask != null && ((ISynapseSecureRecord)item).RlsMask != null )
                {
                    bits.BitArray rowMask = new bits.BitArray( ((ISynapseSecureRecord)item).RlsMask );
                    hasAccess = rowMask.MatchesAtValueIndexes( usrMask, usrIndexes );
                }

                if( hasAccess )
                {
                    list.Add( item );
                    LoadTableRecursive( item, t, parentColumnName, sortExpression, usrMask, usrIndexes );
                }
            }
        }

        private void LoadTableRecursive(ISynapseHierRecord parentItem, DataTable dataTableToLoad,
            string parentColumnName, string sortExpression, bits.BitArray usrMask, int[] usrIndexes)
        {
            DataRow[] children = dataTableToLoad.Select(
                string.Format( "{0} = '{1}'", parentColumnName, parentItem.UId ), sortExpression );

            if( children.Length > 0 )
            {
                foreach( DataRow r in children )
                {
                    ISynapseHierRecord<T> child = CreateRecord( r ) as ISynapseHierRecord<T>;

                    bool hasAccess = true;
                    if( child is ISynapseSecureRecord && usrMask != null && ((ISynapseSecureRecord)child).RlsMask != null )
                    {
                        bits.BitArray rowMask = new bits.BitArray( ((ISynapseSecureRecord)child).RlsMask );
                        hasAccess = rowMask.MatchesAtValueIndexes( usrMask, usrIndexes );
                    }

                    if( hasAccess )
                    {
                        parentItem.Children.Add( child );
                        LoadTableRecursive( child, dataTableToLoad, parentColumnName, sortExpression, usrMask, usrIndexes );
                    }
                }
            }
        }
    }
}