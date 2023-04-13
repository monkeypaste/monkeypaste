using MonkeyPaste.Common;
using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        Contains = 0,
        Exact,
        BeginsWith,
        EndsWith,
        Regex,
        //Automatic,
        //Wildcard,
        //Lexical // like azure cognitive search?
    }

    public enum MpTriggerType {
        None = 0,
        ContentAdded,
        FileSystemChange,
        ContentTagged,
        Shortcut,
        //ActiveAppChanged,
        //ClipboardChanged
    }

    public enum MpActionType {
        //ActionObjId(below)
        None = 0,
        Classify,  //tagid
        Analyze,   //analyticItemPresetId
        Conditional,   //MatcherTypeEnumId
        Trigger,    //TriggerTypeEnumId 
        Repeater,
        FileWriter,
        AddFromClipboard,
        KeySimulator,
        Delay
    }

    public class MpAction : MpDbModelBase, MpIClonableDbModel<MpAction> {

        #region Interfaces

        #region MpIClonableDbModel Implementation

        public async Task<MpAction> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false) {
            // NOTE parentId must be set after calling this method
            // NOTE2 If is Shortcut trigger, shortcut not cloned

            // CLONE ACTION 
            var cloned_a = await CreateAsync(
                actionType: ActionType,
                label: Label,
                parentId: 0,
                sortOrderIdx: SortOrderIdx,
                arg1: Arg1,
                arg2: Arg2,
                arg3: Arg3,
                arg4: Arg4,
                description: Description,
                isReadOnly: IsModelReadOnly,
                location: new MpPoint(X, Y),
                suppressWrite: suppressWrite);
            if (!deepClone) {
                return cloned_a;
            }

            // CLONE CHILDREN & UPDATE PARENT

            var child_al = await MpDataModelProvider.GetChildActionsAsync(Id);
            foreach (var (ca, caidx) in child_al.OrderBy(x => x.SortOrderIdx).WithIndex()) {
                var cloned_ca = await ca.CloneDbModelAsync(
                    deepClone: deepClone,
                    suppressWrite: suppressWrite);
                cloned_ca.ParentActionId = cloned_a.Id;
                cloned_ca.SortOrderIdx = caidx;
                if (!suppressWrite) {
                    await cloned_ca.WriteToDatabaseAsync();
                }
            }

            // CLONE PARAMETERS & UPDATE HOST 
            var pvl = await MpDataModelProvider.GetAllParameterHostValuesAsync(MpParameterHostType.Action, Id);

            foreach (var pv in pvl) {
                var cloned_pv = await pv.CloneDbModelAsync();
                cloned_pv.ParameterHostId = cloned_a.Id;
                if (!suppressWrite) {
                    await cloned_pv.WriteToDatabaseAsync();
                }
            }

            return cloned_a;
        }
        #endregion

        #endregion

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
            if (sortOrderIdx == 0 && parentId > 0) {
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

            if (!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public override async Task DeleteFromDatabaseAsync() {
            var apvl = await MpDataModelProvider.GetAllParameterHostValuesAsync(MpParameterHostType.Action, Id);
            await Task.WhenAll(apvl.Select(x => x.DeleteFromDatabaseAsync()));

            await base.DeleteFromDatabaseAsync();
        }

        public MpAction() { }

        public override string ToString() {
            return $"Action Id: {Id} ParentId: {ParentActionId} Type: '{ActionType}' Label: '{Label}'";
        }

    }
}
