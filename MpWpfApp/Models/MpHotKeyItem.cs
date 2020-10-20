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
        public string KeyList { get; private set; }
        public string ModList { get; private set; }

        public MpHotKeyItem() {
            HotkeyItemId = 0;
            ItemIdx = 0;
            CommandId = 0;
            KeyList = ModList = string.Empty;
        }
        public MpHotKeyItem(string keyList) : this() {
            KeyList = keyList;
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
            KeyList = dr["KeyList"].ToString();
            ModList = dr["ModList"].ToString();
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
                MpDb.Instance.ExecuteNonQuery("insert into MpHotKeyItem(fk_MpCommandId,ItemIdx,KeyList,ModList) values(" + CommandId + "," + ItemIdx + ",'" + KeyList + "','" + ModList + "')");
                HotkeyItemId = MpDb.Instance.GetLastRowId("MpHotKeyItem", "pk_MpHotKeyItemId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpHotKeyItem set KeyList='" + KeyList + "', ModKey='" + ModList + "' where pk_MpHotKeyItemId=" + HotkeyItemId);
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
            columnData.Add("KeyList", this.KeyList);
            columnData.Add("ModList", this.ModList);
        }

        public int CompareTo(object obj) {
            var otherHotkey = (MpHotKeyItem)obj;
            if(otherHotkey.KeyList == KeyList && otherHotkey.ModList == ModList) {
                return 0;
            }
            return -1;
        }
                
        public void AddKey(Key key) {
            KeyList += key.ToString() + Delimeter;
        }

        public void RemoveKey(Key key) {
            var keyList = KeyList.Split(Delimeter.ToCharArray());
            var toRemoveKeyList = new List<string>();
            foreach (string k in keyList) {
                if(k == key.ToString()) {
                    toRemoveKeyList.Add(k);
                }
            }
            KeyList = string.Empty;
            foreach (string k in keyList) {
                if (!toRemoveKeyList.Contains(k)) {
                    KeyConverter kc = new KeyConverter();
                    AddKey((Key)kc.ConvertFromString(k));
                }
            }
        }

        public override string ToString() {
            var keyList = KeyList.Split(Delimeter.ToCharArray());
            if(keyList.Length == 0) {
                return string.Empty;
            }
            string outStr = "<";
            foreach(string k in keyList) {
                outStr += k + " ";
            }
            outStr = outStr.Trim() + ">";
            return outStr;
        }
    }
}
