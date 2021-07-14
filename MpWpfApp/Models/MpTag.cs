using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTag : MpDbObject, MpISyncableDbObject {
        public int TagId { get; set; }
        public Guid TagGuid { get; set; }

        public int TagSortIdx { get; set; }
        public int ColorId { get; set; }

        public string TagName { get; set; }
        public MpColor TagColor { 
            get; 
            set; 
        }
        //unused
        public int ParentTagId { get; set; } = 0;
        public MpTag() { }
        public MpTag(string tagName, Color tagColor, int tagSortIdx) {
            TagGuid = Guid.NewGuid();
            TagName = tagName;
            TagColor = new MpColor((int)tagColor.R, (int)tagColor.G, (int)tagColor.B, 255);
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
                TagGuid = Guid.NewGuid();
            } else {
                TagGuid = Guid.Parse(dr["MpTagGuid"].ToString());
            }
            
           
            TagSortIdx = Convert.ToInt32(dr["SortIdx"].ToString());
            TagName = dr["TagName"].ToString();
            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            TagColor = new MpColor(ColorId);
        }

        private bool IsAltered() {
            var dt = MpDb.Instance.Execute(
                @"SELECT pk_MpTagId FROM MpTag WHERE MpTagGuid=@tg AND fk_ParentTagId=@ptid AND TagName=@tn AND SortIdx=@si AND fk_MpColorId=@cid",
                new Dictionary<string, object> {
                    { "@tg", TagGuid.ToString() },
                    { "@ptid", ParentTagId },
                    { "@tn", TagName },
                    { "@si", TagSortIdx },
                    { "@cid", ColorId }
                });
            return dt.Rows.Count == 0;
        }

        private string AlteredColumnNames() {
            var dt = MpDb.Instance.Execute(
                @"SELECT * FROM MpTag WHERE pk_MpTagId=@tid",
                new Dictionary<string, object> { { "@tid", TagId } });
            if (dt == null || dt.Rows.Count == 0) {
                return null;
            }
            var tag = new MpTag(dt.Rows[0]);
            return null;
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
        public bool LinkWithCopyItem(MpCopyItem ci) {
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
            var citg = Guid.NewGuid();
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemTag(MpCopyItemTagGuid,fk_MpCopyItemId,fk_MpTagId) values(@citg,@ciid,@tid)",
                new Dictionary<string, object> {
                    { "@citg", citg.ToString() },
                    { "@ciid", ci.CopyItemId },
                    { "@tid", TagId }
                },citg.ToString());
            var cistog = Guid.NewGuid();
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemSortTypeOrder(MpCopyItemSortTypeOrderGuid,fk_MpCopyItemId,fk_MpSortTypeId,SortOrder) values(@cistog,@ciid,@stid,@so)",
                new Dictionary<string, object> {
                    { "@cistog", cistog.ToString() },
                    { "@ciid", ci.CopyItemId },
                    { "@stid", TagId },
                    { "@so", SortOrderIdx }
                },cistog.ToString());
                //+ ci.CopyItemId + "," + this.TagId + "," + SortOrderIdx + ")");
            //WriteToDatabase();
            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
            return true;
        }
        public void UnlinkWithCopyItem(MpCopyItem ci) {
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
                    var citg = Guid.Parse(dr["MpCopyItemTagGuid"].ToString());
                    MpDb.Instance.ExecuteWrite(
                    "delete from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", Convert.ToInt32(dr["pk_MpCopyItemTagId"].ToString()) }
                        }, citg.ToString());
                }
            }
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }

        public override void WriteToDatabase(string sourceClientGuid, bool isFirstLoad = false) {
            if (TagGuid == Guid.Empty) {
                TagGuid = Guid.NewGuid();
            }
            //if (!IsAltered()) {
            //    return;
            //}
            if (string.IsNullOrEmpty(TagName)) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            //if new tag
            if (TagId == 0) {
                if (TagColor == null) {
                    //occurs with initial tag creation on first load
                    TagColor = MpColor.GetColorById(ColorId);
                }
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "insert into MpTag(MpTagGuid,TagName,fk_MpColorId,SortIdx) values(@tg,@tn,@cid,@si)",
                    new Dictionary<string, object> {
                        { "@tg", TagGuid.ToString() },
                        { "@tn", TagName },
                        { "@cid", ColorId },
                        { "@si", TagSortIdx }
                    }, TagGuid.ToString(),sourceClientGuid,this,isFirstLoad);
                TagId = MpDb.Instance.GetLastRowId("MpTag", "pk_MpTagId");
            } else {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "update MpTag set MpTagGuid=@tg, TagName=@tn, fk_MpColorId=@cid, SortIdx=@si where pk_MpTagId=@tid",
                    new Dictionary<string, object> {
                        { "@tg", TagGuid.ToString() },
                        { "@tn", TagName },
                        { "@cid", ColorId },
                        { "@tid", TagId },
                        { "@si", TagSortIdx }
                    }, TagGuid.ToString(),sourceClientGuid,this,isFirstLoad);
            }
        }
        public override void WriteToDatabase() {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
        }
        public void WriteToDatabase(bool isFirstLoad) {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid,isFirstLoad);
        }
        public override void DeleteFromDatabase(string sourceClientGuid) {
            MpDb.Instance.ExecuteWrite(
                "delete from MpTag where pk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                }, TagGuid.ToString());

            var dt = MpDb.Instance.Execute(@"select * from MpCopyItemTag where where fk_MpTagId=@tid", new Dictionary<string, object> { { "@tid", TagId } });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    var citg = Guid.Parse(dr["MpCopyItemTagGuid"].ToString());
                    MpDb.Instance.ExecuteWrite(
                    "delete from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", Convert.ToInt32(dr["pk_MpCopyItemTagId"].ToString()) }
                        }, citg.ToString());
                }
            }
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
        }
        public void DeleteFromDatabase() {
            DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
        }

        public MpTag CreateFromLogs(string tagGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var tdr = MpDb.Instance.GetDbObjectByTableGuid("MpTag", tagGuid);
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
                    case "SortIdx":
                        newTag.TagSortIdx = Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    case "fk_MpColorId":
                        var cdr = MpDb.Instance.GetDbObjectByTableGuid("MpColor", li.AffectedColumnValue);
                        newTag.ColorId = new MpColor(cdr).ColorId;
                        break;
                    default:
                        throw new Exception(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                }
            }
            newTag.WriteToDatabase(fromClientGuid);
            return newTag;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpTag() {
                TagGuid = System.Guid.Parse(objParts[0]),
                TagName = objParts[1],
                TagSortIdx = Convert.ToInt32(objParts[2]),
                ColorId = Convert.ToInt32(objParts[3])
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}",
                ParseToken,
                TagGuid.ToString(),
                TagName,
                TagSortIdx,
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
            //if(TagId > 0) {
            //    diffLookup = CheckValue(TagId, other.TagId,
            //        "pk_MpTagId",
            //        diffLookup);
            //}
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
            diffLookup = CheckValue(
                TagSortIdx, other.TagSortIdx,
                "SortIdx",
                diffLookup);
            diffLookup = CheckValue(
                ColorId, other.ColorId,
                "fk_MpColorId",
                diffLookup,
                TagColor.ColorGuid
                );

            return diffLookup;
        }
    }
}
