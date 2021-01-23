using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MpWpfApp {
    public enum MpCopyItemChangeType {
        None = 0,
        Add,
        Remove,
        Update,
        Move,
        Reset
    }

    public class MpCopyItemChangeEventArgs : EventArgs {
        public MpCopyItemChangeType ChangeType { get; set; } = MpCopyItemChangeType.None;
        public MpCopyItemChangeEventArgs(MpCopyItemChangeType changeType) {
            ChangeType = changeType;
        }
        public override string ToString() {
            return Enum.GetName(typeof(MpCopyItemChangeType), ChangeType);
        }
    }

    public class MpCopyItemDataProvider : INotifyPropertyChanged, IItemsProvider<MpCopyItem> {
        #region Private Variables
        private int _tagId = 0;
        #endregion

        #region Properties
        private int _count = 0;
        public int Count {
            get {
                return _count;
            }
            set {
                if(_count != value) {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }
        #endregion

        #region Event Definitions
        public event EventHandler CopyItemChanged;
        public virtual void OnCopyItemChanged(MpCopyItem ci, MpCopyItemChangeEventArgs args) => CopyItemChanged?.Invoke(ci, args);
        #endregion

        #region INotifyPropertyChanged Implementation
        public bool ThrowOnInvalidPropertyName { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName) {
                    throw new Exception(msg);
                } else {
                    Debug.Fail(msg);
                }
            }
        }
        #endregion

        #region Public Methods
        public MpCopyItemDataProvider(int tagId) {
            SetTagId(tagId);
        }

        public void SetTagId(int tagId) {
            _tagId = tagId;
        }

        public async Task<uint> GetStartIndexAsync(int copyItemId) {
            var allItemList = await GetCopyItemsByTagIdAsync();
            var itemList = allItemList.Where(x => x.CopyItemId == copyItemId).ToList();
            if(itemList != null && itemList.Count > 0) {
                return (uint)allItemList.IndexOf(itemList[0]);
            }
            return 0;
        }

        public async Task<int> GetCopyItemsByTagIdCountAsync() {
            var dt = await MpDb.Instance.ExecuteAsync(
                "select pk_MpCopyItemId from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
                new Dictionary<string, object> {
                        { "@tid", _tagId }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                return dt.Rows.Count;
            }
            return 0;
        }

        public IAsyncOperation<List<MpCopyItem>> GetCopyItemsByTagIdAsync(uint startIndex, uint maxNumberOfItems) {
            var copyItemList = new List<MpCopyItem>();
            var dt = MpDb.Instance.ExecuteAsync(
                "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid) order by pk_MpCopyItemId limit @mnoi offset @si",
                new Dictionary<string, object> {
                        { "@tid", _tagId },
                        { "@mnoi", maxNumberOfItems },
                        { "@si", startIndex }
                    }).Result;
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    copyItemList.Add(new MpCopyItem(dr));
                }
            }
            return (IAsyncOperation<List<MpCopyItem>>)copyItemList;
        }

        public async Task<List<MpCopyItem>> GetCopyItemsByTagIdAsync() {
            var copyItemList = new List<MpCopyItem>();
            var dt = await MpDb.Instance.ExecuteAsync(
                "select * from MpCopyItem where pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=@tid)",
                new Dictionary<string, object> {
                        { "@tid", _tagId }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    copyItemList.Add(new MpCopyItem(dr));
                }
            }
            return copyItemList;
        }
        #endregion

        #region IItemsProvider Implementation
        public int FetchCount() {
            throw new NotImplementedException();
        }

        public IList<MpCopyItem> FetchRange(int startIndex, int count) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
