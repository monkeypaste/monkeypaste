using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public enum MpTagType {
        None=1,
        App,
        Device,
        Custom,
        Default
    }

    public class MpTag:MpDbObject {
        public MpTagType TagType { get; set; }
        public int TagId { get; set; }
        public int ColorId { get; set; }
        public string TagName { get; set; }
        public MpColor TagColor { get; set; }

        public MpTag(string tagName,Color tagColor,MpTagType tagType = MpTagType.Custom) {
            TagName = tagName;
            TagType = tagType;
            TagColor = new MpColor((int)tagColor.R,(int)tagColor.G,(int)tagColor.B,(int)tagColor.A);
            ColorId = TagColor.ColorId;
        }
        public MpTag(int tagId) {
            DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpTag where pk_MpTagId=" + tagId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpTag(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            TagId = Convert.ToInt32(dr["pk_MpTagId"].ToString());
            TagType = (MpTagType)Convert.ToInt32(dr["fk_MpTagTypeId"].ToString());
            TagName = dr["TagName"].ToString();
            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            TagColor = new MpColor(ColorId);
        }

        public override void WriteToDatabase() {
            if(TagName == null || TagName == string.Empty || MpApplication.Instance.DataModel.Db.NoDb) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            if(TagId == 0) {
                DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpTag where TagName='" + TagName + "'");
                //if tag already exists just populate this w/ its data
                if(dt != null && dt.Rows.Count > 0) {
                    TagId = Convert.ToInt32(dt.Rows[0]["pk_MpTagId"].ToString());
                    if(dt.Rows[0]["fk_MpColorId"] != null) {
                        ColorId = Convert.ToInt32(dt.Rows[0]["fk_MpColorId"].ToString());
                        TagColor = new MpColor(ColorId);
                    } else {
                        ColorId = TagColor.ColorId;
                    }
                } else {
                    MpApplication.Instance.DataModel.Db.ExecuteNonQuery("insert into MpTag(fk_MpTagTypeId,TagName,fk_MpColorId) values(" + (int)TagType + ",'" + TagName + "'," + ColorId + ")");
                    TagId = MpApplication.Instance.DataModel.Db.GetLastRowId("MpTag","pk_MpTagId");
                }
            } else {
                Console.WriteLine("MpTag warning, attempting to update a tag but not implemented");
            }
        }
        public bool IsLinkedWithCopyItem(MpCopyItem ci) {
            DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpCopyItemTag where fk_MpTagId=" + TagId + " and fk_MpCopyItemId=" + ci.CopyItemId);
            if(dt != null && dt.Rows.Count > 0) {
                return true;
            }
            return false;
        }
        public void LinkWithCopyItem(MpCopyItem ci) {
            if(IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to relink tag " + TagId + " with copyitem " + ci.copyItemId+" ignoring...");
                return;
            }
            MpApplication.Instance.DataModel.Db.ExecuteNonQuery("insert into MpCopyItemTag(fk_MpCopyItemId,fk_MpTagId) values(" + ci.CopyItemId + "," + TagId + ")");

            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
        }
        public void UnlinkWithCopyItem(MpCopyItem ci) {
            if(!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            MpApplication.Instance.DataModel.Db.ExecuteNonQuery("delete from MpCopyItemTag where fk_MpCopyItemId="+ci.CopyItemId+" and fk_MpTagId="+TagId);

            Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void DeleteFromDatabase() {
            MpApplication.Instance.DataModel.Db.ExecuteNonQuery("delete from MpTag where pk_MpTagId=" + this.TagId);
        }
        private void MapDataToColumns() {
            TableName = "MpTag";
            columnData.Clear();
            columnData.Add("pk_MpTagId",this.TagId);
            columnData.Add("fk_MpTagTypeId",this.TagType);
            columnData.Add("fk_MpColorId",this.ColorId);
            columnData.Add("TagName",this.TagName);
        }
    }
}
