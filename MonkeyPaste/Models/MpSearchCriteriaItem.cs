using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Table("MpSearchCriteriaItem")]
    public class MpSearchCriteriaItem :
        MpDbModelBase,
        MpIIsValueEqual<MpSearchCriteriaItem>,
        MpIClonableDbModel<MpSearchCriteriaItem> {
        #region Constants

        public const MpLogicalQueryType DEFAULT_QUERY_JOIN_TYPE = MpLogicalQueryType.And;

        #endregion

        #region Interfaces

        #region MpIIsValueEqual Implementation

        public bool IsValueEqual(MpSearchCriteriaItem other) {
            if (other == null) {
                return false;
            }
            return
                QueryTagId == other.QueryTagId &&
                SortOrderIdx == other.SortOrderIdx &&
                QueryType == other.QueryType &&
                JoinType == other.JoinType &&
                Options == other.Options &&
                MatchValue == other.MatchValue &&
                IsCaseSensitive == other.IsCaseSensitive &&
                IsWholeWord == other.IsWholeWord;
        }
        #endregion

        #region MpIClonableDbModel Implementation

        public async Task<MpSearchCriteriaItem> CloneDbModelAsync(bool deepClone = true, bool suppressWrite = false) {
            // NOTE parent id is cloned if not provided
            var sci_clone = await MpSearchCriteriaItem.CreateAsync(
                tagId: QueryTagId,
                sortOrderIdx: SortOrderIdx,
                queryType: QueryType,
                joinType: JoinType,
                options: Options,
                matchValue: MatchValue,
                isCaseSensitive: IsCaseSensitive,
                isWholeWord: IsWholeWord,
                suppressWrite: suppressWrite);
            return sci_clone;
        }
        #endregion

        #endregion

        #region Statics
        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpSearchCriteriaItemId")]
        public override int Id { get; set; }

        [Column("MpSearchCriteriaItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpTagId")]
        public int QueryTagId { get; set; } = 0;

        public string Options { get; set; }

        public string MatchValue { get; set; } = string.Empty;

        [Column("e_MpQueryType")]
        public string QueryTypeName { get; set; } = MpQueryType.Advanced.ToString();

        public int IsCaseSensitiveValue { get; set; }
        public int IsWholeWordValue { get; set; }

        [Column("e_MpLogicalQueryType")]
        public string JoinTypeName { get; set; } = DEFAULT_QUERY_JOIN_TYPE.ToString();

        public int SortOrderIdx { get; set; } = 0;

        #endregion

        #region Properties

        [Ignore]
        public Guid SearchCriteriaItemGuid {
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
        public bool IsCaseSensitive {
            get => IsCaseSensitiveValue == 1;
            set => IsCaseSensitiveValue = value ? 1 : 0;
        }
        [Ignore]
        public bool IsWholeWord {
            get => IsWholeWordValue == 1;
            set => IsWholeWordValue = value ? 1 : 0;
        }

        [Ignore]
        public MpLogicalQueryType JoinType {
            get => JoinTypeName.ToEnum<MpLogicalQueryType>();
            set => JoinTypeName = value.ToString();
        }
        [Ignore]
        public MpQueryType QueryType {
            get => QueryTypeName.ToEnum<MpQueryType>();
            set => QueryTypeName = value.ToString();
        }

        #endregion

        #region Statics

        public static MpContentQueryBitFlags DefaultSimpleFilters =>
            MpContentQueryBitFlags.TextType |
            MpContentQueryBitFlags.ImageType |
            MpContentQueryBitFlags.FileType |
            MpContentQueryBitFlags.Title |
            MpContentQueryBitFlags.Content |
            MpContentQueryBitFlags.Url |
            MpContentQueryBitFlags.AppPath |
            MpContentQueryBitFlags.AppName |
            MpContentQueryBitFlags.Annotations;

        public static async Task<MpSearchCriteriaItem> CreateAsync(
            string guid = "",
            int tagId = 0,
            int sortOrderIdx = -1,
            MpLogicalQueryType joinType = DEFAULT_QUERY_JOIN_TYPE,
            MpQueryType queryType = MpQueryType.None,
            string options = "",
            string matchValue = "",
            bool isCaseSensitive = false,
            bool isWholeWord = false,
            bool suppressWrite = false) {
            if (tagId < 0 && !suppressWrite) {
                throw new Exception("Must provide tag id");
            }
            if (queryType == MpQueryType.None) {
                throw new Exception("Must have query type");
            }
            if (sortOrderIdx < 0 && queryType == MpQueryType.Advanced) {
                // NOTE simple is always at the top and should have -1 sort since its 
                // not managed in adv collection
                sortOrderIdx = await MpDataModelProvider.GetCriteriaItemCountByTagId(tagId);
            }
            var sci = new MpSearchCriteriaItem() {
                SearchCriteriaItemGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                QueryTagId = tagId,
                QueryType = queryType,
                SortOrderIdx = sortOrderIdx,
                JoinType = joinType,
                Options = options,
                MatchValue = matchValue,
                IsCaseSensitive = isCaseSensitive,
                IsWholeWord = isWholeWord
            };

            if (!suppressWrite) {
                await sci.WriteToDatabaseAsync();
            }
            return sci;
        }
        #endregion

        public MpSearchCriteriaItem() : base() { }

        public override Task WriteToDatabaseAsync() {
            if (string.IsNullOrEmpty(Guid)) {
                // handle save for pending query
                Guid = System.Guid.NewGuid().ToString();
            }
            return base.WriteToDatabaseAsync();
        }
        public override async Task DeleteFromDatabaseAsync() {
            List<Task> deleteTasks = new List<Task>();

            deleteTasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(deleteTasks);
        }

    }
}
