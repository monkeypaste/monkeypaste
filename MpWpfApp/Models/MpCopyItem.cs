using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SQLite;
using SQLiteNetExtensions;

namespace MpWpfApp {
    [Table("MpCopyItem")]
    public class MpCopyItem : MpDbModelBase, ICloneable, MpISyncableDbObject {
        #region Private Variables
        #endregion

        #region Properties


        public int CopyItemId { get; set; } = 0;
        public Guid CopyItemGuid { get; set; }

        public int SourceId { get; set; }

        public MpSource Source { get; set; }

        public string ItemColor { get; set; }

        public string Title { get; set; } = string.Empty;

        public MpCopyItemType ItemType { get; private set; } = MpCopyItemType.None;

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

        public string ItemDescription { get; set; } = string.Empty;

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
                

        public string ItemData { get; private set; } = string.Empty;

        public MpEventEnabledFlowDocument ItemFlowDocument { 
            get {
                if(ItemData == null) {
                    return new MpEventEnabledFlowDocument();
                }
                return ItemData.ToFlowDocument();
            }
        }

        public string ItemPlainText {
            get {
                return ItemData.ToPlainText();
            }
        }

        public string ItemRichText {
            get {
                return ItemData.ToRichText();
            }
        }

        public BitmapSource ItemBitmapSource {
            get {
                return ItemData.ToBitmapSource();
            }
        }

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
                if(ItemType != MpCopyItemType.Image) {
                    return false;
                }
                return !string.IsNullOrEmpty(ItemDescription);
            }
        }

        public int CompositeParentCopyItemId { get; set; } = -1;
        public int CompositeSortOrderIdx { get; set; } = -1;

        public bool IsCompositeParent {
            get {
                return GetCompositeChildren(this).Count > 0;
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

        public static MpCopyItem Create(MpSource source, string data, MpCopyItemType itemType) {
            var newCopyItem = new MpCopyItem() {
                CopyDateTime = DateTime.Now,
                Title = "Untitled",
                ItemData = data,
                ItemType = itemType,
                ItemColor = MpHelpers.Instance.GetRandomColor().ToString(),
                SourceId = source.SourceId,
                Source = source,
                CopyCount = 1
            };

            MpDb.Instance.AddOrUpdate<MpCopyItem>(newCopyItem);

            return newCopyItem;
        }
        public static int GetTotalItemCount() {
            var dt = MpDb.Instance.Execute(
                   "select pk_MpCopyItemId from MpCopyItem where fk_MpCopyItemTypeId is not 4", null);
            if (dt != null) {
                return dt.Rows.Count;
            }
            return 0;
        }
        
        public static List<MpCopyItem> GetCompositeChildren(MpCopyItem ci) {
            if(ci == null || ci.CopyItemId == 0) {
                return new List<MpCopyItem>();
            }
            return MpDb.Instance.GetItems<MpCopyItem>().Where(x => x.CompositeParentCopyItemId == ci.CopyItemId).OrderBy(x=>x.CompositeSortOrderIdx).ToList();
        }

        public static List<MpCopyItem> GetAllCopyItems() {
            var allItems = new List<MpCopyItem>();
            var dt = MpDb.Instance.Execute("select * from MpCopyItem order by CopyDateTime DESC", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    var ci = new MpCopyItem(dr);
                    allItems.Add(ci);
                }
            }
            return allItems;
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
            if (fromItem.ItemType == MpCopyItemType.Image || toItem.ItemType == MpCopyItemType.Image) {
                // for now, do not allow combining with image types
                return null;
            }

            if(fromItem == toItem) {
                return toItem;
            }

            toItem.LinkCompositeChild(fromItem, forceIdx);
            
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

        public MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }
        #endregion

        public void UnlinkCompositeChild(MpCopyItem cci) {
            cci.CompositeParentCopyItemId = 0;
            cci.WriteToDatabase();
        }

        public void LinkCompositeChild(MpCopyItem cci, int forceIdx = -1) {
            cci.CompositeParentCopyItemId = CopyItemId;
            var compList = MpCopyItem.GetCompositeChildren(this);
            cci.CompositeSortOrderIdx = forceIdx < 0 ? compList.Count : forceIdx < compList.Count ? forceIdx : compList.Count;
            cci.WriteToDatabase();
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
                    if (ItemType == MpCopyItemType.Image) {
                        info = "("+(int)ItemBitmapSource.Width + "px) x (" + (int)ItemBitmapSource.Height + "px)";
                    } else if (ItemType == MpCopyItemType.RichText) {
                        info = CharCount + " chars | " + LineCount + " lines";
                    } else if (ItemType == MpCopyItemType.FileList) {
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
            var ccil = GetCompositeChildren(this);
            if (ccil.Count == 0) {
                return ItemFlowDocument;
            }
            int maxCols = int.MinValue;
            foreach (var cci in ccil) {
                maxCols = Math.Max(maxCols, MpHelpers.Instance.GetColCount(cci.ItemPlainText));
            }
            string separatorLine = string.Empty;
            for (int i = 0; i < maxCols; i++) {
                separatorLine += separatorChar;
            }
            var separatorDocument = separatorLine.ToRichText().ToFlowDocument();
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            for (int i = 0; i < ccil.Count; i++) {
                var cci = ccil[i];
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
            var ccil = GetCompositeChildren(this);
            var itemRichText = MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty);
            foreach(var cci in ccil) {
                itemRichText = MpHelpers.Instance.CombineRichText(cci.ItemRichText, itemRichText, false);
            }
            return itemRichText;
        }

        public List<string> GetFileList(string baseDir = "",MpCopyItemType forceType = MpCopyItemType.None) {
            //returns path of tmp file for rt or img and actual paths of filelist
            var fileList = new List<string>();
            if (ItemType == MpCopyItemType.FileList) {
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
                switch (ItemType) {
                    case MpCopyItemType.RichText:
                        if(forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, ItemBitmapSource));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, ItemRichText));
                        }
                        foreach(var cci in GetCompositeChildren(this)) {
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
            CompositeParentCopyItemId = 0;
            CompositeSortOrderIdx = 0;
            WriteToDatabase();
        }


        private void UpdateDetails() {
            switch(ItemType) {
                case MpCopyItemType.Image:
                    if(ItemBitmapSource != null) {
                        ItemSize = new Size(ItemBitmapSource.Width, ItemBitmapSource.Height);
                    }
                    break;
                case MpCopyItemType.FileList:
                    FileCount = GetFileList().Count;
                    DataSizeInMb = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.RichText:
                    LineCount = MpHelpers.Instance.GetRowCount(ItemPlainText);
                    CharCount = ItemPlainText.Length;
                    ItemSize = ItemFlowDocument.GetDocumentSize();
                    break;
            }
        }
        
        private List<int> GetCompsiteCopyItemIdBySortOrderList() {
            if(ItemType != MpCopyItemType.Composite) {
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
            ItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            SourceId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            ItemColor = dr["HexColor"].ToString();
            CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            Title = dr["Title"].ToString();
            CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());
            ItemDescription = (dr["ItemDescription"] ?? string.Empty).ToString();
            ItemRichText = (dr["ItemRtf"] ?? string.Empty).ToString();
            ItemHtml = (dr["ItemHtml"] ?? string.Empty).ToString();
            PasteCount = Convert.ToInt32(dr["PasteCount"].ToString());

            ItemDbImageId = Convert.ToInt32(dr["fk_MpDbImageId"].ToString());
            ItemDbImage = new MpDbImage(ItemDbImageId);

            DbImageScreenshotId = Convert.ToInt32(dr["fk_SsMpDbImageId"].ToString());
            DbImageScreenshot = new MpDbImage(DbImageScreenshotId);

            ItemCsv = (dr["ItemCsv"] ?? string.Empty).ToString();

            App = MpApp.GetAppById(AppId);

            if (ItemType == MpCopyItemType.Image) {
                ItemPlainText = (dr["ItemText"] ?? string.Empty).ToString();
                //SetData(MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
                //ItemCsv = ItemPlainText;
            } else if(ItemType == MpCopyItemType.Composite) {
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

            CompositeParentCopyItemId = Convert.ToInt32((dr["fk_ParentCopyItemId"] ?? 0).ToString());
            if (CompositeParentCopyItemId > 0) {
                CompositeSortOrderIdx = Convert.ToInt32((dr["CompositeSortOrderIdx"] ?? 0).ToString());
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


            if (ItemType == MpCopyItemType.Composite) {
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
                AppId = App.AppId;
            }
            Title = string.IsNullOrEmpty(Title) ? string.Empty : Title;

            string itemText = (ItemType == MpCopyItemType.RichText || ItemType == MpCopyItemType.Composite) ? ItemRichText : ItemPlainText;
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
                            { "@icsv",ItemCsv },
                            { "@citd", (int)ItemType },
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
                            { "@citd", (int)ItemType },
                            { "@aid", App == null ? AppId:App.AppId },
                            { "@clrId", ItemColor },
                            { "@t", Title },
                            { "@cdt", CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", ItemDbImageId}
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
                ItemType,
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
                ItemType = (MpCopyItemType)Convert.ToInt32(objParts[9])
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
                ((int)ItemType).ToString(),
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
                ItemType,
                other.ItemType,
                "fk_MpCopyItemTypeId",
                diffLookup,
                ((int)ItemType).ToString());
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
                        newCopyItem.ItemType = (MpCopyItemType)Convert.ToInt32(li.AffectedColumnValue);
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

            if(newCopyItem.ItemType == MpCopyItemType.None) {
                newCopyItem.ItemType = MpCopyItemType.RichText;
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
        //Composite,
        Csv //this is only used during runtime
    }
}
