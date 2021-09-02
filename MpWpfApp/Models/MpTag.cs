using MonkeyPaste;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using MonkeyPaste;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Linq;

namespace MpWpfApp {
    [Table("MpTag")]
    public class MpTag : MpDbModelBase, MpISyncableDbObject {
        private static List<MpTag> _AllTagList = null;

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

        [Column("HexColor")]
        public string HexColor { get; set; }

        public string TagName { get; set; }


        [Column("fk_ParentTagId")]
        [ForeignKey(typeof(MpTag))]
        public int ParentTagId { get; set; } = 0;

        public MpTag() {
        }

        public MpTag(string tagName, Color color, int tagSortIdx) : this() {
            TagGuid = System.Guid.NewGuid();
            TagName = tagName;
            HexColor = MpHelpers.Instance.ConvertColorToHex(color);
            TagSortIdx = tagSortIdx;
        }
        public MpTag(int tagId) : this() {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpTag where pk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", tagId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpTag(DataRow dr) : this() {
            LoadDataRow(dr);
        }
        public static List<MpTag> GetAllTags() {
            if (_AllTagList == null) {
                _AllTagList = new List<MpTag>();
                DataTable dt = MpDb.Instance.Execute("select * from MpTag", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllTagList.Add(new MpTag(dr));
                    }
                }
            }
            return _AllTagList;
        }

        public static MpTag GetTagById(int tagId) {
            return GetAllTags().Where(x => x.TagId == tagId).FirstOrDefault();
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
            HexColor = dr["HexColor"].ToString();
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
        public bool LinkWithCopyItem(MpCopyItem ci, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (ci == null) {
                return false;
            }
            //returns FALSE if copyitem is already linked to maintain correct counts
            if (IsLinkedWithCopyItem(ci)) {               
                return false;
            }

            var cit = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.NewGuid(),
                CopyItemId = ci.CopyItemId,
                TagId = TagId
            };

            cit.WriteToDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);

            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
            return true;
        }
        public void UnlinkWithCopyItem(MpCopyItem ci, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (ci == null) {
                return;
            }
            if (!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }

            var cit = MpCopyItemTag.GetCopyItemTagById(TagId, ci.CopyItemId);
            if(cit != null) {
                cit.DeleteFromDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);
                Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
            }
        }

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false,bool ignoreSyncing = false) {
            if(string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisDeviceGuid;
            }
            if (TagGuid == System.Guid.Empty) {
                TagGuid = System.Guid.NewGuid();
            }

            if (string.IsNullOrEmpty(TagName)) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            if (TagId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpTag(MpTagGuid,TagName,HexColor,SortIdx) values(@tg,@tn,@cid,@si)",
                    new Dictionary<string, object> {
                        { "@tg", TagGuid.ToString() },
                        { "@tn", TagName },
                        { "@cid", HexColor },
                        { "@si", TagSortIdx }
                    }, TagGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);
                TagId = MpDb.Instance.GetLastRowId("MpTag", "pk_MpTagId");
                GetAllTags().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpTag set MpTagGuid=@tg, TagName=@tn, HexColor=@cid, SortIdx=@si where pk_MpTagId=@tid",
                    new Dictionary<string, object> {
                        { "@tg", TagGuid.ToString() },
                        { "@tn", TagName },
                        { "@cid", HexColor },
                        { "@tid", TagId },
                        { "@si", TagSortIdx }
                    }, TagGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);
                var t = GetAllTags().Where(x => x.TagId == TagId).FirstOrDefault();
                if (t != null) {
                    _AllTagList[_AllTagList.IndexOf(t)] = this;
                }
            }
        }
        public override void WriteToDatabase() {
            if(IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }            
        }
        public void WriteToDatabase(bool ignoreTracking,bool ignoreSyncing) {
            WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid,ignoreTracking,ignoreSyncing);
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisDeviceGuid;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpTag where pk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                }, TagGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);

            var citl = MpCopyItemTag.GetCopyItemTagsByTagId(TagId);
            foreach(var cit in citl) {
                cit.DeleteFromDatabase(sourceClientGuid, ignoreTracking, ignoreSyncing);
            }

            GetAllTags().Remove(this);
        }
        
        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
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
                    case "HexColor":
                        newTag.HexColor = li.AffectedColumnValue;
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
                HexColor = objParts[2]
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
                HexColor);
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
                HexColor, other.HexColor,
                "HexColor",
                diffLookup);

            return diffLookup;
        }
    }
}
