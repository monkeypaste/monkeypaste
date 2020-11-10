
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Reactive.Linq;
using Gma.System.MouseKeyHook;
using MouseKeyHook.Rx;

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

        public ICommand Command { get; set; }
        #endregion

        #region Private Variables
        private IDisposable _keysObservable = null;
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
            Command = null;
            KeyList = string.Empty;
            DefaultKeyList = string.Empty;
            _keysObservable = null;
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
        public bool UnregisterShortcut() {
            if(_keysObservable != null) {
                _keysObservable.Dispose();
                return true;
            }
            return false;
        }
        public bool RegisterShortcutCommand(ICommand icommand) {
            try {
                UnregisterShortcut();
                var mwvm = (MpMainWindowViewModel)System.Windows.Application.Current.MainWindow.DataContext;
                
                if (IsSequence()) {
                    var seq = new Trigger[] { Trigger.FromString(KeyList) };
                    //var sequenceAssignments = new Dictionary<Sequence, Action>();
                    //sequenceAssignments.Add(Sequence.FromString(KeyList), () => icommand.Execute(null));
                    if (IsGlobal) {
                        //mwvm.GlobalHook.OnSequence(sequenceAssignments);
                        _keysObservable = mwvm.GlobalHook.KeyDownObservable().Matching(seq).Subscribe((trigger) => {
                            //Debug.WriteLine(trigger.ToString());
                            icommand?.Execute(null);
                        });
                    } else {
                        //mwvm.ApplicationHook.OnSequence(sequenceAssignments);
                        _keysObservable = mwvm.ApplicationHook.KeyDownObservable().Matching(seq).Subscribe((trigger) => {
                            //Debug.WriteLine(trigger.ToString());
                            icommand?.Execute(null);
                        });
                    }
                } else {
                    var comb = new Trigger[] { Trigger.FromString(KeyList) };
                    //var combinationAssignments = new Dictionary<Combination, Action>();
                    //combinationAssignments.Add(Combination.FromString(KeyList), () => icommand.Execute(null));
                    if(IsGlobal) {
                        //mwvm.GlobalHook.OnCombination(combinationAssignments);
                        _keysObservable = mwvm.GlobalHook.KeyDownObservable().Matching(comb).Subscribe((trigger) => {
                            //Debug.WriteLine(trigger.ToString());
                            icommand?.Execute(null);
                        });
                    } else {
                        //mwvm.ApplicationHook.OnCombination(combinationAssignments);
                        _keysObservable = mwvm.ApplicationHook.KeyDownObservable().Matching(comb).Subscribe((trigger) => {
                            //Debug.WriteLine(trigger.ToString());
                            icommand?.Execute(null);
                        });
                    }
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
