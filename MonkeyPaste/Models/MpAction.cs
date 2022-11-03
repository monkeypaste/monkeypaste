using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public enum MpMacroActionType {
        None = 0,
        Tokenize,
        Command
    }

    public enum MpMacroCommandType {
        None = 0,
        Local,
        Remote
    }

    public enum MpComparisonOperatorType {
        None = 0,
        Contains,
        Exact,
        BeginsWith,
        EndsWith,
        Regex,
        Automatic,
        Wildcard,
        Lexical // like azure cognitive search?
    }

    public enum MpTriggerType {
        None = 0,
        ContentAdded, 
        FileSystemChange,
        ContentTagged,
        Shortcut,
        ParentOutput
    }

    public enum MpActionType {
                   //ActionObjId(below)
        None = 0, 
        Classify,  //tagid
        Analyze,   //analyticItemPresetId
        Compare,   //MatcherTypeEnumId
        Trigger,    //TriggerTypeEnumId 
        Macro,
        Timer,
        FileWriter,
        Annotater
    }

    public class MpAction : MpDbModelBase {

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpActionId")]
        public override int Id { get; set; }

        [Column("MpActionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_ParentActionId")]
        public int ParentActionId { get; set; } = 0;

        [Column("fk_MpIconId")]
        public int IconId { get; set; } = 0;

        public int SortOrderIdx { get; set; } = 0;

        [Column("e_MpActionTypeId")]
        public int ActionTypeId { get; set; } = 0;

        [Column("fk_ActionObjId")]
        public int ActionObjId { get; set; } = 0;

        public string Arg1 { get; set; } = null;

        public string Arg2 { get; set; } = null;

        public string Arg3 { get; set; } = null;

        public string Arg4 { get; set; } = null;

        public string Arg5 { get; set; } = null;

        public double X { get; set; }

        public double Y { get; set; }

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ReadOnly { get; set; } = 0;

        [Column("b_IsEnabled")]
        public int IsEnabledValue { get; set; }

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid ActionGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [Ignore]
        public override bool IsReadOnly {
            get => ReadOnly == 1;
            set => ReadOnly = value ? 1 : 0;
        }

        [Ignore]
        public bool IsEnabled {
            get => IsEnabledValue == 1;
            set => IsEnabledValue = value ? 1 : 0;
        }


        [Ignore]
        public MpActionType ActionType {
            get => (MpActionType)ActionTypeId;
            set => ActionTypeId = (int)value;
        }


        #endregion

        public static async Task<MpAction> CreateAsync(
            MpActionType actionType = MpActionType.None,
            int actionObjId = 0,
            string label = "",
            int parentId = 0,
            int sortOrderIdx = 0,
            string arg1 = "",
            string arg2 = "",
            string arg3 = "",
            string arg4 = "",
            int iconId = 0,
            string description = "",
            bool isReadOnly = false,
            MpPoint location = null,
            bool suppressWrite = false) {

            //var dupCheck = await MpDataModelProvider.GetActionByLabel(label);
            //if(dupCheck != null) {
            //    MpConsole.WriteTraceLine("Action must have unique name");
            //    return null;
            //}
            if(sortOrderIdx == 0 && parentId > 0) {
                sortOrderIdx = await MpDataModelProvider.GetChildActionCountAsync(parentId);
            }
            if(!suppressWrite && actionType == MpActionType.Trigger && iconId == 0) {
                string iconStr = null;
                switch ((MpTriggerType)actionObjId) {
                    case MpTriggerType.ContentAdded:
                        iconStr = MpBase64Images.ClipboardIcon;
                        break;
                    case MpTriggerType.ContentTagged:
                        iconStr = MpBase64Images.TagIcon;
                        break;
                    case MpTriggerType.FileSystemChange:
                        iconStr = MpBase64Images.FolderChangedIcon;
                        break;
                    case MpTriggerType.Shortcut:
                        iconStr = MpBase64Images.JoystickUnset;
                        break;
                    case MpTriggerType.ParentOutput:
                        iconStr = MpBase64Images.ChainIcon;
                        break;
                }
                if (string.IsNullOrEmpty(iconStr)) {
                    iconId = MpPrefViewModel.Instance.ThisAppIcon.Id;
                } else { 
                    var icon = await MpIcon.Create(
                        iconImgBase64: iconStr, 
                        createBorder: false);
                    iconId = icon.Id;
                }
            }
            location = location == null ? new MpPoint() : location;
            var mr = new MpAction() {
                ActionGuid = System.Guid.NewGuid(),
                Label = label,
                ActionType = actionType,
                ActionObjId = actionObjId,
                Arg1 = arg1,
                Arg2 = arg2,
                Arg3 = arg3,
                Arg4 = arg4,
                ParentActionId = parentId,
                SortOrderIdx = sortOrderIdx,
                IconId = iconId,
                Description = description,
                X = location.X,
                Y = location.Y
            };

            if(!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public MpAction() { }
    }
}
