using MonkeyPaste;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    [Table("MpTag")]
    public class MpTag : MpDbModelBase, MpISyncableDbObject {

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTagId")]
        public int TagId { get; set; }

        [Column("MpTagGuid")]
        public string Guid { get; set; }

        [Ignore]
        public Guid TagGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [Column("SortIdx")]
        public int TagSortIdx { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }

        public string TagName { get; set; }

        [Ignore]
        public MpColor Color { get; set; }

        [Column("fk_ParentTagId")]
        [ForeignKey(typeof(MpTag))]
        public int ParentTagId { get; set; } = 0;

        public MpTag() { }
        public MpTag(string tagName, Color color, int tagSortIdx) {
            TagGuid = System.Guid.NewGuid();
            TagName = tagName;
            Color = new MpColor((int)color.R, (int)color.G, (int)color.B, 255);
            TagSortIdx = tagSortIdx;
        }
        public MpTag(int tagId) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpTag where pk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", tagId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpTag(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpTag> GetAllTags() {
            List<MpTag> tags = new List<MpTag>();
            DataTable dt = MpDb.Instance.Execute("select * from MpTag", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow r in dt.Rows) {
                    tags.Add(new MpTag(r));
                }
            }
            return tags;
        }
        public override void LoadDataRow(DataRow dr) {
            TagId = Convert.ToInt32(dr["pk_MpTagId"].ToString());
            if (dr["MpTagGuid"] == null || dr["MpTagGuid"].GetType() == typeof(System.DBNull)) {
                TagGuid = System.Guid.NewGuid();
            } else {
                TagGuid = System.Guid.Parse(dr["MpTagGuid"].ToString());
            }            
           
            TagSortIdx = Convert.ToInt32(dr["SortIdx"].ToString());
            TagName = dr["TagName"].ToString();
            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            Color = new MpColor(ColorId);
        }
                
        public bool IsLinkedWithCopyItem(MpCopyItem ci) {
            if(ci == null) {
                return false;
            }
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpCopyItemTag where fk_MpTagId=@tid and fk_MpCopyItemId=@ciid",
                new Dictionary<string, object> {
                    { "@tid", TagId },
                    { "@ciid", ci.CopyItemId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                return true;
            }
            return false;
        }
        public bool LinkWithCopyItem(MpCopyItem ci, string sourceClientGuid = "") {
            if(ci == null) {
                return false;
            }
            //returns FALSE if copyitem is already linked to maintain correct counts
            if (IsLinkedWithCopyItem(ci)) {               
                return false;
            }
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpCopyItemTag where fk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                });
            int SortOrderIdx = dt.Rows.Count + 1;
            var citg = System.Guid.NewGuid();
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemTag(MpCopyItemTagGuid,fk_MpCopyItemId,fk_MpTagId) values(@citg,@ciid,@tid)",
                new Dictionary<string, object> {
                    { "@citg", citg.ToString() },
                    { "@ciid", ci.CopyItemId },
                    { "@tid", TagId }
                },citg.ToString());
            var cistog = System.Guid.NewGuid();
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemSortTypeOrder(MpCopyItemSortTypeOrderGuid,fk_MpCopyItemId,fk_MpSortTypeId,SortOrder) values(@cistog,@ciid,@stid,@so)",
                new Dictionary<string, object> {
                    { "@cistog", cistog.ToString() },
                    { "@ciid", ci.CopyItemId },
                    { "@stid", TagId },
                    { "@so", SortOrderIdx }
                },cistog.ToString());
            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
            return true;
        }
        public void UnlinkWithCopyItem(MpCopyItem ci, string sourceClientGuid = "") {
            if(ci == null) {
                return;
            }
            if (!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            var dt = MpDb.Instance.Execute(@"select * from MpCopyItemTag where where fk_MpCopyItemId=@ciid and fk_MpTagId=@tid", new Dictionary<string, object> { { "@ciid", ci.CopyItemId },
                    { "@tid", TagId } });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    var citg = System.Guid.Parse(dr["MpCopyItemTagGuid"].ToString());
                    MpDb.Instance.ExecuteWrite(
                    "delete from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", Convert.ToInt32(dr["pk_MpCopyItemTagId"].ToString()) }
                        }, citg.ToString());
                }
            }
            Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false,bool ignoreSyncing = false) {
            if(string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisClientGuid;
            }
            if (TagGuid == System.Guid.Empty) {
                TagGuid = System.Guid.NewGuid();
            }

            if (string.IsNullOrEmpty(TagName)) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            if (Color == null) {
                //occurs with initial tag creation on first load
                Color = MpColor.GetColorById(ColorId);
                if (Color == null) {
                    MpConsole.WriteTraceLine(@"Tag create error, Color should be defined already but so creating random one");
                    //seems to be an intermittent problem maybe caused by SyncStart SyncEnd calls for color
                    Color = new MpColor(MpHelpers.Instance.GetRandomColor());
                }
            }
            if (TagId == 0) {
                
                Color.WriteToDatabase(sourceClientGuid,ignoreTracking,ignoreSyncing);
                ColorId = Color.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "insert into MpTag(MpTagGuid,TagName,fk_MpColorId,SortIdx) values(@tg,@tn,@cid,@si)",
                    new Dictionary<string, object> {
                        { "@tg", TagGuid.ToString() },
                        { "@tn", TagName },
                        { "@cid", ColorId },
                        { "@si", TagSortIdx }
                    }, TagGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);
                TagId = MpDb.Instance.GetLastRowId("MpTag", "pk_MpTagId");
            } else {
                Color.WriteToDatabase(sourceClientGuid,ignoreTracking,ignoreSyncing);
                ColorId = Color.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "update MpTag set MpTagGuid=@tg, TagName=@tn, fk_MpColorId=@cid, SortIdx=@si where pk_MpTagId=@tid",
                    new Dictionary<string, object> {
                        { "@tg", TagGuid.ToString() },
                        { "@tn", TagName },
                        { "@cid", ColorId },
                        { "@tid", TagId },
                        { "@si", TagSortIdx }
                    }, TagGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);                
            }
        }
        public override void WriteToDatabase() {
            if(IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
            }            
        }
        public void WriteToDatabase(bool ignoreTracking,bool ignoreSyncing) {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid,ignoreTracking,ignoreSyncing);
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisClientGuid;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpTag where pk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                }, TagGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);

            var dt = MpDb.Instance.Execute(@"select * from MpCopyItemTag where fk_MpTagId=@tid", new Dictionary<string, object> { { "@tid", TagId } });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    var citg = System.Guid.Parse(dr["MpCopyItemTagGuid"].ToString());
                    MpDb.Instance.ExecuteWrite(
                    "delete from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", Convert.ToInt32(dr["pk_MpCopyItemTagId"].ToString()) }
                        }, citg.ToString(),sourceClientGuid,ignoreTracking,ignoreSyncing);
                }
            }
            if(Color != null) {
                Color.DeleteFromDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);
            }
        }
        
        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }

        public async Task<object> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var tdr = MpDb.Instance.GetDbDataRowByTableGuid("MpTag", tagGuid);
            MpTag newTag = null;
            if (tdr == null) {
                newTag = new MpTag();
            } else {
                newTag = new MpTag(tdr);
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpTagGuid":
                        newTag.TagGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "TagName":
                        newTag.TagName = li.AffectedColumnValue;
                        break;
                    //case "SortIdx":
                    //    newTag.TagSortIdx = Convert.ToInt32(li.AffectedColumnValue);
                    //    break;
                    case "fk_MpColorId":
                        var cdr = MpDb.Instance.GetDbDataRowByTableGuid("MpColor", li.AffectedColumnValue);
                        newTag.ColorId = new MpColor(cdr).ColorId;
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            //newTag.WriteToDatabase(fromClientGuid);
            return newTag;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpTag() {
                TagGuid = System.Guid.Parse(objParts[0]),
                TagName = objParts[1],
                //TagSortIdx = Convert.ToInt32(objParts[2]),
                ColorId = Convert.ToInt32(objParts[2])
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}",
                ParseToken,
                TagGuid.ToString(),
                TagName,
                //TagSortIdx,
                ColorId);
        }

        public Type GetDbObjectType() {
            return typeof(MpTag);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpTag other = null;
            if(drOrModel is DataRow) {
                other = new MpTag(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpTag();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(TagGuid, other.TagGuid,
                "MpTagGuid",
                diffLookup);
            diffLookup = CheckValue(ParentTagId, other.ParentTagId,
                "fk_ParentTagId",
                diffLookup);
            diffLookup = CheckValue(
                TagName, other.TagName,
                "TagName",
                diffLookup);
            //diffLookup = CheckValue(
            //    TagSortIdx, other.TagSortIdx,
            //    "SortIdx",
            //    diffLookup);
            diffLookup = CheckValue(
                ColorId, other.ColorId,
                "fk_MpColorId",
                diffLookup,
                Color.ColorGuid
                );

            return diffLookup;
        }
    }
}
