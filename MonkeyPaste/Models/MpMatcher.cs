using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpMatcherType {
        None = 1,
        Contains = 2,
        Exact = 4,
        BeginsWith = 8,
        EndsWith = 16,
        Regex = 32,
        Source = 64
    }

    public enum MpMatchTriggerType {
        None = 0,
        Content, 
        File,
        Folder,
        Tag,
        Shortcut
    }

    public enum MpMatchActionType {
        None = 0,
        Classifier, //tagid
        Analyzer, //analyticItemPresetId
        Transformer //copyItemId
    }

    public class MpMatcher : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpMatcherId")]
        public override int Id { get; set; }

        [Column("MpMatcherGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpMatcherTypeId")]
        public int MatcherTypeId { get; set; }

        [Column("e_MpMatchTriggerTypeId")]
        public int MatchTriggerTypeId { get; set; }

        public int TriggerActionTypeId { get; set; }
        public int TriggerActionObjId { get; set; }

        [Column("e_MpIsMatchActionTypeId")]
        public int IsMatchActionTypeId { get; set; }

        public int IsMatchTargetObjectId { get; set; }

        public string MatchData { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid MatcherGuid {
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
        public MpMatcherType MatcherType {
            get => (MpMatcherType)MatcherTypeId;
            set => MatcherTypeId = (int)value;
        }

        [Ignore]
        public MpMatchTriggerType TriggerType {
            get => (MpMatchTriggerType)MatchTriggerTypeId;
            set => MatchTriggerTypeId = (int)value;
        }

        [Ignore]
        public MpMatchActionType TriggerActionType {
            get => (MpMatchActionType)TriggerActionTypeId;
            set => TriggerActionTypeId = (int)value;
        }

        [Ignore]
        public MpMatchActionType IsMatchActionType {
            get => (MpMatchActionType)IsMatchActionTypeId;
            set => IsMatchActionTypeId = (int)value;
        }

        #endregion

        public static async Task<MpMatcher> Create(
            MpMatcherType matchType, 
            string matchData,

            MpMatchTriggerType trigger,            

            MpMatchActionType onTriggerAction,
            int onTriggerActionObjId,

            MpMatchActionType isMatchAction,
            int isMatchTargetObjId) {            
            var mr = new MpMatcher() {
                MatcherGuid = System.Guid.NewGuid(),

                MatcherType = matchType,
                MatchData = matchData,

                TriggerType = trigger,
                TriggerActionType = onTriggerAction,
                TriggerActionObjId = onTriggerActionObjId,

                IsMatchActionType = isMatchAction,
                IsMatchTargetObjectId = isMatchTargetObjId
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpMatcher() { }
    }
}
