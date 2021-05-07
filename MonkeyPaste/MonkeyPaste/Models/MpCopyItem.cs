using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xamarin.Forms;
using SQLite;
//using Windows.Storage;

namespace MonkeyPaste.Models {
    public class MpCopyItem : Base.MpDbObject, ICloneable {
        private static List<MpCopyItem> _AllCopyItemList = null;
        public static int TotalCopyItemCount = 0;
        #region Private Variables
        private object _itemData = null;

        #endregion

        #region Properties

        //public MpApp App { get; set; }

        //public MpUrl ItemUrl { get; set; }

        //public MpClient Client { get; set; }

        //public MpColor ItemColor { get; set; }

        public string Title { get; set; } = string.Empty;

        public MpCopyItemType CopyItemType { get; private set; } = MpCopyItemType.None;

        //public int ClientId { get; private set; } = 0;
        //public int AppId { get; set; } = 0;
        //public int IconId { get; set; } = 0;

        public DateTime CopyDateTime { get; set; }

        public int LineCount { get; set; } = 0;

        public int CharCount { get; set; } = 0;

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

        public int FileCount { get; set; } = 0;

        public Size ItemSize { get; set; } 

        public double DataSizeInMb { get; set; } = 0;

        public int RelevanceScore {
            get {
                return CopyCount + PasteCount;
            }
        }

        public string ItemDescription { get; set; }
                
        public string ItemPlainText { get; set; }

        public string ItemRichText { get; set; }

        //public BitmapSource ItemScreenshot { get; private set; }

        private string _itemCsv = string.Empty;
        public string ItemCsv { 
            get {
                return _itemCsv;
            }
            set {
                _itemCsv = value;
            }
        }

        public List<MpCopyItem> CompositeItemList { get; set; } = new List<MpCopyItem>();

        public int CompositeCopyItemId { get; set; } = 0;
        public int CompositeParentCopyItemId { get; set; } = -1;
        public int CompositeSortOrderIdx { get; set; } = -1;

        public bool IsCompositeParent {
            get {
                return CopyItemType == MpCopyItemType.Composite;
            }
        }

        public bool IsSubCompositeItem {
            get {
                return CompositeParentCopyItemId > 0;
            }
        }

        public object Clone() {
            throw new NotImplementedException();
        }
        #endregion

    }

    public enum MpCopyItemDetailType {
        None = 0,
        DateTimeCreated,
        DataSize,
        UsageStats
    }

    public enum MpCopyItemType {
        None = 0,
        RichText,
        Image,
        FileList,
        Composite,
        Csv //this is only used during runtime
    }
}
