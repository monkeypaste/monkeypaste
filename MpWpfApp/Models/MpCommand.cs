using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpCommand : MpDbObject {
        private static List<MpCommand> _CommandList = new List<MpCommand>();

        public int CommandId { get; set; } = 0;
        public string CommandName { get; set; } = "None";
        public string KeyList { get; private set; } = string.Empty;
        public bool IsGlobal { get; set; } = false;
        public int CopyItemId { get; set; } = 0;

        private List<List<Key>> _keyList = new List<List<Key>>();

        #region Static Methods
        public static List<MpCommand> GetAllCommands() {
            List<MpCommand> commands = new List<MpCommand>();
            DataTable dt = MpDb.Instance.Execute("select * from MpCommand");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    commands.Add(new MpCommand(dr));
                }
            }
            return commands;
        }
        public static List<MpCommand> GetCommandByName(string commandName) {
            return GetAllCommands().Where(x => x.CommandName == commandName).ToList();
        }
        public static List<MpCommand> GetCommandByCopyItemId(int copyItemId) {
            return GetAllCommands().Where(x => x.CopyItemId == copyItemId).ToList();
        }
        #endregion
        #region Public Methods
        public MpCommand() {
            CommandId = 0;
            CommandName = "None";
            KeyList = string.Empty;
            _keyList = new List<List<Key>>();
            IsGlobal = false;
            CopyItemId = 0;
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
            CommandId = Convert.ToInt32(dr["pk_MpCommandId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemid"].ToString());
            CommandName = dr["CommandName"].ToString();
            KeyList = dr["KeyList"].ToString();
            IsGlobal = Convert.ToInt32(dr["IsGlobal"].ToString()) == 1;

        }
        public override void WriteToDatabase() {
            if (CommandId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpCommand(CommandName,IsGlobal,KeyList) VALUES('" + CommandName + "'," + (IsGlobal == true ? 1:0) + ",'" + KeyList + "')"); ;
                CommandId = MpDb.Instance.GetLastRowId("MpCommand", "pk_MpCommandId");
            } 
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpCommand where pk_MpCommandId=" + this.CommandId);
        }
        private void MapDataToColumns() {
            TableName = "MpCommand";
            columnData.Clear();
            columnData.Add("pk_MpCommandId", this.CommandId);
        }
        public bool IsSequence() {
            return KeyList.Contains(",");
        }
        public void ClearHotKeyList() {
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
