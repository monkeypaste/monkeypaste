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
        Shortcut
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

        public int SortOrderIdx { get; set; } = 0;

        [Column("e_MpActionType")]
        public string ActionTypeName { get; set; }

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
        public override bool IsModelReadOnly {
            get => ReadOnly == 1;
            set => ReadOnly = value ? 1 : 0;
        }


        [Ignore]
        public MpActionType ActionType {
            get => ActionTypeName.ToEnum<MpActionType>();
            set => ActionTypeName = value.ToString();
        }


        #endregion

        public static async Task<MpAction> CreateAsync(
            MpActionType actionType = MpActionType.None,
            string label = "",
            int parentId = 0,
            int sortOrderIdx = 0,
            string arg1 = "",
            string arg2 = "",
            string arg3 = "",
            string arg4 = "",
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
            location = location == null ? new MpPoint() : location;
            var mr = new MpAction() {
                ActionGuid = System.Guid.NewGuid(),
                Label = label,
                ActionType = actionType,
                Arg1 = arg1,
                Arg2 = arg2,
                Arg3 = arg3,
                Arg4 = arg4,
                ParentActionId = parentId,
                SortOrderIdx = sortOrderIdx,
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

        public override string ToString() {
            return $"Action Id: {Id} ParentId: {ParentActionId} Type: '{ActionType}' Label: '{Label}'";
        }
    }
}
