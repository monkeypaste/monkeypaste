using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTag : MpDbObject {
        public int TagId { get; set; }
        public int TagSortIdx { get; set; }
        public int ColorId { get; set; }
        public string TagName { get; set; }
        public MpColor TagColor { get; set; }
        //unused
        public int ParentTagId { get; set; }

        public MpTag(string tagName, Color tagColor, int tagCount) {
            TagName = tagName;
            TagColor = new MpColor((int)tagColor.R, (int)tagColor.G, (int)tagColor.B, 255);
            TagSortIdx = tagCount;
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
            TagSortIdx = Convert.ToInt32(dr["SortIdx"].ToString());
            TagName = dr["TagName"].ToString();
            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            TagColor = new MpColor(ColorId);
        }

        public override void WriteToDatabase() {
            if (string.IsNullOrEmpty(TagName)) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            //if new tag
            if (TagId == 0) {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "insert into MpTag(TagName,fk_MpColorId,SortIdx) values(@tn,@cid,@si)",
                    new Dictionary<string, object> {
                        { "@tn", TagName },
                        { "@cid", ColorId },
                        { "@si", TagSortIdx }
                    });
                TagId = MpDb.Instance.GetLastRowId("MpTag", "pk_MpTagId");
            } else {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "update MpTag set TagName=@tn, fk_MpColorId=@cid, SortIdx=@si where pk_MpTagId=@tid",
                    new Dictionary<string, object> {
                        { "@tn", TagName },
                        { "@cid", ColorId },
                        { "@tid", TagId },
                        { "@si", TagSortIdx }
                    });
            }
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
        public void LinkWithCopyItem(MpCopyItem ci) {
            if (IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to relink tag " + TagId + " with copyitem " + ci.copyItemId+" ignoring...");
                return;
            }
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpCopyItemTag where fk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                });
            int SortOrderIdx = dt.Rows.Count + 1;
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemTag(fk_MpCopyItemId,fk_MpTagId) values(@ciid,@tid)",
                new Dictionary<string, object> {
                    { "@ciid", ci.CopyItemId },
                    { "@tid", TagId }
                });
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemSortTypeOrder(fk_MpCopyItemId,fk_MpSortTypeId,SortOrder) values(@ciid,@stid,@so)",
                new Dictionary<string, object> {
                    { "@ciid", ci.CopyItemId },
                    { "@stid", TagId },
                    { "@so", SortOrderIdx }
                });
                //+ ci.CopyItemId + "," + this.TagId + "," + SortOrderIdx + ")");
            WriteToDatabase();
            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
        }
        public void UnlinkWithCopyItem(MpCopyItem ci) {
            if (!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where fk_MpCopyItemId=@ciid and fk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@ciid", ci.CopyItemId },
                    { "@tid", TagId }
                });
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void DeleteFromDatabase() {            
            MpDb.Instance.ExecuteWrite(
                "delete from MpTag where pk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where fk_MpTagId=@tid",
                new Dictionary<string, object> {
                    { "@tid", TagId }
                });
            //MpDb.Instance.ExecuteWrite("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
        }
    }
}
