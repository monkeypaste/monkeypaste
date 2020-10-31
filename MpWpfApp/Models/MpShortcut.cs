using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpShortcut : MpDbObject {
        private static List<MpShortcut> _CommandList = new List<MpShortcut>();

        public int CommandId { get; set; } = 0;
        public string CommandName { get; set; } = "None";
        public string KeyList { get; private set; } = string.Empty;
        public bool IsGlobal { get; set; } = false;
        public int CopyItemId { get; set; } = 0;

        public ICommand Command { get; set; }

        private List<List<Key>> _keyList = new List<List<Key>>();

        #region Static Methods
        public static List<MpShortcut> GetAllCommands() {
            List<MpShortcut> commands = new List<MpShortcut>();
            DataTable dt = MpDb.Instance.Execute("select * from MpShortcut");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    commands.Add(new MpShortcut(dr));
                }
            }
            return commands;
        }
        public static List<MpShortcut> GetCommandByName(string commandName) {
            return GetAllCommands().Where(x => x.CommandName == commandName).ToList();
        }
        public static List<MpShortcut> GetCommandByCopyItemId(int copyItemId) {
            return GetAllCommands().Where(x => x.CopyItemId == copyItemId).ToList();
        }
        #endregion
        #region Public Methods
        public MpShortcut() {
            CommandId = 0;
            CommandName = "None";
            Command = null;
            KeyList = string.Empty;
            _keyList = new List<List<Key>>();
            IsGlobal = false;
            CopyItemId = 0;
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

        public bool RegisterShortcutCommand(ICommand icommand) {
            try {
                var combinationAssignments = new Dictionary<Combination, Action>();
                var sequenceAssignments = new Dictionary<Sequence, Action>();

                if (IsSequence()) {
                    sequenceAssignments.Add(Sequence.FromString(KeyList), () => icommand.Execute(null));
                } else {
                    combinationAssignments.Add(Combination.FromString(KeyList), () => icommand.Execute(null));
                }

                var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
                if (sequenceAssignments.Count > 0) {
                    mwvm.GlobalHook.OnSequence(sequenceAssignments);
                }
                if (combinationAssignments.Count > 0) {
                    mwvm.GlobalHook.OnCombination(combinationAssignments);
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error creating shortcut: " + ex.ToString());
                return false;
            }
            Console.WriteLine("Shortcut Successfully registered for '" + CommandName + "' with hotkeys: " + KeyList);
            Command = icommand;
            WriteToDatabase();
            return true;
        }
        public void AddKey(Key key,bool isNewCombination) {
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
            CommandId = Convert.ToInt32(dr["pk_MpShortcutId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemid"].ToString());
            CommandName = dr["CommandName"].ToString();
            KeyList = dr["KeyList"].ToString();
            IsGlobal = Convert.ToInt32(dr["IsGlobal"].ToString()) == 1;

        }
        public override void WriteToDatabase() {
            if (CommandId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpShortcut(CommandName,IsGlobal,KeyList) VALUES('" + CommandName + "'," + (IsGlobal == true ? 1:0) + ",'" + KeyList + "')"); ;
                CommandId = MpDb.Instance.GetLastRowId("MpShortcut", "pk_MpShortcutId");
            } 
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpShortcut where pk_MpShortcutId=" + this.CommandId);
        }
        private void MapDataToColumns() {
            TableName = "MpShortcut";
            columnData.Clear();
            columnData.Add("pk_MpShortcutId", this.CommandId);
        }
        public bool IsSequence() {
            return KeyList.Contains(",");
        }
        public void ClearKeyList() {
            KeyList = string.Empty;
            _keyList = new List<List<Key>>();
        }
        public override string ToString() {
            string outStr = "Command Name: " + CommandName;
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
