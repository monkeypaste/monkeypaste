using MonkeyPaste.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MonkeyPaste {
    public class MpQueryPageTools : MpIDbIdCollection {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIDbIdCollection Implementation
        public int GetItemId(int queryIdx) {
            if (queryIdx < 0 || queryIdx >= AllQueryIds.Count) {
                return -1;
            }
            return AllQueryIds[queryIdx];
        }

        public int GetItemOffsetIdx(int itemId) {
            return AllQueryIds.IndexOf(itemId);
        }

        public void InsertId(int idx, int id) {
            if (idx < 0 || idx > AllQueryIds.Count) {
                // bad idx
                Debugger.Break();
                return;
            }
            if (idx == AllQueryIds.Count) {
                AllQueryIds.Add(id);
            } else {
                AllQueryIds.Insert(idx, id);
            }
        }
        public bool RemoveItemId(int itemId) {
            bool was_removed = AllQueryIds.Remove(itemId);
            return was_removed;
        }
        public bool RemoveIdx(int queryIdx) {
            if (queryIdx < 0 || queryIdx >= AllQueryIds.Count) {
                return false;
            }
            AllQueryIds.RemoveAt(queryIdx);
            return true;
        }
        #endregion

        #endregion

        #region Properties

        public ObservableCollection<int> AllQueryIds { get; private set; }

        #endregion

        #region Constructors
        public MpQueryPageTools() {
            AllQueryIds = new ObservableCollection<int>();
            AllQueryIds.CollectionChanged += _allQueryCopyItemIds_CollectionChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void _allQueryCopyItemIds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MpMessenger.SendGlobal(MpMessageType.TotalQueryCountChanged);
        }
        #endregion

        #region Commands
        #endregion


    }
}
