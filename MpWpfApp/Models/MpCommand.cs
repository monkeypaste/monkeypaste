using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpCommand : MpDbObject {
        public int CommandId { get; set; }

        public List<MpHotKeyItem> HotKeyItemList = new List<MpHotKeyItem>();

        public MpCommandType CommandType { get; set; }

        public ICommand CommandRef { get; set; }


        public MpCommand() {
            CommandId = 0;
            CommandType = MpCommandType.None;
            CommandRef = null;
            ClearHotKeyList();
        }
        public MpCommand(int hkId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpCommand where pk_MpCommandId=" + hkId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpCommand(DataRow dr) {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            CommandId = Convert.ToInt32(dr["pk_MpCommandId"].ToString());
            CommandType = (MpCommandType)Convert.ToInt32(dr["fk_MpCommandTypeId"].ToString());
            HotKeyItemList = new List<MpHotKeyItem>();

            DataTable dt = MpDb.Instance.Execute("select * from MpHotKeyItem where fk_MpCommandId=" + CommandId + " ORDER BY ItemIdx");
            if (dt != null && dt.Rows.Count > 0) {
                for (int i = 0; i < dt.Rows.Count; i++) {
                    HotKeyItemList.Add(new MpHotKeyItem(dt.Rows[i]));
                }
            }
        }
        public override void WriteToDatabase() {
            if (CommandId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpCommand(fk_MpCommandTypeId) values(" + (int)CommandType+")");
                CommandId = MpDb.Instance.GetLastRowId("MpCommand", "pk_MpCommandId");
                foreach (MpHotKeyItem hki in HotKeyItemList) {
                    hki.CommandId = CommandId;
                }
            } 
            foreach(MpHotKeyItem hki in HotKeyItemList) {
                hki.WriteToDatabase();
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpCommand where pk_MpCommandId=" + this.CommandId);
            foreach (MpHotKeyItem hki in HotKeyItemList) {
                hki.DeleteFromDatabase();
            }
        }
        private void MapDataToColumns() {
            TableName = "MpCommand";
            columnData.Clear();
            columnData.Add("pk_MpCommandId", this.CommandId);
            columnData.Add("CommandName", Enum.GetName(typeof(MpCommandType), this.CommandType));            
        }

        public void ClearHotKeyList() {
            HotKeyItemList = new List<MpHotKeyItem>();
        }

        public string GetHotKeyString() {
            string outStr = string.Empty;
            foreach (MpHotKeyItem hki in HotKeyItemList) {
                outStr += hki.ToString() + ",";
            }
            if(outStr == string.Empty) {
                return outStr;
            }
            return outStr.Remove(outStr.Length - 1, 1);
        }
        public override string ToString() {
            string outStr = "Command: " + Enum.GetName(typeof(MpCommandType),CommandType);
            outStr += GetHotKeyString();
            return outStr;
        }
    }
    public enum MpCommandType {
        None = 0,
        ShowWindow,
        HideWindow,
        AppendMode,
        AutoCopyMode,
        RightClickPasteMode,
        PasteSelectedClip,
        DeleteSelectedClip,
        Search,
        PasteClip,
        Custom
    }
}
