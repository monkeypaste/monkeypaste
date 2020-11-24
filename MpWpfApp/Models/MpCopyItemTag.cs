using System;
using System.Data;

namespace MpWpfApp {
    public class MpCopyItemTag : MpDbObject {
        public int CopyItemTagId { get; set; }
        public int CopyItemId { get; set; }
        public int TagId { get; set; }

        public MpCopyItemTag(int copyItemId, int tagId) {
            CopyItemId = copyItemId;
            TagId = tagId;
        }
        public MpCopyItemTag(int copyItemTagId) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@citid", copyItemTagId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpCopyItemTag(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            CopyItemTagId = Convert.ToInt32(dr["pk_MpCopyItemTagId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            TagId = Convert.ToInt32(dr["fk_MpTagId"].ToString());
        }

        public override void WriteToDatabase() {
            if (CopyItemId == 0 || TagId == 0) {
                Console.WriteLine("MpCopyItemTag Error, cannot create tag without item and tag id's");
                return;
            }
            //new 
            if (CopyItemTagId == 0) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpCopyItemTag where fk_MpCopyItemId=@ciid and fk_MpTagId=@tid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId },
                        { "@tid", TagId }
                    });
                //if copy/tag already exists ignore duplicate
                if (dt != null && dt.Rows.Count > 0) {
                    Console.WriteLine("Ignoring duplicate tag relationship for copyItemId:" + CopyItemId + " tagId:" + TagId);
                } else {
                    MpDb.Instance.ExecuteWrite(
                        "insert into MpCopyItemTag(fk_MpCopyItemId,fk_MpTagId) values(@ciid,@tid)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@ciid", CopyItemId },
                            { "@tid", TagId }
                        });
                    CopyItemTagId = MpDb.Instance.GetLastRowId("MpCopyItemTag", "pk_MpCopyItemTagId");
                }
            } else {
                Console.WriteLine("MpCopyItemTag warning, attempting to update a tag but not implemented");
            }
        }
        public bool IsLinkedWithCopyItem(MpCopyItem ci) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpCopyItemTag where fk_MpTagId=@tid and fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
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
                //Console.WriteLine("MpCopyItemTag Warning attempting to relink tag " + CopyItemTagId + " with copyitem " + ci.copyItemId+" ignoring...");
                return;
            }
            MpDb.Instance.ExecuteWrite(
                "insert into MpCopyItemTag(fk_MpCopyItemId,fk_MpTagId) values(@ciid,@tid)",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@ciid", ci.CopyItemId },
                    { "@tid", TagId }
                });

            Console.WriteLine("Tag link created between tag " + CopyItemTagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void UnlinkWithCopyItem(MpCopyItem ci) {
            if (!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpCopyItemTag Warning attempting to unlink non-linked tag " + CopyItemTagId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where fk_MpCopyItemId=@cid and fk_MpTagId=@tid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@cid", CopyItemId },
                    { "@tid", TagId }
                });

            Console.WriteLine("Tag link removed between tag " + CopyItemTagId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where pk_MpCopyItemTagId=@citid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@citid", CopyItemTagId }
                });
        }
        private void MapDataToColumns() {
            TableName = "MpCopyItemTag";
            columnData.Clear();
            columnData.Add("pk_MpCopyItemTagId", this.CopyItemTagId);
            columnData.Add("fk_MpCopyItemId", this.CopyItemId);
            columnData.Add("fk_MpTagId", this.TagId);
        }
    }
}
