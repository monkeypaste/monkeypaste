using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;

namespace MpWpfApp {
    public class MpShortcut : MpDbObject {
        #region Public Properties
        public int ShortcutId { get; set; } = 0;
        public string ShortcutName { get; set; } = "None";
        public string KeyList { get; set; } = string.Empty;
        public string DefaultKeyList { get; set; } = string.Empty;
        public bool IsGlobal { get; set; } = false;
        public int CopyItemId { get; set; } = 0;
        public int TagId { get; set; } = 0;

        #endregion

        #region Private Variables
        #endregion

        #region Static Methods
        public static List<MpShortcut> GetAllShortcuts() {
            List<MpShortcut> commands = new List<MpShortcut>();
            DataTable dt = MpDb.Instance.Execute("select * from MpShortcut");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    commands.Add(new MpShortcut(dr));
                }
            }
            return commands;
        }
        public static List<MpShortcut> GetShortcutByName(string shortcutName) {
            return GetAllShortcuts().Where(x => x.ShortcutName == shortcutName).ToList();
        }
        public static List<MpShortcut> GetShortcutByCopyItemId(int copyItemId) {
            return GetAllShortcuts().Where(x => x.CopyItemId == copyItemId).ToList();
        }
        public static List<MpShortcut> GetShortcutByTagId(int tagId) {
            return GetAllShortcuts().Where(x => x.TagId == tagId).ToList();
        }
        #endregion

        #region Public Methods
        public MpShortcut() {
            ShortcutId = 0;
            ShortcutName = "None";
            KeyList = string.Empty;
            DefaultKeyList = string.Empty;
            IsGlobal = false;
            CopyItemId = -1;
            TagId = -1;
        }
        public MpShortcut(int hkId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpShortcut where pk_MpShortcutId=" + hkId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpShortcut(DataRow dr) {
            LoadDataRow(dr);
        }
        public void Reset() {
            KeyList = DefaultKeyList;
        }

        public override void LoadDataRow(DataRow dr) {
            ShortcutId = Convert.ToInt32(dr["pk_MpShortcutId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            TagId = Convert.ToInt32(dr["fk_MpTagId"].ToString());
            ShortcutName = dr["ShortcutName"].ToString();
            KeyList = dr["KeyList"].ToString();
            DefaultKeyList = dr["DefaultKeyList"].ToString();
            //this is used to set default application shortcuts when db is first made
            if((KeyList == null || KeyList == string.Empty) && DefaultKeyList.Length > 0) {
                KeyList = DefaultKeyList;
            }
            IsGlobal = Convert.ToInt32(dr["IsGlobal"].ToString()) == 1;
        }
        public override void WriteToDatabase() {
            if (ShortcutId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpShortcut(ShortcutName,IsGlobal,KeyList,DefaultKeyList,fk_MpCopyItemId,fk_MpTagId) VALUES('" + ShortcutName + "'," + (IsGlobal == true ? 1 : 0) + ",'" + KeyList + "','" + DefaultKeyList + "'," + CopyItemId + "," + TagId + ")");
                ShortcutId = MpDb.Instance.GetLastRowId("MpShortcut", "pk_MpShortcutId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpShortcut set ShortcutName='" + ShortcutName + "', KeyList='" + KeyList + "', DefaultKeyList='" + DefaultKeyList + "', fk_MpCopyItemId=" + CopyItemId + ", fk_MpTagId=" + TagId + " where pk_MpShortcutId=" + ShortcutId);
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpShortcut where pk_MpShortcutId=" + this.ShortcutId);
        }
        private void MapDataToColumns() {
            TableName = "MpShortcut";
            columnData.Clear();
            columnData.Add("pk_MpShortcutId", this.ShortcutId);
        }
        public override string ToString() {
            string outStr = "Shortcut Name: " + ShortcutName;
            outStr += " " + KeyList;
            return outStr;
        }
        #endregion
    }
    public enum MpShortcutType {
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
