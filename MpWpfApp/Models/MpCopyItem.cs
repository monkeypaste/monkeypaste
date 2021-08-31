using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpCopyItem : MpDbModelBase, ICloneable, MpISyncableDbObject {
        private static List<MpCopyItem> _AllCopyItemList = null;
        public static int TotalCopyItemCount = 0;
        #region Private Variables
        //private static int _CopyItemCount = 0;
        private object _itemData = null;
        #endregion

        #region Properties

        public int CopyItemId { get; set; } = 0;
        public Guid CopyItemGuid { get; set; }

        public int PreCopyItemId { get; set; } = 0;

        public int PostCopyItemId { get; set; } = 0;

        public MpApp App { get; set; }

        public MpUrl ItemUrl { get; set; }

        public string ItemColor { get; set; }

        public string Title { get; set; } = string.Empty;

        public MpCopyItemType CopyItemType { get; private set; } = MpCopyItemType.None;

        //public int ClientId { get; private set; } = 0;
        public int AppId { get; set; } = 0;
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

        public string ItemHtml { get; set; }

        public BitmapSource ItemScreenshot { 
            get {
                if(DbImageScreenshot == null) {
                    return null;
                }
                return DbImageScreenshot.DbImage;
            }
            private set {
                if(DbImageScreenshot == null) {
                    DbImageScreenshot = new MpDbImage();
                }
                DbImageScreenshot.DbImage = value;
            }
        }
        
        public int DbImageScreenshotId { get; set; }
        public MpDbImage DbImageScreenshot { get; set; }

        public MpEventEnabledFlowDocument ItemFlowDocument { get; set; }

        private string _itemCsv = string.Empty;
        public string ItemCsv { 
            get {
                if(CopyItemType == MpCopyItemType.Image) {
                    var outStr = string.Empty;
                    foreach (var obj in ImageItemObjectList) {
                        outStr += obj.ObjectTypeName + ",";
                    }
                    return string.IsNullOrEmpty(outStr) ? outStr : outStr.Remove(outStr.Length-1, 1);
                }
                return _itemCsv;
            }
            set {
                _itemCsv = value;
            }
        }

        public BitmapSource ItemBitmapSource {
            get {
                if(ItemDbImage == null) {
                    return null;
                }
                return ItemDbImage.DbImage;
            }
            set {
                if(ItemDbImage == null) {
                    ItemDbImage = new MpDbImage();
                }
                ItemDbImage.DbImage = value;
            }
        }

        public int ItemDbImageId { get; set; }
        public MpDbImage ItemDbImage { get; set; }

        //private BitmapSource _itemBitmapSource = null;
        //public BitmapSource ItemBitmapSource {
        //    get {
        //        switch (CopyItemType) {
        //            //case MpCopyItemType.FileList:
        //            //    return MpHelpers.Instance.ConvertRichTextToBitmapSource(MpHelpers.Instance.ConvertPlainTextToRichText((string)_itemData));
        //            case MpCopyItemType.Image:
        //                return (BitmapSource)_itemData;
        //            case MpCopyItemType.FileList:
        //            case MpCopyItemType.Composite:
        //            case MpCopyItemType.RichText:
        //                if(_itemBitmapSource == null) {
        //                    return new BitmapImage();
        //                }
        //                return _itemBitmapSource;
        //        }
        //        return new BitmapImage();
        //    }
        //    set {
        //        _itemBitmapSource = value;
        //        _itemBmpByteArray = _itemBitmapSource.ToByteArray();
        //    }
        //}

        //private byte[] _itemBmpByteArray = null;
        //public byte[] ItemBmpByteArray {
        //    get {
        //        return _itemBmpByteArray;
        //    }
        //}

        public List<MpDetectedImageObject> ImageItemObjectList = new List<MpDetectedImageObject>();

        public List<MpCopyItemTemplate> TemplateList {
            get {
                return MpCopyItemTemplate.GetAllTemplatesForCopyItem(CopyItemId);
            }
        }

        public string TemplateRegExMatchString {
            get {
                var outStr = string.Empty;
                foreach(var t in TemplateList) {
                    if(outStr.Contains(t.TemplateName)) {
                        continue;
                    }
                    outStr += t.TemplateName + "|";
                }
                if(!string.IsNullOrEmpty(outStr)) {
                    return outStr.Remove(outStr.Length - 1, 1);
                }
                return outStr;
            }
        }

        public bool HasOcrText {
            get {
                if(CopyItemType != MpCopyItemType.Image) {
                    return false;
                }
                if((ImageItemObjectList == null || ImageItemObjectList.Count == 0) && !string.IsNullOrEmpty(ItemPlainText)) {
                    return true;
                }
                return false;
            }
        }

        public List<MpCopyItem> CompositeItemList { get; set; } = new List<MpCopyItem>();

        //public int CompositeCopyItemId { get; set; } = 0;
        //public Guid CompositeCopyItemGuid { get; set; }
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
        #endregion

        #region Factory Methods
        public static List<MpCopyItem> GetPage(
            int tagId,
            int start,
            int count,
            string sortColumn = "pk_MpCopyItemId",
            bool isDescending = false) {
            //SELECT
            //user_number,
            //user_name
            //FROM user_table
            //WHERE(user_name LIKE '%{1}%' OR user_number LIKE '%{2}%')
            //AND user_category = { 3 } OR user_category = { 4 }
            //ORDER BY user_uid LIMIT { 5}
            //OFFSET { 6}
            //Where { 5} is page size and { 6 } is page number * page size.
            var result = MpDb.Instance.Execute(
                                string.Format(@"SELECT * from MpCopyItem " +
                                      "WHERE pk_MpCopyItemId in " +
                                        "(SELECT fk_MpCopyItemId FROM MpCopyItemTag " +
                                         "WHERE fk_MpTagId=@tid) " +
                                      "ORDER BY {0} {1} LIMIT {2} OFFSET {3}",
                                      sortColumn, 
                                      (isDescending ? "DESC" : "ASC"),
                                      count,
                                      start),
                                new Dictionary<string, object> {
                                    { "@tid",tagId }
                                });

            var cil = new List<MpCopyItem>();
            if(result != null) {
                foreach(var dr in result.Rows) {
                    cil.Add(new MpCopyItem((DataRow)dr));
                }
            }
            return cil;
        }

        public static async Task<MpCopyItem> CreateFromClipboardAsync(IntPtr processHandle, int remainingRetryCount = 5, DispatcherPriority priority = DispatcherPriority.Background) {
            MpCopyItem newCopyItem = null;
            await Application.Current.Dispatcher.BeginInvoke(
                    (Action)(() => {
                        newCopyItem = CreateFromClipboard(processHandle, remainingRetryCount);
                    }), priority);

            return newCopyItem;
        }

        public static MpCopyItem CreateFromClipboard(IntPtr processHandle, int remainingRetryCount = 5) {
            if(remainingRetryCount < 0) {
                Console.WriteLine("Retry count exceeded ignoring copy item");
                return null;
            }            
            try {
                if(processHandle == IntPtr.Zero) {
                    // since source is unknown set to this app
                    processHandle = MpClipboardManager.Instance.LastWindowWatcher.ThisAppHandle;
                }
                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) {
                    return null;
                }                
                object itemData = null;
                MpCopyItemType itemType = MpCopyItemType.None;
                
                if (iData.GetDataPresent(DataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = (string[])iData.GetData(DataFormats.FileDrop, true);
                } else if (iData.GetDataPresent(DataFormats.CommaSeparatedValue)) {
                    itemType = MpCopyItemType.Csv;
                    itemData = (string)iData.GetData(DataFormats.CommaSeparatedValue);
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    itemType = MpCopyItemType.RichText;
                    itemData = (string)iData.GetData(DataFormats.Rtf);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = (BitmapSource)Clipboard.GetImage();
                } else if ((iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) && !string.IsNullOrEmpty((string)iData.GetData(DataFormats.Text))) {
                    itemType = MpCopyItemType.RichText;
                    itemData = MpHelpers.Instance.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.UnicodeText));
                } else {
                    Console.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }
                if (itemType == MpCopyItemType.RichText && ((string)itemData).Length > Properties.Settings.Default.MaxRtfCharCount) {
                    itemData = MpHelpers.Instance.ConvertRichTextToPlainText((string)itemData);
                    if (((string)itemData).Length > Properties.Settings.Default.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        if (Properties.Settings.Default.NotificationShowCopyItemTooLargeToast) {
                            MpStandardBalloonViewModel.ShowBalloon(
                            "Item TOO LARGE",
                            $"Max Item Characters is {Properties.Settings.Default.MaxRtfCharCount} and copied item is {((string)itemData).Length} characters",
                            Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                        }  
                        return null;
                    }
                }

                if (Properties.Settings.Default.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.RichText && 
                    string.IsNullOrWhiteSpace(((string)itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }
                if (Properties.Settings.Default.IgnoreNewDuplicates) {
                    var dupItem = MpCopyItem.GetCopyItemByData(itemData);
                    if (dupItem != null) {
                        return dupItem;
                    }
                }

                return new MpCopyItem(itemType, itemData, processHandle);
            }
            catch (Exception e) {
                //this catches intermittent COMExceptions (happened copy/pasting in Excel)
                Console.WriteLine("Caught exception creating copyitem (will reattempt to open clipboard): " + e.ToString());
                return CreateFromClipboard(processHandle, remainingRetryCount - 1);
            }
        }
        
        public static async Task<MpCopyItem> CreateFromClipboardAsync(IntPtr processHandle, DispatcherPriority priority) {
            MpCopyItem newItem = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                newItem = CreateFromClipboard(processHandle);
            }, priority);
            return newItem;
        }        

        public static MpCopyItem CreateRandomItem(MpCopyItemType itemType) {
            switch(itemType) {
                case MpCopyItemType.RichText:
                    var ci =  new MpCopyItem(
                        MpCopyItemType.RichText,
                        MpHelpers.Instance.GetRandomString(10,1),
                        MpHelpers.Instance.ConvertPlainTextToRichText(
                            MpHelpers.Instance.GetRandomString(80, MpHelpers.Instance.Rand.Next(1, 100))),
                        MpHelpers.Instance.GetRandomColor(),
                        new WindowInteropHelper(Application.Current.MainWindow).Handle,null);//((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.ThisAppHandle);
                    //ci.WriteToDatabase();
                    return ci;
                    

            }
            return null;
        }

        public static int GetTotalItemCount() {
            var dt = MpDb.Instance.Execute(
                   "select pk_MpCopyItemId from MpCopyItem where fk_MpCopyItemTypeId is not 4", null);
            if (dt != null) {
                return dt.Rows.Count;
            }
            return 0;
        }

        public static List<MpCopyItem> GetAllCopyItems(out int count) {
            count = 0;
            if(_AllCopyItemList == null) {
                _AllCopyItemList = new List<MpCopyItem>();
                var dt = MpDb.Instance.Execute("select * from MpCopyItem order by CopyDateTime DESC", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        var ci = new MpCopyItem(dr);
                        if (!ci.IsSubCompositeItem) {
                            count++;
                        }
                        _AllCopyItemList.Add(ci);
                    }
                }
            } else {
                count = _AllCopyItemList.Where(x => x.CopyItemType != MpCopyItemType.Composite).ToList().Count;
            }
            return _AllCopyItemList;
        }

        public static MpCopyItem GetCopyItemById(int ciid) {
            if(ciid <= 0) {
                return null;
            }
            DataTable dt = MpDb.Instance.Execute(
                   "select * from MpCopyItem where pk_MpCopyItemId=@ciid",
                   new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", ciid }
                       });
            if (dt != null && dt.Rows.Count > 0) {
                return new MpCopyItem(dt.Rows[0]);
            }
            return null;
        }

        public static MpCopyItem GetCopyItemByData(object data) {
            if(data.GetType() == typeof(string[])) {
                var str = string.Empty;
                foreach(var s in (string[])data) {
                    str += s + Environment.NewLine;
                }
                DataTable dt = MpDb.Instance.Execute(
                   "select * from MpCopyItem where ItemText=@it",
                   new System.Collections.Generic.Dictionary<string, object> {
                            { "@it", str }
                       });
                if (dt != null && dt.Rows.Count > 0) {
                    return new MpCopyItem(dt.Rows[0]);
                }
                return null;
            }
            else if (data.GetType() == typeof(string)) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpCopyItem where ItemText=@it",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@it", (string)data }
                        });
                if (dt != null && dt.Rows.Count > 0) {
                    return new MpCopyItem(dt.Rows[0]);
                }
                return null;
            } else if(data is BitmapSource) {
                return null;
                // code below needs to be refactored to subquery dbimages for copyitem images
                //DataTable dt = MpDb.Instance.Execute(
                //    "select * from MpDbImage where pk_MpDbImageId=@iid and ImageBase64=@istr",
                //    new System.Collections.Generic.Dictionary<string, object> {
                //            { "@iid", ItemIm}
                //        });
                //if (dt != null && dt.Rows.Count > 0) {
                //    return new MpCopyItem(dt.Rows[0]);
                //}
            }
            return null;
        }

        public static MpCopyItem Merge(
            MpCopyItem fromItem,
            MpCopyItem toItem,
            bool useFileData = false,
            bool isFileDataMerged = false,
            int forceIdx = -1) {
            if(toItem == null) {
                return fromItem;
            }
            if (fromItem.CopyItemType == MpCopyItemType.Image || toItem.CopyItemType == MpCopyItemType.Image) {
                // for now, do not allow combining with image types
                return null;
            }

            if(fromItem == toItem) {
                return toItem;
            }
            MpCopyItem compositeItem = null;
            switch (fromItem.CopyItemType) {
                case MpCopyItemType.FileList:
                    switch(toItem.CopyItemType) {
                        case MpCopyItemType.FileList:
                            //FileList->FileList = FileList
                            var toPathList = toItem.GetFileList();
                            foreach (string fromPath in fromItem.GetFileList()) {
                                toPathList.Add(fromPath);
                            }
                            var toString = string.Empty;
                            foreach(var toPath in toPathList) {
                                toString += toPath + Environment.NewLine;
                            }
                            toItem.SetData(toString);
                            break;
                        case MpCopyItemType.RichText:
                            //FileList->RichText = RichText(of file path's or data) | Composite (with File Path's(or file data if supported and flagged) as rich text sub-composites(or single if flagged))
                            string fromText = string.Empty;
                            foreach (string fromPath in fromItem.GetFileList()) {
                                fromText += useFileData ? MpHelpers.Instance.ReadTextFromFile(fromPath) + Environment.NewLine : fromPath + Environment.NewLine;
                            }
                            var toRichText = MpHelpers.Instance.CombineRichText(fromText.ToRichText(), toItem.ItemRichText);
                            toItem.SetData(toRichText);
                            break;
                        case MpCopyItemType.Composite:
                            //FileList->Composite = Composite(each file path or data as sub - composite or merged based on flags)
                            break;
                    }
                    foreach (var tag in MpTag.GetAllTags()) {
                        if (tag.IsLinkedWithCopyItem(fromItem)) {
                            tag.LinkWithCopyItem(toItem);
                        }
                    }
                    fromItem.DeleteFromDatabase();
                    toItem.WriteToDatabase();
                    return toItem;
                //case MpCopyItemType.Image:
                //    SetData(
                //        MpHelpers.Instance.CombineBitmap(
                //            new List<BitmapSource> {
                //                ItemBitmapSource,
                //                fromItem.ItemBitmapSource}));
                //    break;
                case MpCopyItemType.RichText:
                    switch(toItem.CopyItemType) {
                        case MpCopyItemType.None:
                            return fromItem;
                        case MpCopyItemType.FileList:
                            //RichText->FileList = FileList(with RichText as file w / title at eol)

                            break;
                        case MpCopyItemType.RichText:
                            //RichText->RichText = RichText | Composite
                            compositeItem = new MpCopyItem(
                                MpCopyItemType.Composite, 
                                toItem.Title, 
                                null, 
                                MpHelpers.Instance.ConvertHexToColor(toItem.ItemColor), 
                                IntPtr.Zero, 
                                toItem.App);
                            compositeItem.WriteToDatabase();
                            toItem.CompositeParentCopyItemId = compositeItem.CopyItemId;
                            toItem.CompositeSortOrderIdx = 0;
                            compositeItem.CompositeItemList.Add(toItem);
                            break;
                        case MpCopyItemType.Composite:
                            //RichText->Composite = Composite
                            compositeItem = toItem;
                            break;
                    }
                    fromItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(MpHelpers.Instance.GetRandomColor());
                    fromItem.CompositeParentCopyItemId = compositeItem.CopyItemId;
                    fromItem.CompositeSortOrderIdx = compositeItem.CompositeItemList.Count;
                    compositeItem.CompositeItemList.Add(fromItem);
                    if(forceIdx >= 0) {
                        compositeItem.CompositeItemList.Move(fromItem.CompositeSortOrderIdx, forceIdx);
                        foreach(var cci in compositeItem.CompositeItemList) {
                            cci.CompositeSortOrderIdx = compositeItem.CompositeItemList.IndexOf(cci);
                        }
                    }
                    compositeItem.SetData(null);
                    compositeItem.WriteToDatabase();
                    return compositeItem;
                case MpCopyItemType.Composite:
                    switch(toItem.CopyItemType) {
                        case MpCopyItemType.FileList:
                            //Composite->FileList = FileList(each sub - composite is file path stored in a userdata folder or is ONE file in userdata folder if merge is flagged)
                            break;
                        case MpCopyItemType.RichText:
                            //Composite->RichText = RichText
                            compositeItem = new MpCopyItem(MpCopyItemType.Composite, toItem.Title, null, MpHelpers.Instance.ConvertHexToColor(toItem.ItemColor), IntPtr.Zero, toItem.App);
                            compositeItem.WriteToDatabase();
                            toItem.CompositeParentCopyItemId = compositeItem.CopyItemId;
                            toItem.CompositeSortOrderIdx = 0;
                            compositeItem.CompositeItemList.Add(toItem);                                                      
                            break;
                        case MpCopyItemType.Composite:
                            // Composite->Composite = Composite
                            compositeItem = toItem;
                            break;
                    }
                    foreach (var fci in fromItem.CompositeItemList.OrderByDescending(x => x.CompositeSortOrderIdx)) {
                        // NOTE sorting by desc sort order so 
                        fci.CompositeParentCopyItemId = compositeItem.CopyItemId;
                        fci.CompositeSortOrderIdx = compositeItem.CompositeItemList.Count;
                        if (forceIdx < 0 || forceIdx >= compositeItem.CompositeItemList.Count) {
                            //add composite item AFTER rtf item by default and if forceIdx is too high ignore it
                            compositeItem.CompositeItemList.Add(fci);
                            //manually set forceIdx so next item is added BEFORE this item
                            forceIdx = 1;
                        } else {
                            compositeItem.CompositeItemList.Insert(forceIdx, fci);
                        }
                    }
                    if (forceIdx >= 0) {
                        //reset sortOrderIdx by actual idx
                        foreach (var cci in compositeItem.CompositeItemList) {
                            cci.CompositeSortOrderIdx = compositeItem.CompositeItemList.IndexOf(cci);
                        }
                    }

                    fromItem.CompositeItemList.Clear();
                    if (fromItem.CopyItemType == MpCopyItemType.Composite) {
                        fromItem.DeleteFromDatabase();
                    }
                    compositeItem.SetData(null);
                    compositeItem.WriteToDatabase();
                    return compositeItem;
            }
            //if can't combine don't alter fromItem and just return toItem;
            return toItem;
        }

        public static async Task<MpCopyItem> MergeAsync(
            MpCopyItem fromItem, 
            MpCopyItem toItem,
            bool useFileData = false,
            bool isFileDataMerged = false,
            int forceIdx = -1,
            DispatcherPriority priority = DispatcherPriority.Background) {
            MpCopyItem mergedItem = null;
            await Application.Current.Dispatcher.BeginInvoke((Action)(
                () => {
                    mergedItem = MpCopyItem.Merge(
                        fromItem, 
                        toItem, 
                        useFileData, 
                        isFileDataMerged, 
                        forceIdx);
                }), priority);
            return mergedItem;
        }
        #endregion

        #region Public Methods

        #region Constructors
        public MpCopyItem() { }

        public MpCopyItem(
            MpCopyItemType itemType,
            object itemData,
            IntPtr processHandle) : this(
                itemType,
                itemType == MpCopyItemType.RichText || itemType == MpCopyItemType.Csv ? "Text" : Enum.GetName(typeof(MpCopyItemType), itemType),
                itemData,
                MpHelpers.Instance.GetRandomColor(),
                processHandle,
                null) { }            

        public MpCopyItem(
            MpCopyItemType itemType,
            string title,
            object data,
            Color tileColor,
            IntPtr hwnd,
            MpApp app) {
            CopyItemGuid = Guid.NewGuid();
            CopyItemType = itemType;
            CopyDateTime = DateTime.Now;
            if(string.IsNullOrEmpty(title)) {
                Title = CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Csv ? "Text" : Enum.GetName(typeof(MpCopyItemType), CopyItemType);
            } else {
                Title = title;
            }
            CopyCount = 1;

            if(hwnd != IntPtr.Zero) {
                //occurs for items added from clipboard
                App = MpApp.GetAppByHandle(hwnd);
                if (App == null) {
                    App = MpApp.Create(hwnd);
                    //MpApp.GetAllApps().Add(App);
                } 

                if (Properties.Settings.Default.DoFindBrowserUrlForCopy) {
                    if (MpRunningApplicationManager.Instance.ActiveProcessPath == Properties.Settings.Default.UserDefaultBrowserProcessPath) {
                        DbImageScreenshot = new MpDbImage();
                        ItemScreenshot = MpHelpers.Instance.CopyScreen();
                    }
                }
            } else if(app != null) {
                App = app;
            }

            ItemColor = MpHelpers.Instance.ConvertColorToHex(tileColor);
            ItemDbImage = new MpDbImage();
            DbImageScreenshot = new MpDbImage();

            //ItemTitleSwirl = InitSwirl();
            ImageItemObjectList = new List<MpDetectedImageObject>();

            switch (itemType) {
                case MpCopyItemType.Csv:
                    ItemCsv = (string)data;
                    CopyItemType = MpCopyItemType.RichText;
                    SetData(MpHelpers.Instance.ConvertPlainTextToRichText(ItemCsv));
                    break;
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                    }
                    SetData(paths);
                    break;
                case MpCopyItemType.Image:
                    ItemDbImage = new MpDbImage((BitmapSource)data);
                    SetData((BitmapSource)data);
                    break;
                case MpCopyItemType.RichText:                    
                    SetData((string)data);
                    break;
            }
        }

        public MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }
        #endregion

        public void SetData(object data) {
            MpHelpers.Instance.RunOnMainThread(() => {
                _itemData = data;
                UpdateItemData();
                UpdateDetails();
            });
        }
        public object GetData() {
            return _itemData;
        }        

        public void UnlinkCompositeChild(MpCopyItem cci) {
            if (!IsCompositeParent || !CompositeItemList.Contains(cci)) {
                return;
            }
            cci.UnlinkFromCompositeParent();
            CompositeItemList.Remove(cci);
            WriteToDatabase();
        }

        public MpCopyItem LinkCompositeChild(MpCopyItem cci, int forceIdx = -1) {
            if(CopyItemType != MpCopyItemType.RichText && CopyItemType != MpCopyItemType.Composite) {
                return cci;
            }
            if (CompositeItemList.Contains(cci) && CompositeItemList.IndexOf(cci) == forceIdx) {
                return cci;
            }
            cci.UnlinkFromCompositeParent();

            var newItem = MpCopyItem.Merge(cci,this,false,false,forceIdx);
            return newItem;
            //if(CopyItemType == MpCopyItemType.RichText) {
            //    pcci = new MpCopyItem(MpCopyItemType.Composite, Title, null, ItemColor.Color, IntPtr.Zero, App);
            //    pcci.WriteToDatabase();
            //    pcci.CompositeItemList.Add(this);
            //    CompositeCopyItemId = 0;
            //    CompositeParentCopyItemId = pcci.CopyItemId;
            //    CompositeSortOrderIdx = 0;
            //} else {
            //    pcci = this;
            //}
            
            //cci.CompositeCopyItemId = 0;
            //cci.CompositeParentCopyItemId = pcci.CopyItemId;
            //if(forceIdx >= 0) {
            //    if(forceIdx >= pcci.CompositeItemList.Count) {
            //        pcci.CompositeItemList.Add(cci);
            //    } else {
            //        pcci.CompositeItemList.Insert(forceIdx, cci);
            //    }
            //} else {
            //    pcci.CompositeItemList.Add(cci);
            //}
            //cci.CompositeSortOrderIdx = pcci.CompositeItemList.IndexOf(cci);
            //pcci.WriteToDatabase();

            //return cci;
        }

        public string GetDetail(MpCopyItemDetailType detailType) {
            string info = "I dunno";// string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc
                    info = "Copied " + CopyDateTime.ToString();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItemType == MpCopyItemType.Image) {
                        info = "("+(int)ItemBitmapSource.Width + "px) x (" + (int)ItemBitmapSource.Height + "px)";
                    } else if (CopyItemType == MpCopyItemType.RichText) {
                        info = CharCount + " chars | " + LineCount + " lines";
                    } else if (CopyItemType == MpCopyItemType.Composite) {
                        info = CompositeItemList.Count + " items | " + CharCount + " chars | " + LineCount + " lines";
                    } else if (CopyItemType == MpCopyItemType.FileList) {
                        info = FileCount + " files | " + DataSizeInMb + " MB";
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    info = CopyCount + " copies | " + PasteCount + " pastes";
                    break;
                default:
                    info = "Unknown detailId: " + (int)detailType;
                    break;
            }

            return info;
        }

        public MpCopyItemTemplate GetTemplateByName(string templateName) {
            foreach(var t in TemplateList) {
                if(t.TemplateName == templateName) {
                    return t;
                }
            }
            return null;
        }

        public int GetPasteCount() {
            if (CopyItemId <= 0) {
                return 0;
            }
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpPasteHistory where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            if (dt == null) {
                return 0;
            }
            return dt.Rows.Count;
        }

        public MpEventEnabledFlowDocument GetSeparatedCompositeFlowDocument(string separatorChar = "- ") {
            if (CopyItemType != MpCopyItemType.Composite) {
                return ItemFlowDocument;
            }
            int maxCols = int.MinValue;
            foreach (var cci in CompositeItemList) {
                maxCols = Math.Max(maxCols, MpHelpers.Instance.GetColCount(cci.ItemPlainText));
            }
            string separatorLine = string.Empty;
            for (int i = 0; i < maxCols; i++) {
                separatorLine += separatorChar;
            }
            var separatorDocument = separatorLine.ToRichText().ToFlowDocument();
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            for (int i = 0; i < CompositeItemList.Count; i++) {
                var cci = CompositeItemList[i];
                if (i != 0) {
                    MpHelpers.Instance.CombineFlowDocuments(
                    separatorDocument,
                    fullDocument,
                    false);
                }
                MpHelpers.Instance.CombineFlowDocuments(
                    cci.ItemFlowDocument,
                    fullDocument,
                    false);
            }

            var ps = fullDocument.GetDocumentSize();
            fullDocument.PageWidth = ps.Width;
            fullDocument.PageHeight = ps.Height;
            return fullDocument;
        }

        public string GetCompositeItemRichText() {
            if(CopyItemType != MpCopyItemType.Composite) {
                return string.Empty;
            }
            var itemRichText = MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty);
            foreach(var cci in CompositeItemList) {
                itemRichText = MpHelpers.Instance.CombineRichText(cci.ItemRichText, itemRichText, false);
            }
            return itemRichText;
        }

        //public int GetCompositeParentCopyItemId() {
        //    //returns -1 if item is not a composite
        //    //returns 0 if item is the parent composite item (so ITS pk is the ParentId for its children)
        //    //returns N if item is part of a composite item
        //    if(CopyItemId <= 0) {
        //        return -1;
        //    }
        //    //first check if it is a composite item
        //    var dt = MpDb.Instance.Execute(
        //            "select * from MpCompositeCopyItem where fk_ParentMpCopyItemId=@ciid",
        //            new System.Collections.Generic.Dictionary<string, object> {
        //                    { "@ciid", CopyItemId }
        //                });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        return 0;
        //    }
        //    dt = MpDb.Instance.Execute(
        //            "select * from MpCompositeCopyItem where fk_MpCopyItemId=@ciid",
        //            new System.Collections.Generic.Dictionary<string, object> {
        //                    { "@ciid", CopyItemId }
        //                });
        //    if (dt != null && dt.Rows.Count > 0) {
        //        return Convert.ToInt32(dt.Rows[0]["fk_ParentMpCopyItemId"].ToString());
        //    }

        //    return -1;
        //}

        public List<string> GetFileList(string baseDir = "",MpCopyItemType forceType = MpCopyItemType.None) {
            //returns path of tmp file for rt or img and actual paths of filelist
            var fileList = new List<string>();
            if (CopyItemType == MpCopyItemType.FileList) {
                if(forceType == MpCopyItemType.Image) {
                    fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource));
                } else if (forceType == MpCopyItemType.RichText) {
                    fileList.Add(MpHelpers.Instance.WriteTextToFile(Path.GetTempFileName(), ItemPlainText));
                } else {
                    var splitArray = ItemPlainText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitArray == null || splitArray.Length == 0) {
                        throw new Exception("CopyItem GetFileList error, file list should not be empty");
                    } else {
                        foreach (string p in splitArray) {
                            if (!string.IsNullOrEmpty(p.Trim())) {
                                fileList.Add(p);
                            }
                        }
                    }
                }
            } else {
                string op = Path.GetTempFileName();// MpHelpers.Instance.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType:forceType),Title,baseDir);
                //file extension
                switch (CopyItemType) {
                    case MpCopyItemType.RichText:
                        if(forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, ItemBitmapSource));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, ItemRichText));
                        }
                        break;
                    case MpCopyItemType.Composite:
                        foreach(var cci in CompositeItemList) {
                            if (forceType == MpCopyItemType.Image) {
                                fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, cci.ItemBitmapSource));
                            } else {
                                fileList.Add(MpHelpers.Instance.WriteTextToFile(op, cci.ItemRichText));
                            }
                            op = Path.GetTempFileName(); //MpHelpers.Instance.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType : forceType), Title, baseDir);
                        }
                        break;
                    case MpCopyItemType.Image:
                        if (forceType == MpCopyItemType.RichText) {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, ItemPlainText));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, ItemBitmapSource));
                        }
                        break;
                }
            }

            if(string.IsNullOrEmpty(baseDir) && Application.Current.MainWindow.DataContext != null) {
                //for temporary files add to mwvm list for shutdown cleanup
                foreach(var fp in fileList) {
                    ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).AddTempFile(fp);
                }
            }
            // add temp files to 
            return fileList;
        }

        public async Task GatherAnalytics() {
            if(ItemScreenshot == null) {
                return;
            }
            await Task.Delay(1);
        }
        #endregion

        #region Private Methods
        private void UnlinkFromCompositeParent() {
            if (!IsSubCompositeItem) {
                return;
            }
            //MpDb.Instance.ExecuteWrite(
            //    "delete from MpCompositeCopyItem where fk_MpCopyItemId=@ciid",
            //    new System.Collections.Generic.Dictionary<string, object> {
            //            { "@ciid", CopyItemId }
            //        });
            //CompositeCopyItemId = 0;
            CompositeParentCopyItemId = -1;
            CompositeSortOrderIdx = -1;
            WriteToDatabase();
        }

        private void UpdateItemData() {
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                    ItemPlainText = (string)_itemData;
                    ItemRichText = ItemPlainText.ToRichText();
                    ItemFlowDocument = ItemRichText.ToFlowDocument();
                    var csvText = string.Empty;
                    foreach (var path in GetFileList()) {
                        csvText += path + ", ";
                    }
                    if (csvText.Length > 2) {
                        csvText = csvText.Remove(csvText.Length - 2, 2);
                    }
                    ItemCsv = csvText;
                    ItemBitmapSource = ItemFlowDocument.ToBitmapSource();

                    break;
                case MpCopyItemType.Image:
                    //if(_itemBmpByteArray == null) {
                    //    _itemBmpByteArray = ItemBitmapSource.ToByteArray();
                    //}
                    if (!string.IsNullOrEmpty(ItemPlainText)) {
                        ItemRichText = ItemPlainText.ToRichText();
                        ItemFlowDocument = ItemRichText.ToFlowDocument();
                    }
                    break;
                case MpCopyItemType.RichText:
                    ItemRichText = (string)_itemData;
                    ItemFlowDocument = ItemRichText.ToFlowDocument();
                    ItemPlainText = ItemRichText.ToPlainText();
                    if (Properties.Settings.Default.ShowItemPreview) {
                        ItemBitmapSource = ItemFlowDocument.ToBitmapSource();
                    }

                    if (!string.IsNullOrEmpty(ItemCsv) && MpHelpers.Instance.IsStringCsv(ItemPlainText)) {
                        //this is when copying from excel or DataObject type supports csv
                        ItemCsv = ItemPlainText;
                    }
                    break;
                case MpCopyItemType.Composite:
                    //_itemData is null and needs to be gathered from sub-items
                    _itemData = null;
                    ItemRichText = string.Empty.ToRichText();
                    ItemFlowDocument = ItemRichText.ToFlowDocument();
                    ItemPlainText = ItemRichText.ToPlainText();
                    ItemBitmapSource = GetSeparatedCompositeFlowDocument().ToBitmapSource();
                    break;
            }
        }

        private DispatcherOperation UpdateItemDataAsync(DispatcherPriority priority = DispatcherPriority.Background) {
            return Dispatcher.CurrentDispatcher.InvokeAsync(UpdateItemData,priority);          
        }

        private void UpdateDetails() {
            switch(CopyItemType) {
                case MpCopyItemType.Image:
                    if(ItemBitmapSource != null) {
                        ItemSize = new Size(ItemBitmapSource.Width, ItemBitmapSource.Height);
                    }
                    break;
                case MpCopyItemType.FileList:
                    FileCount = GetFileList().Count;
                    DataSizeInMb = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.Composite:
                    LineCount = 0;
                    CharCount = 0;
                    foreach (var sci in CompositeItemList) {
                        LineCount += MpHelpers.Instance.GetRowCount(sci.ItemPlainText);
                        CharCount += sci.ItemPlainText.Length;
                    }
                    ItemSize = GetCompositeItemRichText().ToFlowDocument().GetDocumentSize();
                    break;
                case MpCopyItemType.RichText:
                    LineCount = MpHelpers.Instance.GetRowCount(ItemPlainText);
                    CharCount = ItemPlainText.Length;
                    ItemSize = ItemFlowDocument.GetDocumentSize();
                    break;
            }
        }
        
        private List<int> GetCompsiteCopyItemIdBySortOrderList() {
            if(CopyItemType != MpCopyItemType.Composite) {
                return null;
            }
            var dt = MpDb.Instance.Execute(
                    "select pk_MpCopyItemId from MpCopyItem where fk_ParentCopyItemId=@ciid order by CompositeSortOrderIdx ASC",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", CopyItemId }
                        });

            var copyItemIdList = new List<int>();
            if (dt != null && dt.Rows.Count > 0) {
                foreach(DataRow dr in dt.Rows) {
                    copyItemIdList.Add(Convert.ToInt32(dr["pk_MpCopyItemId"].ToString()));
                }
            }
            return copyItemIdList;
        }
        #endregion

        #region Overrides

        public override void LoadDataRow(DataRow dr) {
            CopyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            CopyItemGuid = Guid.Parse(dr["MpCopyItemGuid"].ToString());
            CopyItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            int appId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            ItemColor = dr["HexColor"].ToString();
            CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            Title = dr["Title"].ToString();
            CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());
            ItemDescription = dr["ItemDescription"].ToString();
            ItemRichText = dr["ItemRtf"].ToString();
            ItemHtml = dr["ItemHtml"].ToString();
            PasteCount = Convert.ToInt32(dr["PasteCount"].ToString());

            ItemDbImageId = Convert.ToInt32(dr["fk_MpDbImageId"].ToString());
            ItemDbImage = new MpDbImage(ItemDbImageId);

            DbImageScreenshotId = Convert.ToInt32(dr["fk_SsMpDbImageId"].ToString());
            DbImageScreenshot = new MpDbImage(DbImageScreenshotId);

            ItemCsv = dr["ItemCsv"].ToString();

            App = MpApp.GetAppById(appId);

            if (CopyItemType == MpCopyItemType.Image) {
                ItemPlainText = dr["ItemText"].ToString();
                //SetData(MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
                //ItemCsv = ItemPlainText;
            } else if(CopyItemType == MpCopyItemType.Composite) {
                var compositeItemIdList = GetCompsiteCopyItemIdBySortOrderList();
                foreach (int ciid in compositeItemIdList) {
                    CompositeItemList.Add(MpCopyItem.GetCopyItemById(ciid));
                }                
                //itemData gathered in UpdateItemData
                SetData(null);
            } else {
                SetData(dr["ItemText"].ToString());
                
            }

            //if (dr["ItemImage"] != null && dr["ItemImage"].GetType() != typeof(System.DBNull)) {
                //_itemBmpByteArray = (byte[])dr["ItemImage"];
                //ItemBitmapSource = MpHelpers.Instance.ConvertByteArrayToBitmapSource(_itemBmpByteArray);
            //}

            if (dr["fk_MpUrlId"] != null && dr["fk_MpUrlId"].GetType() != typeof(System.DBNull)) {
                int urlId = Convert.ToInt32(dr["fk_MpUrlId"].ToString());
                ItemUrl = MpUrl.GetUrlById(urlId);
            }

            CompositeParentCopyItemId = Convert.ToInt32(dr["fk_ParentCopyItemId"].ToString());
            if (CompositeParentCopyItemId > 0) {
                CompositeSortOrderIdx = Convert.ToInt32(dr["CompositeSortOrderIdx"].ToString());
            }
            PasteCount = GetPasteCount();
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (CopyItemId <= 0) {
                return;
            }
            var phl = MpPasteHistory.GetPasteHistoryByCopyItemId(CopyItemId);
            foreach(var ph in phl) {
                ph.DeleteFromDatabase(sourceClientGuid,ignoreTracking,ignoreSyncing);
            }
            
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItem where pk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    }, CopyItemGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            MpDb.Instance.ExecuteWrite(
                "delete from MpDetectedImageObject where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });

            var citl = MpCopyItemTag.GetCopyItemTagsByCopyItemId(CopyItemId);
            foreach (var cit in citl) {
                cit.DeleteFromDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);
            }
            

            MpDb.Instance.ExecuteWrite(
                "delete from MpShortcut where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });

            var tl = MpCopyItemTemplate.GetAllTemplatesForCopyItem(CopyItemId);
            foreach (var cit in tl) {
                cit.DeleteFromDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);
            }


            if (CopyItemType == MpCopyItemType.Composite) {
                foreach (var cci in CompositeItemList) {
                    cci.DeleteFromDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);
                }
            }
            var cil = GetAllCopyItems(out int count).Where(x => x.CopyItemId == CopyItemId).ToList();
            if (cil.Count > 0) {
                _AllCopyItemList.RemoveAt(_AllCopyItemList.IndexOf(cil[0]));
            }
        }

        // still req'd if NoDb=true
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {            
            if(CopyItemId < 0) {
                //when CopyItemId == -1 it means its a placeholder clip tile and shouldn't affect database
                return;
            }
            var sw = new Stopwatch();
            sw.Start();
            if(ItemScreenshot != null) {
                DbImageScreenshot.WriteToDatabase();
                DbImageScreenshotId = DbImageScreenshot.DbImageId;
            }

            if(ItemBitmapSource != null) {
                //when preview is turned off will not happen
                ItemDbImage.WriteToDatabase();
                ItemDbImageId = ItemDbImage.DbImageId;
            }
            if(App == null) {
                if(AppId > 0) {
                    App = MpApp.GetAppById(AppId);
                }
            }            
           
            if(App != null) {
                App.WriteToDatabase();
            }
            Title = string.IsNullOrEmpty(Title) ? string.Empty : Title;

            string itemText = (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite) ? ItemRichText : ItemPlainText;
            if(string.IsNullOrEmpty(itemText)) {
                itemText = string.Empty;
            }

            //byte[] itemImage = MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemBitmapSource);
            //if copyitem already exists
            if (CopyItemId > 0) {
                MpDb.Instance.ExecuteWrite(
                        "update MpCopyItem set fk_ParentCopyItemId=@cpciid, CompositeSortOrderIdx=@csoi,MpCopyItemGuid=@cig, PasteCount=@pc, fk_MpUrlId=@uid, ItemDescription=@id, ItemCsv=@icsv, fk_MpCopyItemTypeId=@citd, fk_MpAppId=@aid, HexColor=@clrId, Title=@t, CopyCount=@cc, ItemText=@it, ItemRtf=@irtf, ItemHtml=@ihtml, fk_MpDbImageId=@ii where pk_MpCopyItemId=@ciid",
                        new Dictionary<string, object> {
                            { "@cpciid", CompositeParentCopyItemId },
                            { "@csoi", CompositeSortOrderIdx },
                            { "@cig",CopyItemGuid.ToString() },
                            { "@pc", PasteCount },
                            { "@uid", ItemUrl == null ? 0:ItemUrl.UrlId },
                            { "@id", ItemDescription },
                            { @"icsv",ItemCsv },
                            { "@citd", (int)CopyItemType },
                            { "@aid", App == null ? AppId: App.AppId },
                            { "@clrId", ItemColor },
                            { "@t", Title },
                            { "@cc", CopyCount },
                            { "@it", ItemPlainText },
                            { "@irtf", ItemRichText },
                            { "@ihtml", ItemHtml },
                            { "@ii", ItemDbImageId},
                            { "@ciid", CopyItemId},
                        },CopyItemGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpCopyItem(fk_ParentCopyItemId,CompositeSortOrderIdx,ItemRtf,ItemHtml,MpCopyItemGuid,PasteCount,ItemDescription, fk_MpUrlId, ItemCsv,fk_MpCopyItemTypeId,fk_MpAppId,HexColor,Title,CopyDateTime,CopyCount,ItemText,fk_MpDbImageId) " +
                    "values (@cpciid,@csoi,@irtf,@ihtml,@cig,@pc,@id,@uid,@icsv,@citd,@aid,@clrId,@t,@cdt,@cc,@it,@ii)",
                    new Dictionary<string, object> {
                            { "@cpciid", CompositeParentCopyItemId },
                            { "@csoi", CompositeSortOrderIdx },
                            { "@cig",CopyItemGuid.ToString() },
                            { "@pc", PasteCount },
                            { "@id", ItemDescription },
                            { "@uid", ItemUrl == null ? 0:ItemUrl.UrlId },
                            { "@icsv",ItemCsv },
                            { "@irtf", ItemRichText },
                            { "@ihtml", ItemHtml },
                            { "@citd", (int)CopyItemType },
                            { "@aid", App == null ? AppId:App.AppId },
                            { "@clrId", ItemColor },
                            { "@t", Title },
                            { "@cdt", CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", ItemDbImageId},
                            { "@ciid", CopyItemId},
                        }, CopyItemGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                CopyItemId = MpDb.Instance.GetLastRowId("MpCopyItem", "pk_MpCopyItemId");  
            }

            foreach (var cci in CompositeItemList) {
                //for composite type items
                cci.CompositeParentCopyItemId = CopyItemId;
                cci.WriteToDatabase();
            }
            foreach (var imgObj in ImageItemObjectList) {
                imgObj.CopyItemId = CopyItemId;
                imgObj.WriteToDatabase();
            }
            foreach(var cit in TemplateList) {
                cit.CopyItemId = CopyItemId;
                cit.WriteToDatabase();
            }

            new MpCopyItemTag() {
                CopyItemId = this.CopyItemId,
                TagId = 1 //all tag
            }.WriteToDatabase();

            new MpCopyItemTag() {
                CopyItemId = this.CopyItemId,
                TagId = 2 //recent tag
            }.WriteToDatabase();

            var cil = GetAllCopyItems(out int count).Where(x => x.CopyItemId == CopyItemId).ToList();
            if (cil.Count > 0) {
                _AllCopyItemList[_AllCopyItemList.IndexOf(cil[0])] = this;
            } else {
                _AllCopyItemList.Add(this);
            }

            sw.Stop();
            Console.WriteLine("CopyItem(Id:" + CopyItemId + ") WriteToDatabase time: " + sw.ElapsedMilliseconds + "ms");
        }

        public object Clone() {
            var newItem =  new MpCopyItem(
                CopyItemType,
                Title,
                _itemData,
                MpHelpers.Instance.ConvertHexToColor(ItemColor),
                IntPtr.Zero,
                App);
            newItem.CopyCount = CopyCount;
            newItem.CopyDateTime = CopyDateTime;
            newItem.CopyItemId = 0;
            newItem.WriteToDatabase();

            foreach(var tag in MpTag.GetAllTags()) {
                if(tag.IsLinkedWithCopyItem(this)) {
                    tag.LinkWithCopyItem(newItem);
                }
            }
            foreach(var cci in CompositeItemList) {
                newItem = MpCopyItem.Merge((MpCopyItem)cci.Clone(), newItem);
            }
            return newItem;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var nci = new MpCopyItem() {
                CopyItemGuid = System.Guid.Parse(objParts[0]),
                Title = objParts[1],
                CopyCount = Convert.ToInt32(objParts[2]),
                CopyDateTime = DateTime.Parse(objParts[3]),
                ItemPlainText = objParts[4],
                ItemRichText = objParts[5],
                ItemHtml = objParts[6],
                ItemDescription = objParts[7],
                ItemCsv = objParts[8],
                CopyItemType = (MpCopyItemType)Convert.ToInt32(objParts[9])
            };
            nci.App = MpDb.Instance.GetDbObjectByTableGuid("MpApp", objParts[10]) as MpApp;
            if (string.IsNullOrEmpty(nci.ItemRichText)) {
                nci.ItemHtml = MpRtfToHtmlConverter.Instance.ConvertRtfToHtml(nci.ItemRichText);
            }
            //TODO deserialize this once img and files added
            //nci.CopyItemType = MpCopyItemType.RichText;
            return nci;
        }

        public string SerializeDbObject() {
            if (string.IsNullOrEmpty(ItemHtml)) {
                ItemHtml = MpRtfToHtmlConverter.Instance.ConvertRtfToHtml(ItemRichText);
            }
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}",
                ParseToken,
                CopyItemGuid.ToString(),
                Title,
                CopyCount,
                CopyDateTime.ToString(),
                ItemPlainText,
                ItemRichText,
                ItemHtml,
                ItemDescription,
                ItemCsv, 
                ((int)CopyItemType).ToString(),
                App.AppGuid.ToString()
                );
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItem);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpCopyItem other = null;
            if (drOrModel is DataRow) {
                other = new MpCopyItem(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpCopyItem();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(
                CopyItemGuid, 
                other.CopyItemGuid,
                "MpCopyItemGuid",
                diffLookup);
            diffLookup = CheckValue(
                Title, 
                other.Title,
                "Title",
                diffLookup);
            diffLookup = CheckValue(
                CopyCount, 
                other.CopyCount,
                "CopyCount",
                diffLookup);
            diffLookup = CheckValue(
                CopyDateTime,
                other.CopyDateTime,
                "CopyDateTime",
                diffLookup);
            diffLookup = CheckValue(
                ItemPlainText,
                other.ItemPlainText,
                "ItemText",
                diffLookup);
            diffLookup = CheckValue(
                ItemRichText,
                other.ItemRichText,
                "ItemRtf",
                diffLookup);
            diffLookup = CheckValue(
                ItemHtml,
                other.ItemHtml,
                "ItemHtml",
                diffLookup);
            diffLookup = CheckValue(
                ItemDescription,
                other.ItemDescription,
                "ItemDescription",
                diffLookup);
            diffLookup = CheckValue(
                ItemCsv,
                other.ItemCsv,
                "ItemCsv",
                diffLookup);
            diffLookup = CheckValue(
                AppId,
                other.AppId,
                "fk_MpAppId",
                diffLookup,
                App.AppGuid.ToString());
            diffLookup = CheckValue(
                CopyItemType,
                other.CopyItemType,
                "fk_MpCopyItemTypeId",
                diffLookup,
                ((int)CopyItemType).ToString());
            return diffLookup;
        }

        public async Task<object> CreateFromLogs(string dboGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var cidr = MpDb.Instance.GetDbDataRowByTableGuid("MpCopyItem", CopyItemGuid.ToString());
            MpCopyItem newCopyItem = null;
            if (cidr == null) {
                newCopyItem = new MpCopyItem();
            } else {
                newCopyItem = new MpCopyItem(cidr);
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpCopyItemGuid":
                        newCopyItem.CopyItemGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "Title":
                        newCopyItem.Title = li.AffectedColumnValue;
                        break;
                    case "CopyCount":
                        newCopyItem.CopyCount = Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    case "CopyDateTime":
                        newCopyItem.CopyDateTime = DateTime.Parse(li.AffectedColumnValue);
                        break;
                    case "ItemText":
                        newCopyItem.ItemPlainText = li.AffectedColumnValue;
                        break;
                    case "ItemRtf":
                        newCopyItem.ItemRichText = li.AffectedColumnValue;
                        break;
                    case "ItemHtml":
                        newCopyItem.ItemHtml = li.AffectedColumnValue;
                        break;
                    case "ItemDescription":
                        newCopyItem.ItemDescription = li.AffectedColumnValue;
                        break;
                    case "ItemCsv":
                        newCopyItem.ItemCsv = li.AffectedColumnValue;
                        break;
                    case "fk_MpAppId":
                        newCopyItem.App = MpApp.GetAppByGuid(li.AffectedColumnValue);
                        newCopyItem.AppId = Convert.ToInt32(newCopyItem.App.AppId);
                        break;
                    case "fk_MpCopyItemTypeId":
                        newCopyItem.CopyItemType = (MpCopyItemType)Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }

            if (string.IsNullOrEmpty(newCopyItem.ItemRichText)) {
                if (!string.IsNullOrEmpty(newCopyItem.ItemHtml)) {
                    newCopyItem.ItemRichText = MpHtmlToRtfConverter.Instance.ConvertHtmlToRtf(newCopyItem.ItemHtml);
                } else if (!string.IsNullOrEmpty(newCopyItem.ItemPlainText)) {
                    newCopyItem.ItemRichText = newCopyItem.ItemPlainText.ToRichText();
                    newCopyItem.ItemHtml = newCopyItem.ItemPlainText;
                }
            } else if (string.IsNullOrEmpty(newCopyItem.ItemHtml)) {
                newCopyItem.ItemHtml = MpRtfToHtmlConverter.Instance.ConvertRtfToHtml(newCopyItem.ItemRichText);
            }
            newCopyItem.SetData(newCopyItem.ItemRichText);
            newCopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(MpHelpers.Instance.GetRandomColor());
            //newTag.WriteToDatabase(fromClientGuid);

            if(newCopyItem.CopyItemType == MpCopyItemType.None) {
                newCopyItem.CopyItemType = MpCopyItemType.RichText;
            }
            return newCopyItem;
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
