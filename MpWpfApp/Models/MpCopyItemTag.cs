using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;
using MonkeyPaste;
using Azure;

namespace MpWpfApp {
    [Table("MpCopyItemTag")]
    public class MpCopyItemTag : MpDbModelBase, MpISyncableDbObject {
        private static List<MpCopyItemTag> _AllCopyItemTagList = null;
        public static int TotalCopyItemTagCount = 0;

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemTagId")]
        public int CopyItemTagId { get; set; }

        [Column("MpCopyItemTagGuid")]
        public  string Guid { get; set; }

        [Ignore]
        public Guid CopyItemTagGuid {
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

        [ForeignKey(typeof(MpTag))]
        [Column("fk_MpTagId")]
        public int TagId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        public static List<MpCopyItemTag> GetAllCopyItemsForTagId(int tagId) {
            if (_AllCopyItemTagList == null) {
                GetAllCopyItemTags();
            }
            return _AllCopyItemTagList.Where(x => x.TagId == tagId).ToList();
        }

        public static void DeleteAllCopyItemTagsForCopyItemId(int CopyItemId) {            
            foreach (var cit in GetAllCopyItemTags()) {
                if(cit.CopyItemId == CopyItemId) {
                    cit.DeleteFromDatabase();
                }
            }
        }

        public static void DeleteAllCopyItemTagsForTagId(int tagId) {
            foreach (var cit in GetAllCopyItemTags()) {
                if (cit.TagId == tagId) {
                    cit.DeleteFromDatabase();
                }
            }
        }

        public static List<MpCopyItemTag> GetAllCopyItemTags() {
            if (_AllCopyItemTagList == null) {
                _AllCopyItemTagList = new List<MpCopyItemTag>();
                DataTable dt = MpDb.Instance.Execute("select * from MpCopyItemTag", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllCopyItemTagList.Add(new MpCopyItemTag(dr));
                    }
                }
            }
            return _AllCopyItemTagList;
        }
        public static MpCopyItemTag GetCopyItemTagById(int tagId,int copyItemId) {
            if (_AllCopyItemTagList == null) {
                GetAllCopyItemTags();
            }
            var udbpl = _AllCopyItemTagList.Where(x => x.CopyItemTagId == copyItemId && x.TagId == tagId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static List<MpCopyItemTag> GetCopyItemTagsByTagId(int tagId) {
            if (_AllCopyItemTagList == null) {
                GetAllCopyItemTags();
            }
            return _AllCopyItemTagList.Where(x => x.TagId == tagId).ToList();
        }

        public static MpCopyItemTag GetCopyItemTagByGuid(string colorGuid) {
            if (_AllCopyItemTagList == null) {
                GetAllCopyItemTags();
            }
            var udbpl = _AllCopyItemTagList.Where(x => x.CopyItemTagGuid.ToString() == colorGuid).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public MpCopyItemTag(DataRow dr) : this() {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            CopyItemTagId = Convert.ToInt32(dr["pk_MpCopyItemTagId"].ToString());
            if (dr["MpCopyItemTagGuid"] == null || dr["MpCopyItemTagGuid"].GetType() == typeof(System.DBNull)) {
                CopyItemTagGuid = System.Guid.NewGuid();
            } else {
                CopyItemTagGuid = System.Guid.Parse(dr["MpCopyItemTagGuid"].ToString());
            }
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            TagId = Convert.ToInt32(dr["fk_MpTagId"].ToString());
        }

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisClientGuid;
            }
            if (CopyItemTagGuid == System.Guid.Empty) {
                CopyItemTagGuid = System.Guid.NewGuid();
            }

            if (CopyItemTagId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpCopyItemTag(MpCopyItemTagGuid,fk_MpCopyItemId,fk_MpTagId) values(@citg,@ciid,@tid)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@citg",CopyItemTagGuid.ToString() },
                        { "@ciid", CopyItemId },
                        { "@tid", TagId }
                    }, CopyItemTagGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                CopyItemTagId = MpDb.Instance.GetLastRowId("MpCopyItemTag", "pk_MpCopyItemTagId");
                GetAllCopyItemTags().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpCopyItemTag set MpCopyItemTagGuid=@citg, fk_MpCopyItemId=@ciid,fk_MpTagId=@tid where pk_MpCopyItemTagId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid",CopyItemTagId },
                        { "@citg",CopyItemTagGuid.ToString() },
                        { "@ciid", CopyItemId },
                        { "@tid", TagId }
                    }, CopyItemTagGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                var c = GetAllCopyItemTags().Where(x => x.CopyItemTagId == CopyItemTagId).FirstOrDefault();
                if (c != null) {
                    _AllCopyItemTagList[_AllCopyItemTagList.IndexOf(c)] = this;
                }
            }
        }
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
        public void WriteToDatabase(bool isFirstLoad) {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid, isFirstLoad);
        }
        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (CopyItemTagId <= 0) {
                return;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                new Dictionary<string, object> {
                    { "@citid", CopyItemTagId }
                }, CopyItemTagGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
        }

        public async Task<object> CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var citdr = MpDb.Instance.GetDbDataRowByTableGuid("MpCopyItemTag", tagGuid);
            MpCopyItemTag newCopyItemTag = null;
            if (citdr == null) {
                newCopyItemTag = new MpCopyItemTag();
            } else {
                newCopyItemTag = new MpCopyItemTag(citdr);
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpCopyItemTagGuid":
                        newCopyItemTag.CopyItemTagGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpCopyItemId":
                        var cidr = MpDb.Instance.GetDbDataRowByTableGuid("MpCopyItem", li.AffectedColumnValue);
                        newCopyItemTag.CopyItemId = new MpCopyItem(cidr).CopyItemId;
                        break;
                    case "fk_MpTagId":
                        var tdr = MpDb.Instance.GetDbDataRowByTableGuid("MpTag", li.AffectedColumnValue);
                        newCopyItemTag.TagId = new MpTag(tdr).TagId;
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            //newCopyItemTag.WriteToDatabase(fromClientGuid);
            return newCopyItemTag;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpCopyItemTag() {
                CopyItemTagGuid = System.Guid.Parse(objParts[0]),
                CopyItemId = Convert.ToInt32(objParts[1]),
                TagId = Convert.ToInt32(objParts[2]),
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            var cig = MpCopyItem.GetCopyItemById(CopyItemId)?.CopyItemGuid;
            var tg = MpTag.GetTagById(TagId)?.TagGuid;

            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}",
                ParseToken,
                CopyItemTagGuid.ToString(),
                cig,
                tg);
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItemTag);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            var cig = MpCopyItem.GetCopyItemById(CopyItemId)?.CopyItemGuid;
            var tg = MpTag.GetTagById(TagId)?.TagGuid;
            MpCopyItemTag other = null;
            if (drOrModel is DataRow) {
                other = new MpCopyItemTag(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpCopyItemTag();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(CopyItemTagGuid, other.CopyItemTagGuid,
                "MpCopyItemTagGuid",
                diffLookup);
            diffLookup = CheckValue(CopyItemId, other.CopyItemId,
                "fk_CopyItemId",
                diffLookup,
                cig);
            diffLookup = CheckValue(TagId, other.TagId,
                "fk_TagId",
                diffLookup,
                tg);

            return diffLookup;
        }

        #endregion

        public MpCopyItemTag() {
        }
    }
}
