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
        private List<int> _idsToOmit = new List<int>();
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
        public void Reset(bool isRequery) {
            if (!isRequery) {
                return;
            }
            _idsToOmit.Clear();
        }

        public int GetItemId(int queryIdx) {
            var cqt = Mp.Services.ContentQueryTools;
            if (queryIdx >= cqt.Offset && queryIdx < cqt.Offset + cqt.Limit) {
                return cqt.ContentIds.ElementAt(queryIdx - cqt.Offset);
            }
            return -1;
        }

        public int GetItemOffsetIdx(int itemId) {
            if (_idsToOmit.Any(x => x == itemId)) {
                return -1;
            }
            var cqt = Mp.Services.ContentQueryTools;
            if (cqt.ContentIds.Any(x => x == itemId)) {
                return cqt.Offset + cqt.ContentIds.IndexOf(itemId);
            }
            return -1;
        }

        public bool AddIdToOmit(int itemId) {
            int offset_idx = GetItemOffsetIdx(itemId);
            if (offset_idx >= 0) {
                SetTotalCount(Math.Max(0, _totalCount - 1));
            }
            if (!_idsToOmit.Contains(itemId)) {
                _idsToOmit.Add(itemId);
            }
            return offset_idx >= 0;
        }

        public bool RemoveIdToOmit(int itemId) {
            return _idsToOmit.Remove(itemId);
        }


        //public void InsertId(int idx, int id) {
        //    if (idx < 0 || idx > AllQueryIds.Count) {
        //        // bad idx
        //        MpDebug.Break();
        //        return;
        //    }
        //    if (idx == AllQueryIds.Count) {
        //        AllQueryIds.Add(id);
        //    } else {
        //        AllQueryIds.Insert(idx, id);
        //    }
        //    MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);
        //}
        //public bool RemoveIdx(int queryIdx) {
        //    if (queryIdx < 0 || queryIdx >= AllQueryIds.Count) {
        //        return false;
        //    }
        //    AllQueryIds.RemoveAt(queryIdx);
        //    MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);
        //    return true;
        //}

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
