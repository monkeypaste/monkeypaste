﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
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

    public class MpTag : MpDbObject {
        public MpTagType TagType { get; set; }
        public int TagId { get; set; }
        public int ColorId { get; set; }
        public string TagName { get; set; }
        public MpColor TagColor { get; set; }

        public MpTag(string tagName,Color tagColor,MpTagType tagType = MpTagType.Custom) {
            TagName = tagName;
            TagType = tagType;
            TagColor = new MpColor((int)tagColor.R,(int)tagColor.G,(int)tagColor.B,255);
        }
        public MpTag(int tagId) {
            DataTable dt = MpDataStore.Instance.Db.Execute("select * from MpTag where pk_MpTagId=" + tagId);
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
            if(TagName == null || TagName == string.Empty || MpDataStore.Instance.Db.NoDb) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            //if new tag
            if(TagId == 0) {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDataStore.Instance.Db.ExecuteNonQuery("insert into MpTag(fk_MpTagTypeId,TagName,fk_MpColorId) values(" + (int)TagType + ",'" + TagName + "'," + ColorId + ")");
                TagId = MpDataStore.Instance.Db.GetLastRowId("MpTag", "pk_MpTagId");                 
            } else {
                TagColor.WriteToDatabase();
                ColorId = TagColor.ColorId;
                MpDataStore.Instance.Db.ExecuteNonQuery("update MpTag set fk_MpTagTypeId=" + (int)TagType + ", TagName='" + TagName + "', fk_MpColorId=" + ColorId+" where pk_MpTagId="+TagId);                
            }
        }
        public bool IsLinkedWithCopyItem(MpClip ci) {
            DataTable dt = MpDataStore.Instance.Db.Execute("select * from MpCopyItemTag where fk_MpTagId=" + TagId + " and fk_MpCopyItemId=" + ci.CopyItemId);
            if(dt != null && dt.Rows.Count > 0) {
                return true;
            }
            return false;
        }
        public void LinkWithCopyItem(MpClip ci) {
            if(IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to relink tag " + TagId + " with copyitem " + ci.copyItemId+" ignoring...");
                return;
            }
            MpDataStore.Instance.Db.ExecuteNonQuery("insert into MpCopyItemTag(fk_MpCopyItemId,fk_MpTagId) values(" + ci.CopyItemId + "," + TagId + ")");

            Console.WriteLine("Tag link created between tag " + TagId + " with copyitem " + ci.CopyItemId);
        }
        public void UnlinkWithCopyItem(MpClip ci) {
            if(!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpTag Warning attempting to unlink non-linked tag " + TagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            MpDataStore.Instance.Db.ExecuteNonQuery("delete from MpCopyItemTag where fk_MpCopyItemId="+ci.CopyItemId+" and fk_MpTagId="+TagId);

            Console.WriteLine("Tag link removed between tag " + TagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void DeleteFromDatabase() {
            MpDataStore.Instance.Db.ExecuteNonQuery("delete from MpTag where pk_MpTagId=" + this.TagId);
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