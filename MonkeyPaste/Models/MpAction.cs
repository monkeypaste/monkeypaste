using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpCompareType {
        None = 1,
        Contains = 2,
        Exact = 4,
        BeginsWith = 8,
        EndsWith = 16,
        Regex = 32,
        Source = 64,
        Automatic = 128,
        Wildcard = 256,
        Lexical = 512 // like azure cognitive search?
    }

    public enum MpTriggerType {
        None = 0,
        ContentItemAdded, 
        FileSystemChange,
        ContentItemAddedToTag,
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
        Timer
    }

    public class MpAction : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpActionId")]
        public override int Id { get; set; }

        [Column("MpActionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAction))]
        [Column("fk_ParentActionId")]
        public int ParentActionId { get; set; } = 0;

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; } = 0;

        public int SortOrderIdx { get; set; } = 0;

        [Column("e_MpActionTypeId")]
        public int ActionTypeId { get; set; } = 0;

        [Column("fk_ActionObjId")]
        public int ActionObjId { get; set; } = 0;

        public string Arg1 { get; set; } = string.Empty;

        public string Arg2 { get; set; } = string.Empty;

        public string Arg3 { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ReadOnly { get; set; } = 0;
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
        public bool IsReadOnly {
            get => ReadOnly == 1;
            set => ReadOnly = value ? 1 : 0;
        }


        [Ignore]
        public MpActionType ActionType {
            get => (MpActionType)ActionTypeId;
            set => ActionTypeId = (int)value;
        }


        #endregion

        public static async Task<MpAction> Create(
            string label,
            MpActionType actionType,
            int actionObjId,
            int parentId = 0,
            int sortOrderIdx = 0,
            string arg1 = "",
            string arg2 = "",
            string arg3 = "",
            int iconId = 0,
            string description = "",
            bool isReadOnly = false) {

            //var dupCheck = await MpDataModelProvider.GetActionByLabel(label);
            //if(dupCheck != null) {
            //    MpConsole.WriteTraceLine("Action must have unique name");
            //    return null;
            //}
            if(sortOrderIdx == 0 && parentId > 0) {
                sortOrderIdx = await MpDataModelProvider.GetChildActionCount(parentId);
            }
            if(actionType == MpActionType.Trigger && iconId == 0) {
                string iconStr = null;
                switch ((MpTriggerType)actionObjId) {
                    case MpTriggerType.ContentItemAdded:
                        iconStr = MpBase64Images.ClipboardIcon;
                        break;
                    case MpTriggerType.ContentItemAddedToTag:
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
                    iconId = MpPreferences.ThisAppSource.PrimarySource.SourceIcon.Id;
                } else { 
                    var icon = await MpIcon.Create(iconStr, false);
                    iconId = icon.Id;
                }
            }

            var mr = new MpAction() {
                ActionGuid = System.Guid.NewGuid(),
                Label = label,
                ActionType = actionType,
                ActionObjId = actionObjId,
                Arg1 = arg1,
                Arg2 = arg2,
                Arg3 = arg3,
                ParentActionId = parentId,
                SortOrderIdx = sortOrderIdx,
                IconId = iconId,
                Description = description,
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpAction() { }
    }
}
