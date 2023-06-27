using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpQueryPageTools : MpIQueryPageTools {
        #region Private Variable
        private int _totalCount = 0;
        #endregion

        #region Constants
        #endregion

        #region Interfaces

        #region MpIDbIdCollection Implementation
        public int TotalCount =>
            _totalCount;// - _idsToOmit.Count;

        public void SetTotalCount(int count) {
            _totalCount = count;
            MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);

        }

        #endregion

        #endregion

        #region Properties

        //public ObservableCollection<int> AllQueryIds { get; set; }

        #endregion

        #region Constructors
        public MpQueryPageTools() {
            //AllQueryIds = new ObservableCollection<int>();
            //AllQueryIds.CollectionChanged += _allQueryCopyItemIds_CollectionChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void _allQueryCopyItemIds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Reset) {

            }
        }
        #endregion

        #region Commands
        #endregion


    }
}
