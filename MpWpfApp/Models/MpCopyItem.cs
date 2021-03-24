using System;
using System.Collections.Generic;
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
//using Windows.Storage;

namespace MpWpfApp {
    public class MpCopyItem : MpDbObject, ICloneable {
        #region Private Variables
        //private static int _CopyItemCount = 0;
        private object _itemData = null;

        private static List<MpApp> _AppList = null;
        private static List<MpColor> _ColorList = null;

        #endregion

        #region Properties

        public int CopyItemId { get; set; } = 0;

        public int PreCopyItemId { get; set; } = 0;

        public int PostCopyItemId { get; set; } = 0;

        public MpApp App { get; set; }

        public MpClient Client { get; set; }

        public MpColor ItemColor { get; set; }

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

        public long DataSizeInMb { get; set; } = 0;

        public int RelevanceScore {
            get {
                return CopyCount + PasteCount;
            }
        }
                
        public string ItemPlainText { get; set; }

        public string ItemRichText { get; set; }

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

        private BitmapSource _itemBitmapSource = null;
        public BitmapSource ItemBitmapSource {
            get {
                switch (CopyItemType) {
                    //case MpCopyItemType.FileList:
                    //    return MpHelpers.Instance.ConvertRichTextToBitmapSource(MpHelpers.Instance.ConvertPlainTextToRichText((string)_itemData));
                    case MpCopyItemType.Image:
                        return (BitmapSource)_itemData;
                    case MpCopyItemType.Composite:
                    case MpCopyItemType.RichText:
                        return _itemBitmapSource;
                }
                return new BitmapImage();
            }
            set {
                _itemBitmapSource = value;
            }
        }

        //public BitmapSource ItemTitleSwirl { get; set; }

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
        #endregion

        #region Factory Methods
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
                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) {
                    return null;
                }
                Color itemColor = MpHelpers.Instance.GetRandomColor();
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

                var newItem = new MpCopyItem();
                newItem.CopyItemType = itemType;
                newItem.CopyDateTime = DateTime.Now;
                newItem.Title = newItem.CopyItemType == MpCopyItemType.RichText || newItem.CopyItemType == MpCopyItemType.Csv ? "Text" : Enum.GetName(typeof(MpCopyItemType), newItem.CopyItemType);
                newItem.CopyCount = 1;
                newItem.Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);

                var appPath = MpHelpers.Instance.GetProcessPath(processHandle);
                var app = _AppList.Where(x => x.AppPath == appPath).ToList();
                if (app == null || app.Count == 0) {
                    newItem.App = new MpApp(false, processHandle);
                    _AppList.Add(newItem.App);
                } else {
                    newItem.App = app[0];
                }

                var color = _ColorList.Where(x => x.Color == itemColor).ToList();
                if (color == null || color.Count == 0) {
                    newItem.ItemColor = new MpColor(itemColor);
                    _ColorList.Add(newItem.ItemColor);
                } else {
                    newItem.ItemColor = color[0];
                }
                var sw = new Stopwatch();
                sw.Start();
                //newItem.ItemTitleSwirl = newItem.InitSwirl(null, false);
                sw.Stop();
                Console.WriteLine("Time to create swirl: " + sw.ElapsedMilliseconds + " ms");
                newItem.ImageItemObjectList = new List<MpDetectedImageObject>();
                switch (itemType) {
                    case MpCopyItemType.Csv:
                        newItem.ItemCsv = (string)itemData;
                        newItem.CopyItemType = MpCopyItemType.RichText;
                        newItem.SetData(MpHelpers.Instance.ConvertPlainTextToRichText(newItem.ItemCsv));
                        break;
                    case MpCopyItemType.FileList:
                        string paths = string.Empty;
                        foreach (string str in (string[])itemData) {
                            paths += str + Environment.NewLine;
                        }
                        newItem.SetData(paths);
                        break;
                    case MpCopyItemType.Image:
                        newItem.SetData((BitmapSource)itemData);
                        break;
                    case MpCopyItemType.RichText:
                        newItem.SetData((string)itemData);
                        break;
                }
                return newItem;
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

        public static List<MpCopyItem> GetAllCopyItems(out int count) {
            _AppList = MpApp.GetAllApps();
            _ColorList = MpColor.GetAllColors();
            count = 0;
            var clips = new List<MpCopyItem>();
            var dt = MpDb.Instance.Execute("select * from MpCopyItem order by CopyDateTime DESC", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    var ci = new MpCopyItem(dr);
                    if(!ci.IsSubCompositeItem) {
                        count++;
                    }
                    clips.Add(ci);
                }
            }
            return clips;
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
            } else {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpCopyItem where ItemImage=@ii",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ii", MpHelpers.Instance.ConvertBitmapSourceToByteArray((BitmapSource)data) }
                        });
                if (dt != null && dt.Rows.Count > 0) {
                    return new MpCopyItem(dt.Rows[0]);
                }
            }
            return null;
        }

        public static MpCopyItem Merge(
            MpCopyItem fromItem,
            MpCopyItem toItem,
            bool isInline = false,
            bool createComposite = false,
            bool useFileData = false,
            bool isFileDataMerged = false,
            int forceIdx = -1) {
            /*cases:
                (from)->(to) = (Output Types)
                RichText->RichText = RichText | Composite
                RichText->FileList = FileList(with RichText as file w / title at eol)
                RichText->Composite = Composite

                FileList->FileList = FileList
                FileList->RichText = RichText(of file path's or data) | Composite (with File Path's(or file data if supported and flagged) as rich text sub-composites(or single if flagged))
                FileList->Composite = Composite(each file path or data as sub - composite or merged based on flags)


                Composite->Composite = Composite
                Composite->FileList = FileList(each sub - composite is file path stored in a userdata folder or is ONE file in userdata folder if merge is flagged)
                Composite->RichText = RichText
            */
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
                            if(createComposite) {
                                
                            } else {
                                string fromText = string.Empty;
                                foreach (string fromPath in fromItem.GetFileList()) {
                                    fromText += useFileData ? MpHelpers.Instance.ReadTextFromFile(fromPath) + Environment.NewLine : fromPath + Environment.NewLine;
                                }
                                var toRichText = MpHelpers.Instance.CombineRichText(fromText.ToRichText(), toItem.ItemRichText);
                                toItem.SetData(toRichText);
                            }
                            break;
                        case MpCopyItemType.Composite:

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
                    if (!createComposite) {
                        toItem.SetData(MpHelpers.Instance.CombineRichText(
                                fromItem.ItemRichText,
                                toItem.ItemRichText,
                                !isInline));
                        //always remove tag associations from other item if added its in the ctvm
                        foreach (var tag in MpTag.GetAllTags()) {
                            if (tag.IsLinkedWithCopyItem(fromItem)) {
                                tag.LinkWithCopyItem(toItem);
                            }
                        }
                        fromItem.DeleteFromDatabase();
                        toItem.WriteToDatabase();
                        return toItem;
                    }
                    if(toItem.CopyItemType == MpCopyItemType.Composite) {
                        compositeItem = toItem;
                    } else {
                        compositeItem = new MpCopyItem(MpCopyItemType.Composite, toItem.Title, null, toItem.ItemColor.Color, IntPtr.Zero, toItem.App);
                        compositeItem.WriteToDatabase();
                        toItem.CompositeParentCopyItemId = compositeItem.CopyItemId;
                        toItem.CompositeSortOrderIdx = 0;
                        compositeItem.CompositeItemList.Add(toItem);
                    }
                    
                    fromItem.CompositeParentCopyItemId = compositeItem.CopyItemId;
                    fromItem.CompositeSortOrderIdx = compositeItem.CompositeItemList.Count;
                    compositeItem.CompositeItemList.Add(fromItem);
                    if(forceIdx >= 0) {
                        compositeItem.CompositeItemList.Move(fromItem.CompositeSortOrderIdx, forceIdx);
                        foreach(var cci in compositeItem.CompositeItemList) {
                            cci.CompositeSortOrderIdx = compositeItem.CompositeItemList.IndexOf(cci);
                        }
                    }
                    //always remove tag associations from other item if added its in the ctvm
                    //foreach (var tag in MpTag.GetAllTags()) {
                    //    if (tag.IsLinkedWithCopyItem(fromItem)) {
                    //        tag.LinkWithCopyItem(compositeItem);
                    //    }
                    //    if (tag.IsLinkedWithCopyItem(toItem)) {
                    //        tag.LinkWithCopyItem(compositeItem);
                    //    }
                    //}
                    compositeItem.SetData(null);
                    compositeItem.WriteToDatabase();
                    //toItem.WriteToDatabase();
                    //fromItem.WriteToDatabase();
                    return compositeItem;
                case MpCopyItemType.Composite:
                    switch(toItem.CopyItemType) {
                        case MpCopyItemType.RichText:
                            compositeItem = new MpCopyItem(MpCopyItemType.Composite, toItem.Title, null, toItem.ItemColor.Color, IntPtr.Zero, toItem.App);
                            compositeItem.WriteToDatabase();
                            toItem.CompositeParentCopyItemId = compositeItem.CopyItemId;
                            toItem.CompositeSortOrderIdx = 0;
                            compositeItem.CompositeItemList.Add(toItem);
                            foreach(var sci in fromItem.CompositeItemList) {
                                sci.CompositeParentCopyItemId = compositeItem.CopyItemId;
                                sci.CompositeSortOrderIdx = compositeItem.CompositeItemList.Count;
                                compositeItem.CompositeItemList.Add(sci);
                            }                            
                            break;
                        case MpCopyItemType.Composite:
                            compositeItem = toItem;
                            foreach (var occi in fromItem.CompositeItemList) {
                                //take composite items from fromItem 
                                occi.CompositeParentCopyItemId = compositeItem.CopyItemId;
                                occi.CompositeSortOrderIdx = compositeItem.CompositeItemList.Count;
                                if(forceIdx < 0) {
                                    //and add them to the end of the toItem ny default
                                    compositeItem.CompositeItemList.Add(occi);
                                } else {
                                    //otherwise insert them at the forcedidx
                                    compositeItem.CompositeItemList.Insert(forceIdx, occi);
                                }
                            }
                            if (forceIdx >= 0) {
                                //if inserted and not added reloop through items and
                                //ensure their sort order matches their index
                                foreach (var cci in compositeItem.CompositeItemList) {
                                    cci.CompositeSortOrderIdx = compositeItem.CompositeItemList.IndexOf(cci);
                                }
                            }
                            break;
                    }
                    fromItem.CompositeItemList.Clear();
                    //always remove tag associations from other item if added its in the ctvm
                    //foreach (var tag in MpTag.GetAllTags()) {
                    //    if (tag.IsLinkedWithCopyItem(fromItem)) {
                    //        tag.LinkWithCopyItem(toItem);
                    //    }
                    //}
                    if(fromItem.CopyItemType == MpCopyItemType.Composite) {
                        fromItem.DeleteFromDatabase();
                    }
                    compositeItem.SetData(null);
                    compositeItem.WriteToDatabase();
                    return compositeItem;
            }
            //if can't combine don't alter fromItem and just return toItem;
            return toItem;
        }

        public static async Task MergeAsync(MpCopyItem fromItem, MpCopyItem toItem, DispatcherPriority priority = DispatcherPriority.Background) {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => MpCopyItem.Merge(fromItem,toItem), priority);
        }
        #endregion

        #region Public Methods

        public MpCopyItem() {
            ItemColor = new MpColor(MpHelpers.Instance.GetRandomColor());
            Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            Title = "Loading";
            //ItemTitleSwirl = new BitmapImage();
            CopyDateTime = DateTime.Now;
            App = new MpApp();
            _itemData = "Default";
            CopyItemType = MpCopyItemType.RichText;
            ImageItemObjectList = new List<MpDetectedImageObject>();
        }

        //public MpCopyItem() {

        //}

        //public MpCopyItem(MpCopyItemType type, string title, object data, Color itemColor) : this(type, data, MpHelpers.Instance.GetRandomColor(), ((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow)).Handle,null) { }
        
        private MpCopyItem(
            MpCopyItemType itemType,
            string title,
            object data,
            Color tileColor,
            IntPtr hwnd,
            MpApp app) {
            CopyItemType = itemType;
            CopyDateTime = DateTime.Now;
            if(string.IsNullOrEmpty(title)) {
                Title = CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Csv ? "Text" : Enum.GetName(typeof(MpCopyItemType), CopyItemType);
            } else {
                Title = title;
            }
            CopyCount = 1;
            Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            
            if(hwnd != IntPtr.Zero) {
                var appPath = MpHelpers.Instance.GetProcessPath(hwnd);
                var appList = _AppList.Where(x => x.AppPath == appPath).ToList();
                if (appList == null || appList.Count == 0) {
                    App = new MpApp(false, hwnd);
                    _AppList.Add(App);
                } else {
                    App = appList[0];
                }
            } else if(app != null) {
                App = app;
            }

            var color = _ColorList.Where(x => x.Color == tileColor).ToList();
            if(color == null || color.Count == 0) {
                ItemColor = new MpColor(tileColor);
                _ColorList.Add(ItemColor);
            } else {
                ItemColor = color[0];
            }

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

        public void SetData(object data) {
            _itemData = data;
            UpdateItemData();
            UpdateDetails();
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

        public string GetDetail(MpCopyItemDetailType detailType) {
            string info = "I dunno";// string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc
                    //TimeSpan dur = DateTime.Now - CopyItemCreatedDateTime;
                    info = "Copied " + CopyDateTime.ToString(); //dur.ToString();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItemType == MpCopyItemType.Image) {
                        info = "(" + (int)ItemBitmapSource.Width + ") x (" + (int)ItemBitmapSource.Height + ")";
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
                if (i % 2 == 1) {
                    MpHelpers.Instance.CombineFlowDocuments(
                    separatorDocument,
                    fullDocument,
                    true);
                }
                MpHelpers.Instance.CombineFlowDocuments(
                    cci.ItemFlowDocument,
                    fullDocument,
                    true);
            }
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

        public int GetCompositeParentCopyItemId() {
            //returns -1 if item is not a composite
            //returns 0 if item is the parent composite item (so ITS pk is the ParentId for its children)
            //returns N if item is part of a composite item
            if(CopyItemId <= 0) {
                return -1;
            }
            //first check if it is a composite item
            var dt = MpDb.Instance.Execute(
                    "select * from MpCompositeCopyItem where fk_ParentMpCopyItemId=@ciid",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", CopyItemId }
                        });
            if (dt != null && dt.Rows.Count > 0) {
                return 0;
            }
            dt = MpDb.Instance.Execute(
                    "select * from MpCompositeCopyItem where fk_MpCopyItemId=@ciid",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", CopyItemId }
                        });
            if (dt != null && dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0]["fk_ParentMpCopyItemId"].ToString());
            }

            return -1;
        }

        public List<string> GetFileList(string baseDir = "",MpCopyItemType forceType = MpCopyItemType.None) {
            //returns path of tmp file for rt or img and actual paths of filelist
            var fileList = new List<string>();
            if (CopyItemType == MpCopyItemType.FileList) {
                if(forceType == MpCopyItemType.Image) {
                    fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource));
                } else if (forceType == MpCopyItemType.RichText) {
                    fileList.Add(MpHelpers.Instance.WriteTextToFile(Path.GetTempFileName(), ItemPlainText));
                } else {
                    var splitArray = ItemPlainText.Split(Environment.NewLine.ToCharArray());
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
                string op = MpHelpers.Instance.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType:forceType),Title,baseDir);
                //file extension
                switch (CopyItemType) {
                    case MpCopyItemType.RichText:
                        if(forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, ItemBitmapSource));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, ItemPlainText));
                        }
                        break;
                    case MpCopyItemType.Composite:
                        foreach(var cci in CompositeItemList) {
                            if (forceType == MpCopyItemType.Image) {
                                fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, cci.ItemBitmapSource));
                            } else {
                                fileList.Add(MpHelpers.Instance.WriteTextToFile(op, cci.ItemPlainText));
                            }
                            op = MpHelpers.Instance.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType : forceType), Title, baseDir);
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

            return fileList;
        }

        public BitmapSource InitSwirl(BitmapSource sharedSwirl = null, bool forceUseItemColor = false) {
            //return (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/" + "title_swirl0001.png"));
            if (sharedSwirl == null) {
                var path = @"pack://application:,,,/Resources/Images/";
                var swirlList = new List<BitmapSource>();
                if (forceUseItemColor) {
                    var itemBrush = new SolidColorBrush() { Color = ItemColor.Color };
                    SolidColorBrush lighterColor = MpHelpers.Instance.ChangeBrushAlpha(
                                    MpHelpers.Instance.ChangeBrushBrightness(itemBrush, -0.5f), 100);
                    SolidColorBrush darkerColor = MpHelpers.Instance.ChangeBrushAlpha(
                                    MpHelpers.Instance.ChangeBrushBrightness(itemBrush, -0.4f), 50);
                    SolidColorBrush accentColor = MpHelpers.Instance.ChangeBrushAlpha(
                                    MpHelpers.Instance.ChangeBrushBrightness(itemBrush, -0.0f), 100);

                    var swirl1 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0001.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl1, (itemBrush).Color));

                    var swirl2 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0002.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl2, lighterColor.Color));

                    var swirl3 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0003.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl3, darkerColor.Color));

                    var swirl4 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0004.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl4, accentColor.Color));
                } else {
                    var randomColorList = MpHelpers.Instance.GetRandomizedList<MpColor>(App.PrimaryIconColorList);
                    for (int i = 0; i < 4; i++) {
                        var c = randomColorList[i].Color;
                        c.A = (byte)MpHelpers.Instance.Rand.Next(40,120);
                        var swirl = (BitmapSource)new BitmapImage(new Uri(path + string.Format(@"title_swirl000{0}.png", i + 1)));
                        swirl = MpHelpers.Instance.TintBitmapSource(swirl, c);
                        swirlList.Add(swirl);
                    }
                }
                //randomize order of layers
                var rsl = MpHelpers.Instance.GetRandomizedList<BitmapSource>(swirlList);
                return MpHelpers.Instance.MergeImages(new List<BitmapSource>() { rsl[0], rsl[1], rsl[2], rsl[3] });
            } else {
                return sharedSwirl;
            }
        }

        public async Task<BitmapSource> InitSwirlAsync(BitmapSource sharedSwirl = null, bool forceUseItemColor = false, bool retainAlpha = true, DispatcherPriority priority = DispatcherPriority.Background) {
            BitmapSource swirl = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(async () => {
                swirl = InitSwirl(sharedSwirl, forceUseItemColor);
            }, priority);

            return swirl;
        }
        #endregion

        #region Private Methods
        private void UnlinkFromCompositeParent() {
            if (!IsSubCompositeItem) {
                return;
            }
            MpDb.Instance.ExecuteWrite(
                "delete from MpCompositeCopyItem where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            CompositeCopyItemId = 0;
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
                    foreach(var path in GetFileList()) {
                        csvText += path + ", ";
                    }
                    if(csvText.Length > 2) {
                        csvText = csvText.Remove(csvText.Length - 2, 2);
                    }
                    ItemCsv = csvText;
                    break;
                case MpCopyItemType.Image:
                    if (Application.Current.MainWindow.DataContext == null ||
                       ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).IsLoading) {
                        //when loading from database
                        ImageItemObjectList = MpDetectedImageObject.GetAllObjectsForItem(CopyItemId);
                    } else {
                        //var ibsba = MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemBitmapSource);

                        //var ia = MpImageAnalyzer.Instance.AnalyzeImage(ibsba).Result;

                        //var iocr = MpImageOcr.Instance.OcrImage(ibsba).Result;

                        //if (string.IsNullOrEmpty(ItemPlainText)) {
                        //    if(ImageItemObjectList.Count > 0) {
                        //        ItemPlainText = ItemCsv;
                        //    } else {
                        //        ItemPlainText = "Image";
                        //    }                            
                        //}

                        ItemPlainText = "Image";
                        ItemRichText = ItemPlainText.ToRichText();
                        ItemFlowDocument = ItemRichText.ToFlowDocument();
                    }
                    break;
                case MpCopyItemType.RichText:
                    ItemRichText = (string)_itemData;
                    ItemFlowDocument = ItemRichText.ToFlowDocument();
                    ItemPlainText = ItemRichText.ToPlainText();
                    ItemBitmapSource = ItemFlowDocument.ToBitmapSource();
                    if (!string.IsNullOrEmpty(ItemCsv) && MpHelpers.Instance.IsStringCsv(ItemPlainText)) {
                        //this is when copying from excel or DataObject type supports csv
                        ItemCsv = ItemPlainText;
                    }
                    break;
                case MpCopyItemType.Composite:
                    //_itemData is null and needs to be gathered from sub-items
                    _itemData = GetCompositeItemRichText();
                    ItemRichText = (string)_itemData;
                    ItemFlowDocument = ItemRichText.ToFlowDocument();
                    ItemPlainText = ItemRichText.ToPlainText();                     
                    ItemBitmapSource = GetSeparatedCompositeFlowDocument().ToBitmapSource();
                    break;
            }
        }
        private async Task UpdateItemDataAsync(DispatcherPriority priority = DispatcherPriority.Background) {
            await Dispatcher.CurrentDispatcher.InvokeAsync(UpdateItemData,priority);          
        }

        private void UpdateDetails() {
            switch(CopyItemType) {
                case MpCopyItemType.Image:

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
                    break;
                case MpCopyItemType.RichText:
                    LineCount = MpHelpers.Instance.GetRowCount(ItemPlainText);
                    CharCount = ItemPlainText.Length;
                    break;
            }
        }
        
        private List<int> GetCompsiteCopyItemIdBySortOrderList() {
            if(CopyItemType != MpCopyItemType.Composite) {
                return null;
            }
            var dt = MpDb.Instance.Execute(
                    "select fk_MpCopyItemId from MpCompositeCopyItem where fk_ParentMpCopyItemId=@ciid order by SortOrderIdx ASC",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", CopyItemId }
                        }); ;
            if (dt != null && dt.Rows.Count > 0) {
                var copyItemIdList = new List<int>();
                foreach(DataRow dr in dt.Rows) {
                    copyItemIdList.Add(Convert.ToInt32(dr["fk_MpCopyItemId"].ToString()));
                }
                return copyItemIdList;
            }
            return null;
        }
        #endregion

        #region Overrides

        public override void LoadDataRow(DataRow dr) {
            CopyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            CopyItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            int clientId = Convert.ToInt32(dr["fk_MpClientId"].ToString());
            int appId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            int colorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            Title = dr["Title"].ToString();
            CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());
            
            ItemCsv = dr["ItemCsv"].ToString();

            Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            if(_AppList == null) {
                _AppList = MpApp.GetAllApps();
            }
            App = _AppList.Where(x => x.AppId == appId).ToList()[0];
            if(_ColorList == null) {
                _ColorList = MpColor.GetAllColors();
            }
            ItemColor = _ColorList.Where(x => x.ColorId == colorId).ToList()[0];

            if (CopyItemType == MpCopyItemType.Image) {
                SetData(MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
                ItemPlainText = dr["ItemText"].ToString();
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

            if (dr["ItemImage"] != null && dr["ItemImage"].GetType() != typeof(System.DBNull)) {
                ItemBitmapSource = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]);
            }

            CompositeParentCopyItemId = GetCompositeParentCopyItemId();
            if (CompositeParentCopyItemId <= 0) {
                //only create title swirl for composite parent and non-composite items
                //ItemTitleSwirl = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["TitleSwirl"]);
            } else {
                //since this is a child of a composite element load all of its composite data
                var dt = MpDb.Instance.Execute(
                    "select * from MpCompositeCopyItem where fk_MpCopyItemId=@ciid",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", CopyItemId }
                        });
                if (dt != null && dt.Rows.Count > 0) {
                    DataRow cdr = dt.Rows[0];
                    CompositeCopyItemId = Convert.ToInt32(cdr["pk_MpCompositeCopyItemId"].ToString());
                    CompositeSortOrderIdx = Convert.ToInt32(cdr["SortOrderIdx"].ToString());
                    //IsInlineWithPreviousCompositeItem = Convert.ToInt32(cdr["IsInlineWithPreviousItem"].ToString()) > 0 ? true : false;
                }
            }
            PasteCount = GetPasteCount();
        }

        public void DeleteFromDatabase() {
            if (CopyItemId <= 0) {
                return;
            }
            MpDb.Instance.ExecuteWrite(
                "delete from MpPasteHistory where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItem where pk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpDetectedImageObject where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemSortTypeOrder where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpShortcut where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTemplate where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCompositeCopyItem where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });

            if (CopyItemType == MpCopyItemType.Composite) {
                MpDb.Instance.ExecuteWrite(
                "delete from MpCompositeCopyItem where fk_ParentMpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
                foreach(var cci in CompositeItemList) {
                    cci.DeleteFromDatabase();
                }
            }
            
            //var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
            //mwvm.ClipTrayViewModel.GetDataSource().CopyItemDataProvider.OnCopyItemChanged(this, new MpCopyItemChangeEventArgs(MpCopyItemChangeType.Remove));
        }

        // still req'd if NoDb=true
        public override void WriteToDatabase() {
            var sw = new Stopwatch();
            sw.Start();
            App.WriteToDatabase();
            ItemColor.WriteToDatabase();
            Title = string.IsNullOrEmpty(Title) ? string.Empty : Title;
            string itemText = (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite) ? ItemRichText : ItemPlainText;
            if(string.IsNullOrEmpty(itemText)) {
                itemText = string.Empty;
            }
            byte[] itemImage = MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemBitmapSource);
            //if copyitem already exists
            if (CopyItemId > 0) {
                MpDb.Instance.ExecuteWrite(
                        "update MpCopyItem set ItemCsv=@icsv, fk_MpCopyItemTypeId=@citd, fk_MpClientId=@cid, fk_MpAppId=@aid, fk_MpColorId=@clrId, Title=@t, CopyCount=@cc, ItemText=@it, ItemImage=@ii where pk_MpCopyItemId=@ciid",
                        new Dictionary<string, object> {
                            { @"icsv",ItemCsv },
                            //{ "@ts", MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
                            { "@citd", (int)CopyItemType },
                            { "@cid", Client.ClientId },
                            { "@aid", App.AppId },
                            { "@clrId", ItemColor.ColorId },
                            { "@t", Title },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", itemImage},
                            { "@ciid", CopyItemId},
                        });
            } else {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpCopyItem(ItemCsv,fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount,ItemText,ItemImage) " + 
                    "values (@icsv,@citd,@cid,@aid,@clrId,@t,@cdt,@cc,@it,@ii)",
                    new Dictionary<string, object> {
                            { @"icsv",ItemCsv },
                            //{ "@ts", MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
                            { "@citd", (int)CopyItemType },
                            { "@cid", Client.ClientId },
                            { "@aid", App.AppId },
                            { "@clrId", ItemColor.ColorId },
                            { "@t", Title },
                            { "@cdt", CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", itemImage},
                            { "@ciid", CopyItemId},
                        });
                CopyItemId = MpDb.Instance.GetLastRowId("MpCopyItem", "pk_MpCopyItemId");  
            }

            if(CompositeParentCopyItemId > 0) {
                //for composite children
                if(CompositeCopyItemId == 0) {
                    MpDb.Instance.ExecuteWrite(
                    "insert into MpCompositeCopyItem(fk_MpCopyItemId,fk_ParentMpCopyItemId,SortOrderIdx) " +
                    "values (@ciid,@pciid,@soidx)",
                    new Dictionary<string, object> {
                            { @"ciid",CopyItemId },
                            { "@pciid", CompositeParentCopyItemId },
                            { "@soidx", CompositeSortOrderIdx }
                        });
                    CompositeCopyItemId = MpDb.Instance.GetLastRowId("MpCompositeCopyItem", "pk_MpCompositeCopyItemId");
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "update MpCompositeCopyItem set fk_MpCopyItemId=@ciid, fk_ParentMpCopyItemId=@pciid, SortOrderIdx=@soidx where pk_MpCompositeCopyItemId=@cciid",
                        new Dictionary<string, object> {
                            { @"cciid", CompositeCopyItemId },
                            { @"ciid",CopyItemId },
                            { "@pciid", CompositeParentCopyItemId },
                            { "@soidx", CompositeSortOrderIdx }
                        });
                }
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
            
            sw.Stop();
            Console.WriteLine("CopyItem(Id:" + CopyItemId + ") WriteToDatabase time: " + sw.ElapsedMilliseconds + "ms");
        }

        public object Clone() {
            var newItem =  new MpCopyItem(
                CopyItemType,
                Title,
                _itemData,
                ItemColor.Color,
                IntPtr.Zero,
                App);
            newItem.CopyCount = CopyCount;
            newItem.CopyDateTime = CopyDateTime;
            newItem.CopyItemId = CopyItemId;
            //newItem.ItemTitleSwirl = ItemTitleSwirl;
            return newItem;
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
