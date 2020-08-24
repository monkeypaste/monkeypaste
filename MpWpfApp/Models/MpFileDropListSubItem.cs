using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileDropListSubItem : MpDbObject {
        public int FileDropListSubItemId { get; set; }
        public int FileDropListItemId { get; set; }
        public string ItemPath { get; set; }

        public MpFileDropListSubItem(int fileDropListSubItemId,int fileDropListItemId, string itemPath) {
            FileDropListSubItemId = fileDropListSubItemId;
            FileDropListItemId = fileDropListItemId;
            ItemPath = itemPath;
        }
        public MpFileDropListSubItem(int textItemId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpFileDropListSubItem where pk_MpFileDropListSubItemId=" + textItemId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpFileDropListSubItem(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            FileDropListSubItemId = Convert.ToInt32(dr["pk_MpFileDropListSubItemId"].ToString());
            FileDropListItemId = Convert.ToInt32(dr["fk_MpFileDropListItemId"].ToString());
            ItemPath = dr["ItemPath"].ToString();
        }

        public override void WriteToDatabase() {
            if(string.IsNullOrEmpty(ItemPath) || MpDb.Instance.NoDb) {
                Console.WriteLine("MpFileDropListSubItem Error, cannot create empty path item");
                return;
            }
            //if new sub item
            if(FileDropListSubItemId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpFileDropListSubItem(fk_MpFileDropListItemId,ItemPath) values(" + FileDropListItemId + ",'" + ItemPath + "')");
                FileDropListSubItemId = MpDb.Instance.GetLastRowId("MpFileDropListSubItem", "pk_MpFileDropListSubItemId");                 
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpFileDropListSubItem set ItemPath='" + ItemPath + "', fk_MpFileDropListItemId=" + FileDropListItemId+" where pk_MpFileDropListSubItemId="+FileDropListSubItemId);                
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpFileDropListSubItem where pk_MpFileDropListSubItemId=" + FileDropListSubItemId);
        }
        private void MapDataToColumns() {
            TableName = "MpFileDropListSubItem";
            columnData.Clear();
            columnData.Add("pk_MpFileDropListSubItemId",FileDropListSubItemId);
            columnData.Add("fk_MpFileDropListItemId", FileDropListItemId);
            columnData.Add("ItemPath", ItemPath);
        }
    }
}
