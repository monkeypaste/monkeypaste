using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpHotKeyItem : MpDbObject, IComparable {
        public static string Delimeter = "+";

        public int HotkeyItemId { get; set; }
        public int CommandId { get; set; }
        public int ItemIdx { get; set; }
        public string KeyStr { get; private set; }

        private List<Key> _keyList = new List<Key>();

        public MpHotKeyItem() {
            HotkeyItemId = 0;
            ItemIdx = 0;
            CommandId = 0;
            KeyStr = string.Empty;
            _keyList = new List<Key>();
        }
        public MpHotKeyItem(string keyList) : this() {
            KeyStr = keyList;
        }

        public MpHotKeyItem(int hkId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpHotKeyItem where pk_MpHotKeyItemId=" + hkId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpHotKeyItem(DataRow dr) {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            HotkeyItemId = Convert.ToInt32(dr["pk_MpHotKeyItemId"].ToString());
            ItemIdx = Convert.ToInt32(dr["ItemIdx"].ToString());
            KeyStr = dr["KeyList"].ToString();
        }

        public override void WriteToDatabase() {
            //if new hotkey find last idx of the commands item to make this the next one
            if (HotkeyItemId == 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpHotKeyItem where fk_MpCommandId=" + CommandId + " ORDER BY ItemIdx DESC");
                if (dt != null && dt.Rows.Count > 0) {
                    ItemIdx = Convert.ToInt32(dt.Rows[0]["ItemIdx"].ToString()) + 1;
                } else {
                    ItemIdx = 1;
                }
                MpDb.Instance.ExecuteNonQuery("insert into MpHotKeyItem(fk_MpCommandId,ItemIdx,KeyList,ModList) values(" + CommandId + "," + ItemIdx + ",'" + KeyStr + "')");
                HotkeyItemId = MpDb.Instance.GetLastRowId("MpHotKeyItem", "pk_MpHotKeyItemId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpHotKeyItem set KeyList='" + KeyStr + "' where pk_MpHotKeyItemId=" + HotkeyItemId);
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpHotKeyItem where pk_MpHotKeyItemId=" + this.HotkeyItemId);
        }
        private void MapDataToColumns() {
            TableName = "MpHotKey";
            columnData.Clear();
            columnData.Add("pk_MpHotKeyItemId", this.HotkeyItemId);
            columnData.Add("ItemIdx", this.ItemIdx);
            columnData.Add("KeyList", this.KeyStr);
        }

        public int CompareTo(object obj) {
            var otherHotkey = (MpHotKeyItem)obj;
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
