using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpFileDropListItem : MpCopyItem {
        public int FileDropListItemId { get; set; }
        public List<MpFileDropListSubItem> FileDropListSubItemList { get; set; } = new List<MpFileDropListSubItem>();

        public MpFileDropListItem(int fileDropListItemId, int copyItemId, string[] pathArray) {
            FileDropListItemId = fileDropListItemId;
            CopyItemId = copyItemId;
            foreach(var path in pathArray) {
                FileDropListSubItemList.Add(new MpFileDropListSubItem(0,FileDropListItemId,path));
            }
        }
        public MpFileDropListItem(int fileDropListItemId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpFileDropListItem where pk_MpFileDropListItemId=" + fileDropListItemId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpFileDropListItem(DataRow dr)  {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            FileDropListItemId = Convert.ToInt32(dr["pk_MpFileDropListItemId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());

            FileDropListSubItemList.Clear();
            var dt = MpDb.Instance.Execute("select * from MpFileDropListSubItem where fk_MpFileDropListItemId=" + FileDropListItemId);
            foreach(DataRow r in dt.Rows) {
                FileDropListSubItemList.Add(new MpFileDropListSubItem(r));
            }

            dt = MpDb.Instance.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
            if(dt != null && dt.Rows.Count > 0) {
                base.LoadDataRow(dt.Rows[0]);
            }
        }

        public override void WriteToDatabase() {
            if (FileDropListSubItemList.Count == 0 || MpDb.Instance.NoDb) {
                Console.WriteLine("MpFileDropListItem Error, cannot create empty file drop list item");
                return;
            }
            //if new file drop list
            if (FileDropListItemId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpFileDropListItem(fk_MpCopyItemId) values(" + CopyItemId + ")");
                FileDropListItemId = MpDb.Instance.GetLastRowId("MpFileDropListItem", "pk_MpFileDropListItemId");
                foreach(var subItem in FileDropListSubItemList) {
                    subItem.FileDropListItemId = FileDropListItemId;
                    subItem.WriteToDatabase();
                }
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpFileDropListItem set fk_MpCopyItemId=" + CopyItemId + " where pk_MpFileDropListItemId=" + FileDropListItemId);
                foreach (var subItem in FileDropListSubItemList) {
                    subItem.WriteToDatabase();
                }
            }
        }
        public override void DeleteFromDatabase() {
            base.DeleteFromDatabase();
            MpDb.Instance.ExecuteNonQuery("delete from MpFileDropListSubItem where fk_MpFileDropListItemId=" + FileDropListItemId);
            MpDb.Instance.ExecuteNonQuery("delete from MpFileDropListItem where pk_MpFileDropListItemId=" + FileDropListItemId);
            
        }
        public string[] GetFileArray() {
            var fl = new List<string>();
            foreach(var si in FileDropListSubItemList) {
                fl.Add(si.ItemPath);
            }
            return fl.ToArray();
        }
        private void MapDataToColumns() {
            TableName = "MpFileDropListItem";
            columnData.Clear();
            columnData.Add("pk_MpFileDropListItemId", FileDropListItemId);
            columnData.Add("fk_MpCopyItemId", CopyItemId);
        }
        public bool ContainsPath(string path) {
            foreach(var subItem in FileDropListSubItemList) {
                if(subItem.ItemPath == path) {
                    return true;
                }
            }
            return false;
        }
        public override MpCopyItem GetExistingCopyItem() {
            var dt = MpDb.Instance.Execute("select * from MpFileDropListItem");
            if(dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    int fdliId = Convert.ToInt32(dr["pk_MpFileDropListItemId"].ToString());
                    var subDt = MpDb.Instance.Execute("select * from MpFileDropListSubItem where fk_MpFileDropListItemId=" + fdliId);
                    if(subDt != null && subDt.Rows.Count > 0) {
                        bool isDuplicate = false;
                        foreach (DataRow subDr in subDt.Rows) {
                            string path = subDr["ItemPath"].ToString();
                            if(ContainsPath(path)) {
                                isDuplicate = true;
                                break;
                            }
                        }
                        if(isDuplicate) {
                            return new MpFileDropListItem(dr);
                        }
                    }
                }
            }
            return null;
        }

        public override string GetPlainText() {
            string outStr = string.Empty;
            foreach (var subItem in FileDropListSubItemList) {
                outStr += subItem.ItemPath + Environment.NewLine;
            }
            return outStr;
        }
    }
}
