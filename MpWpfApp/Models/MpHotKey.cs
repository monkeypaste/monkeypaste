using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpHotKey : MpDbObject, IComparable {
        public static string Delimeter = "+";

        public int HotKeyId { get; set; }
        public int CommandId { get; set; }
        public int ItemIdx { get; set; }
        public string KeyStr { get; private set; }

        private List<Key> _keyList = new List<Key>();

        public MpHotKey() {
            HotKeyId = 0;
            ItemIdx = 0;
            CommandId = 0;
            KeyStr = string.Empty;
            _keyList = new List<Key>();
        }
        public MpHotKey(string keyList) : this() {
            KeyStr = keyList;
        }

        public MpHotKey(int hkId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpHotKey where pk_MpHotKeyId=" + hkId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpHotKey(DataRow dr) {
            LoadDataRow(dr);
        }

        

        public override void LoadDataRow(DataRow dr) {
            HotKeyId = Convert.ToInt32(dr["pk_MpHotKeyId"].ToString());            
            ItemIdx = Convert.ToInt32(dr["ItemIdx"].ToString());
            KeyStr = dr["KeyList"].ToString();
        }

        public override void WriteToDatabase() {
            //if new hotkey find last idx of the commands item to make this the next one
            if (HotKeyId == 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpHotKey where fk_MpCommandId=" + CommandId + " ORDER BY ItemIdx DESC");
                if (dt != null && dt.Rows.Count > 0) {
                    ItemIdx = Convert.ToInt32(dt.Rows[0]["ItemIdx"].ToString()) + 1;
                } else {
                    ItemIdx = 1;
                }
                MpDb.Instance.ExecuteNonQuery("insert into MpHotKey(fk_MpCommandId,ItemIdx,KeyList) values(" + CommandId + "," + ItemIdx + ",'" + KeyStr + "')");
                HotKeyId = MpDb.Instance.GetLastRowId("MpHotKey", "pk_MpHotKeyId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpHotKey set KeyList='" + KeyStr + "' where pk_MpHotKeyId=" + HotKeyId);
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpHotKey where pk_MpHotKeyId=" + this.HotKeyId);
        }
        private void MapDataToColumns() {
            TableName = "MpHotKey";
            columnData.Clear();
            columnData.Add("pk_MpHotKeyId", this.HotKeyId);
            columnData.Add("ItemIdx", this.ItemIdx);
            columnData.Add("KeyList", this.KeyStr);
        }

        public int CompareTo(object obj) {
            var otherHotkey = (MpHotKey)obj;
            if(otherHotkey.KeyStr == KeyStr) {
                return 0;
            }
            return -1;
        }
                
        public void AddKey(Key key) {
            if(!_keyList.Contains(key)) {
                _keyList.Add(key);
                switch (key) {
                    case Key.LeftCtrl:
                        KeyStr += "Control";
                        break;
                    case Key.LeftShift:
                        KeyStr += "Shift";
                        break;
                    case Key.LeftAlt:
                        KeyStr += "Alt"; 
                        break;
                    case Key.LWin:
                        KeyStr += "LWin";
                        break;
                    default:
                        KeyStr += "+" + key.ToString();
                        break;
                }
            }
            if(KeyStr.StartsWith("+")) {
                KeyStr = KeyStr.Remove(0, 1);
            }
        }

        public override string ToString() {
            return KeyStr;
        }
    }
}
