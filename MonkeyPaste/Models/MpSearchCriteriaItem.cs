using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Table("MpSearchCriteriaItem")]
    public class MpSearchCriteriaItem :
        MpDbModelBase,
        MpIQueryInfo,
        MpIIsValueEqual<MpSearchCriteriaItem>,
        MpIClonableDbModel<MpSearchCriteriaItem> {
        #region Constants

        public const MpLogicalQueryType DEFAULT_QUERY_JOIN_TYPE = MpLogicalQueryType.And;

        #endregion

        #region Interfaces

        #region MpIQueryInfo Implementation
        public static async Task<MpIQueryInfo> CreateQueryCriteriaAsync(int query_tag_id, bool desc, MpContentSortType sort) {
            var scil = await MpDataModelProvider.GetCriteriaItemsByTagIdAsync(query_tag_id);
            scil = scil.OrderBy(x => x.SortOrderIdx).ToList();
            foreach (var (sci, idx) in scil.WithIndex()) {
                sci._isDescending = desc;
                sci._sortType = sort;
                sci._next = idx < scil.Count - 1 ? scil[idx + 1] : null;
            }
            return scil.FirstOrDefault();
        }
        void MpIQueryInfo.SetNext(MonkeyPaste.MpIQueryInfo next) {
            _next = next;
        }
        private MpIQueryInfo _next;
        MpIQueryInfo MpIQueryInfo.Next =>
            _next;
        private bool _isDescending;
        bool MpIQueryInfo.IsDescending =>
            _isDescending;

        private MpContentSortType _sortType;
        MpContentSortType MpIQueryInfo.SortType =>
            _sortType;

        MpContentQueryBitFlags MpIQueryInfo.QueryFlags =>
            (MpContentQueryBitFlags)QueryFlagsValue;

        MpQueryType MpIQueryInfo.QueryType =>
            QueryType;
        MpLogicalQueryType MpIQueryInfo.JoinType =>
            JoinType;
        int MpIQueryInfo.TagId =>
            MpTag.AllTagId;
        int MpIQueryInfo.SortOrderIdx =>
            SortOrderIdx;
        string MpITextMatchInfo.MatchValue => MatchValue;
        bool MpITextMatchInfo.CaseSensitive =>
            IsCaseSensitive;
        bool MpITextMatchInfo.WholeWord =>
            IsWholeWord;
        bool MpITextMatchInfo.UseRegex =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.Regex);

        #endregion

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

        public long QueryFlagsValue { get; set; }

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
                sortOrderIdx = await MpDataModelProvider.GetCriteriaItemCountByTagIdAsync(tagId);
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

        public async Task WriteToDatabaseAsync(bool force_write = false) {
            // NOTE forced write is to get query flags for readonly tags
            if (string.IsNullOrEmpty(Guid)) {
                // handle save for pending query
                Guid = System.Guid.NewGuid().ToString();
            }
            MpDebug.Assert(QueryTagId > 0, $"Unlinked search criteria writing");

            if (QueryTagId <= MpTag.MAX_READ_ONLY_TAG_ID &&
                !force_write &&
                Id > 0
                ) {
                // prevent altering recent, and format type query criterias
                MpConsole.WriteLine($"Ignored writing read-only search criteria for tag id: {QueryTagId}");
                return;
            }
            await base.WriteToDatabaseAsync();
        }
        public override async Task WriteToDatabaseAsync() {
            await WriteToDatabaseAsync(false);
        }
        public override async Task DeleteFromDatabaseAsync() {
            if (QueryTagId <= MpTag.MAX_READ_ONLY_TAG_ID) {
                // prevent altering recent, and format type query criterias
                MpConsole.WriteLine($"Shouldn't happen");
                return;
            }
            List<Task> deleteTasks = new List<Task>();

            deleteTasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(deleteTasks);
        }

    }
}
