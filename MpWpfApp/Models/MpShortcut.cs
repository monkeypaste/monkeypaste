using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpShortcut : MpDbObject {
        private static List<MpShortcut> _CommandList = new List<MpShortcut>();

        public int ShortcutId { get; set; } = 0;
        public string ShortcutName { get; set; } = "None";
        public string KeyList { get; set; } = string.Empty;
        public string DefaultKeyList { get; set; } = string.Empty;
        public bool IsGlobal { get; set; } = false;
        public int CopyItemId { get; set; } = 0;
        public int TagId { get; set; } = 0;

        public ICommand Command { get; set; }

        private List<List<Key>> _keyList = new List<List<Key>>();

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
            Command = null;
            KeyList = string.Empty;
            DefaultKeyList = string.Empty;
            _keyList = new List<List<Key>>();
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
        public bool RegisterShortcutCommand(ICommand icommand) {
            try {
                var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
                if (IsSequence()) {
                    var sequenceAssignments = new Dictionary<Sequence, Action>();
                    sequenceAssignments.Add(Sequence.FromString(KeyList), () => icommand.Execute(null));
                    mwvm.GlobalHook.OnSequence(sequenceAssignments);
                } else {
                    var combinationAssignments = new Dictionary<Combination, Action>();
                    combinationAssignments.Add(Combination.FromString(KeyList), () => icommand.Execute(null));
                    mwvm.GlobalHook.OnCombination(combinationAssignments);
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error creating shortcut: " + ex.ToString());
                return false;
            }
            Console.WriteLine("Shortcut Successfully registered for '" + ShortcutName + "' with hotkeys: " + KeyList);
            Command = icommand;
            return true;
        }
        public void AddKey(Key key, bool isNewCombination) {
            if(isNewCombination && KeyList.Length > 0) {
                KeyList += ",";
                _keyList.Add(new List<Key>());
            }
            if(_keyList.Count == 0) {
                _keyList.Add(new List<Key>());
            }
            if (!_keyList[_keyList.Count - 1].Contains(key)) {
                _keyList[_keyList.Count - 1].Add(key);
                switch (key) {
                    case Key.LeftCtrl:
                        KeyList += "+Control";
                        break;
                    case Key.LeftShift:
                        KeyList += "+Shift";
                        break;
                    case Key.LeftAlt:
                        KeyList += "+Alt";
                        break;
                    case Key.LWin:
                        KeyList += "+LWin";
                        break;
                    default:
                        KeyList += "+" + key.ToString();
                        break;
                }
            }
            if (KeyList.StartsWith("+")) {
                KeyList = KeyList.Remove(0, 1);
            }
            KeyList = KeyList.Replace(",+", ",");
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
        public bool IsSequence() {
            return KeyList.Contains(",");
        }
        public bool IsCustom() {
            return CopyItemId > 0 || TagId > 0;
        }
        public void ClearKeyList() {
            KeyList = string.Empty;
            _keyList = new List<List<Key>>();
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
