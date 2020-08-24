using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTag : MpDbObject {
        public int TagId { get; set; }
        public int ColorId { get; set; }
        public string TagName { get; set; }
        public MpColor TagColor { get; set; }
        //unused
        public int ParentTagId { get; set; }

        public MpTag(string tagName, Color tagColor) {
            TagName = tagName;
            TagColor = new MpColor((int)tagColor.R, (int)tagColor.G, (int)tagColor.B, 255);
        }
        public MpTag(int tagId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpTag where pk_MpTagId=" + tagId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpTag(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpTag> GetAllTags() {
            List<MpTag> tags = new List<MpTag>();
            DataTable dt = MpDb.Instance.Execute("select * from MpTag");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow r in dt.Rows) {
                    tags.Add(new MpTag(r));
                }
            }
            return tags;
        }
        public override void LoadDataRow(DataRow dr) {
            TagId = Convert.ToInt32(dr["pk_MpTagId"].ToString());
            TagName = dr["TagName"].ToString();
            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            TagColor = new MpColor(ColorId);
        }

        public override void WriteToDatabase() {
            if (string.IsNullOrEmpty(TagName) || MpDb.Instance.NoDb) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            //if new tag
            if (TagId == 0) {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDb.Instance.ExecuteNonQuery("insert into MpTag(TagName,fk_MpColorId) values('" + TagName + "'," + ColorId + ")");
                TagId = MpDb.Instance.GetLastRowId("MpTag", "pk_MpTagId");
            } else {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDb.Instance.ExecuteNonQuery("update MpTag set TagName='" + TagName + "', fk_MpColorId=" + ColorId + " where pk_MpTagId=" + TagId);
            }
        }
        public bool IsLinkedWithCopyItem(MpCopyItem ci) {
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItemTag where fk_MpTagId=" + TagId + " and fk_MpCopyItemId=" + ci.CopyItemId);
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
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItemTag where fk_MpTagId=" + this.TagId);
            int SortOrderIdx = dt.Rows.Count + 1;
            MpDb.Instance.ExecuteNonQuery("insert into MpCopyItemTag(fk_MpCopyItemId,fk_MpTagId) values(" + ci.CopyItemId + "," + TagId + ")");
            MpDb.Instance.ExecuteNonQuery("insert into MpCopyItemSortTypeOrder(fk_MpCopyItemId,fk_MpSortTypeId,SortOrder) values(" + ci.CopyItemId + "," + this.TagId + "," + SortOrderIdx + ")");
            WriteToDatabase();
            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
        }
        public void UnlinkWithCopyItem(MpCopyItem ci) {
            if (!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemTag where fk_MpCopyItemId=" + ci.CopyItemId + " and fk_MpTagId=" + TagId);
            //MpDb.Instance.ExecuteNonQuery("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
            Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpTag where pk_MpTagId=" + this.TagId);
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemTag where fk_MpTagId=" + this.TagId);
            //MpDb.Instance.ExecuteNonQuery("delete from MpTagCopyItemSortOrder where fk_MpTagId=" + this.TagId);
        }
        private void MapDataToColumns() {
            TableName = "MpTag";
            columnData.Clear();
            columnData.Add("pk_MpTagId", this.TagId);
            columnData.Add("fk_MpColorId", this.ColorId);
            columnData.Add("TagName", this.TagName);
        }
    }
}
