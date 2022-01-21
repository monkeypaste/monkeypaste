using SQLite;
using SQLiteNetExtensions.Attributes;
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
        Source = 64,
        Automatic = 128,
        Wildcard = 256,
        Lexical = 512 // like azure cognitive search?
    }

    public enum MpMatchTriggerType {
        None = 0,
        ContentItemAdded, 
        WatchFileChanged,
        WatchFolderChange,
        ContentItemAddedToTag,
        Shortcut,
        ParentMatchOutput
    }

    public enum MpMatchActionType {
        None = 0,
        Classify, //tagid
        Analyze, //analyticItemPresetId
        Transform, //copyItemId
        Compare
    }

    public class MpMatcher : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpMatcherId")]
        public override int Id { get; set; }

        [Column("MpMatcherGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpMatcher))]
        [Column("fk_ParentMatcherId")]
        public int ParentMatcherId { get; set; } = 0;

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; } = 0;

        public int SortOrderIdx { get; set; } = 0;

        [Column("e_MpMatcherTypeId")]
        public int MatcherTypeId { get; set; } = 0;

        [Column("e_MpMatchTriggerTypeId")]
        public int MatchTriggerTypeId { get; set; } = 0;

        [Column("e_MpTriggerActionTypeId")]
        public int TriggerActionTypeId { get; set; } = 0;

        [Column("fk_TriggerActionObjId")]
        public int TriggerActionObjId { get; set; } = 0;

        public int MatchCount { get; set; } = 0;

        public string Title { get; set; } = string.Empty;

        public string MatchData { get; set; } = string.Empty;

        public string IsMatchPropertyPath { get; set; } = string.Empty;

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

        //[Ignore]
        //public MpMatchActionType IsMatchActionType {
        //    get => (MpMatchActionType)IsMatchActionTypeId;
        //    set => IsMatchActionTypeId = (int)value;
        //}

        #endregion

        public static async Task<MpMatcher> Create(
            MpMatcherType matchType, 
            string title,
            string matchPropertyPath,
            string matchData,

            MpMatchTriggerType trigger,            

            MpMatchActionType onTriggerAction,
            int onTriggerActionObjId,

            MpMatchActionType isMatchAction,
            int isMatchTargetObjId,
            int parentMatcherId = 0,
            int sortOrderIdx = 0) {

            string iconStr = null;
            switch(trigger) {
                case MpMatchTriggerType.ContentItemAdded:
                    iconStr = MpBase64Images.Instance.ClipboardIcon;
                    break;
                case MpMatchTriggerType.ContentItemAddedToTag:
                    iconStr = MpBase64Images.Instance.TagIcon;
                    break;
                case MpMatchTriggerType.WatchFolderChange:
                case MpMatchTriggerType.WatchFileChanged:
                    iconStr = MpBase64Images.Instance.FolderChangedIcon;
                    break;
                case MpMatchTriggerType.Shortcut:
                    iconStr = MpBase64Images.Instance.JoystickUnset;
                    break;
                case MpMatchTriggerType.ParentMatchOutput:
                    iconStr = MpBase64Images.Instance.ChainIcon;
                    break;
            }
            int iconId = MpPreferences.ThisAppSource.PrimarySource.SourceIcon.Id;
            if(!string.IsNullOrEmpty(iconStr)) {
                var icon = await MpIcon.Create(iconStr, false);
                iconId = icon.Id;
            }
            var mr = new MpMatcher() {
                MatcherGuid = System.Guid.NewGuid(),

                MatcherType = matchType,
                Title = title,
                IsMatchPropertyPath = matchPropertyPath,
                MatchData = matchData,

                TriggerType = trigger,
                TriggerActionType = onTriggerAction,
                TriggerActionObjId = onTriggerActionObjId,

                //IsMatchActionType = isMatchAction,
                //IsMatchTargetObjectId = isMatchTargetObjId
                ParentMatcherId = parentMatcherId,
                SortOrderIdx = sortOrderIdx,
                IconId = iconId
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpMatcher() { }
    }
}
