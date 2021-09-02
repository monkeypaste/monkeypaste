using System;
using System.Collections.Generic;
using MonkeyPaste;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpKeyCombination {

    }
    public class MpShortcut : MpDbModelBase {
        #region Public Properties
        public int ShortcutId { get; set; } = 0;
        public int CopyItemId { get; set; } = 0;
        public int TagId { get; set; } = 0;
        public string ShortcutName { get; set; } = string.Empty;
        public string KeyString { get; set; } = string.Empty;
        public string DefaultKeyString { get; set; } = string.Empty;
        public MpRoutingType RoutingType { get; set; } = MpRoutingType.None;

        public List<List<Key>> KeyList {
            get {
                var keyList = new List<List<Key>>();
                var combos = KeyString.Split(new string[] { ", " },StringSplitOptions.RemoveEmptyEntries).ToList<string>();                
                foreach(var c in combos) {
                    var keys = c.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    keyList.Add(new List<Key>());
                    foreach(var k in keys) {
                        keyList[keyList.Count - 1].Add(MpHelpers.Instance.ConvertStringToKey(k));
                    }
                }
                return keyList;
            }
        }
        #endregion

        #region Private Variables
        #endregion

        #region Static Methods
        public static List<MpShortcut> GetAllShortcuts() {
            List<MpShortcut> commands = new List<MpShortcut>();
            DataTable dt = MpDb.Instance.Execute("select * from MpShortcut", null);
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
        public static List<MpShortcut> GetShortcutListByCopyItemId(int copyItemId) {
            return GetAllShortcuts().Where(x => x.CopyItemId == copyItemId).ToList();
        }
        public static List<MpShortcut> GetShortcutByTagId(int tagId) {
            return GetAllShortcuts().Where(x => x.TagId == tagId).ToList();
        }
        #endregion

        #region Public Methods
        public MpShortcut() {
            ShortcutId = 0;
            ShortcutName = string.Empty;
            KeyString = string.Empty;
            DefaultKeyString = string.Empty;
            RoutingType = MpRoutingType.None;
            CopyItemId = 0;
            TagId = 0;
        }
        public MpShortcut(int hkId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpShortcut where pk_MpShortcutId=@hkid",
                new Dictionary<string, object> {
                    { "@hkid", hkId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpShortcut(int copyItemId, int tagId, string keyString, string shortcutName) : this() {
            ShortcutName = shortcutName;
            CopyItemId = copyItemId;
            TagId = tagId;
            KeyString = keyString;
            RoutingType = TagId > 0 ? MpRoutingType.Internal : MpRoutingType.Direct;
        }

        public MpShortcut(DataRow dr) {
            LoadDataRow(dr);
        }
        public void Reset() {
            KeyString = DefaultKeyString;
        }

        public override void LoadDataRow(DataRow dr) {
            ShortcutId = Convert.ToInt32(dr["pk_MpShortcutId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            TagId = Convert.ToInt32(dr["fk_MpTagId"].ToString());
            ShortcutName = dr["ShortcutName"].ToString();
            KeyString = dr["KeyString"].ToString();
            DefaultKeyString = dr["DefaultKeyString"].ToString();
            
            RoutingType = (MpRoutingType)Convert.ToInt32(dr["RoutingType"].ToString());            
        }

        public override void WriteToDatabase() {
            if (ShortcutId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpShortcut(ShortcutName,RoutingType,KeyString,DefaultKeyString,fk_MpCopyItemId,fk_MpTagId) values(@sn,@rt,@ks,@dks,@ciid,@tid)",
                    new Dictionary<string, object> {
                        { "@sn", ShortcutName},
                        { "@rt", (int)RoutingType},
                        { "@ks", KeyString},
                        { "@dks", DefaultKeyString},
                        { "@ciid", CopyItemId},
                        { "@tid", TagId}
                    });
                ShortcutId = MpDb.Instance.GetLastRowId("MpShortcut", "pk_MpShortcutId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpShortcut set ShortcutName=@sn, KeyString=@ks, DefaultKeyString=@dks, fk_MpCopyItemId=@ciid, fk_MpTagId=@tid, RoutingType=@rtid where pk_MpShortcutId=@sid",
                    new Dictionary<string, object> {
                        { "@sn", ShortcutName},
                        { "@rtid", (int)RoutingType},
                        { "@ks", KeyString},
                        { "@dks", DefaultKeyString},
                        { "@ciid", CopyItemId},
                        { "@tid", TagId},
                        { "@sid", ShortcutId }
                    });
            }
        }

        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpShortcut where pk_MpShortcutId=@sid",
                new Dictionary<string, object> {
                    { "@sid", ShortcutId }
                });
        }

        public override string ToString() {
            string outStr = "Shortcut Name: " + ShortcutName + " Id: " + ShortcutId;
            outStr += " " + KeyString;
            return outStr;
        }
        #endregion
    }
    public enum MpRoutingType {
        None = 0,
        Internal, //1
        Direct, //2
        Bubble, //3 sendkey before
        Tunnel  //4 sendkey after
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
