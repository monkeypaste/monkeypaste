using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpUserAccountState {
        Connected,
        Disconnected,
        Unregistered
    }

    interface MpIAccountTools {
        string GetStoreSubscriptionUrl(MpUserAccountType uat, bool isMonthly);
        Task<bool> RefreshAddOnInfoAsync();
        Task<MpSubscriptionFormat> GetStoreUserLicenseInfoAsync();
        Task<bool?> PurchaseSubscriptionAsync(MpUserAccountType uat, bool isMonthly);
    }

    public partial class MpAvAccountTools : MpIAccountTools {
        #region Private Variables
        private MpContentCapInfo _lastCapInfo = new();
        private int _lastContentCount = 0;
        private int _lastTrashCount = 0;

        #endregion

        #region Constants
        public const string EMPTY_RATE_TEXT = "???";
        public const int MIN_PASSWORD_LENGTH = 6;

        const MpUserAccountType TEST_ACCOUNT_TYPE = MpUserAccountType.Free;

        const int MAX_FREE_CLIP_COUNT = 5;
        const int MAX_STANDARD_CLIP_COUNT = 1000;
        const int MAX_UNLIMITED_CLIP_COUNT = -1;

        const int MAX_FREE_TRASH_COUNT = 20;
        const int MAX_STANDARD_TRASH_COUNT = -1;
        const int MAX_UNLIMITED_TRASH_COUNT = -1;

        const int DEFAULT_UNLIMITED_TRIAL_DAY_COUNT = 7;

        #endregion

        #region Statics
        private static MpAvAccountTools _instance;
        public static MpAvAccountTools Instance => _instance ?? (_instance = new MpAvAccountTools());
        #endregion

        #region Interfaces

        #region MpIAccountTools Implementation
        public int LastContentCount =>
            _lastContentCount;

        public int GetAccountPriority(MpUserAccountType uat, bool isMonthly) {
            if (uat == MpUserAccountType.Free) {
                return 0;
            }
            if (uat == MpUserAccountType.Standard) {
                return isMonthly ? 1 : 2;
            }
            return isMonthly ? 3 : 4;
        }

        public int GetContentCapacity(MonkeyPaste.MpUserAccountType acctType) {
            switch (acctType) {
                default:
                case MpUserAccountType.Free:
                    return MAX_FREE_CLIP_COUNT;
                case MpUserAccountType.Standard:
                    return MAX_STANDARD_CLIP_COUNT;
                case MpUserAccountType.Unlimited:
                    return MAX_UNLIMITED_CLIP_COUNT;
            }
        }

        public int GetTrashCapacity(MonkeyPaste.MpUserAccountType acctType) {
            switch (acctType) {
                default:
                case MpUserAccountType.Free:
                    return MAX_FREE_TRASH_COUNT;
                case MpUserAccountType.Standard:
                    return MAX_STANDARD_TRASH_COUNT;
                case MpUserAccountType.Unlimited:
                    return MAX_UNLIMITED_TRASH_COUNT;
            }
        }

        public string GetAccountRate(MpUserAccountType acctType, bool isMonthly) {
            if (acctType == MpUserAccountType.Free) {
                return "$0.00";
            }
            if (AccountTypePriceLookup.TryGetValue((acctType, isMonthly), out string rate)) {
                return rate;
            }
            // NOTE this should only happen when not offline
            return EMPTY_RATE_TEXT;
        }

        public async Task<MpContentCapInfo> RefreshCapInfoAsync(MpUserAccountType cur_uat, MpAccountCapCheckType source) {
            int prev_content_count = _lastContentCount;
            int prev_trash_count = _lastTrashCount;

            int new_content_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.AllTagId);
            int new_trash_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.TrashTagId);
            bool has_changed = new_content_count != _lastContentCount || new_trash_count != _lastTrashCount;
            if (has_changed) {
                _lastContentCount = new_content_count;
                _lastTrashCount = new_trash_count;
                MpMessenger.SendGlobal(MpMessageType.AccountInfoChanged);
            }

            int content_cap = GetContentCapacity(cur_uat);
            if (content_cap < 0) {
                // unlimited, no need to check trash
                _lastCapInfo = new MpContentCapInfo();
                return _lastCapInfo;
            }

            // ADD LOCK /////////////////////////////////////////////////////

            int favorite_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(
                tid: MpTag.FavoritesTagId,
                ignore_descendants: false);
            IsContentAddPausedByAccount = favorite_count >= content_cap;

            // TO TRASH /////////////////////////////////////////////////////

            //int totalCount = source == MpAccountCapCheckType.Add ? prev_content_count : new_content_count;

            int to_trash_ciid = 0;
            int max_diff = source == MpAccountCapCheckType.Add ? 0 : -1;
            if (new_content_count - content_cap > max_diff) {
                int content_offset = MpAvPrefViewModel.Instance.ContentCountAtAccountDowngrade - content_cap;
                if (content_offset >= new_content_count ||
                    content_offset < 0) {
                    content_offset = 0;
                }
                to_trash_ciid = await GetNextToTrashAsync(content_offset);
            }

            // TO REMOVE //////////////////////////////////////////

            int totalTrash = _lastTrashCount;
            int trash_cap = GetTrashCapacity(cur_uat);
            bool needs_trash_cap_info = totalTrash >= trash_cap;
            int to_remove_ciid = 0;
            if (needs_trash_cap_info) {
                to_remove_ciid = await GetNextToRemoveAsync();
            }

            _lastCapInfo = new MpContentCapInfo() {
                ToBeTrashed_ciid = to_trash_ciid,
                ToBeRemoved_ciid = to_remove_ciid,
            };
            return _lastCapInfo;
        }

        #endregion

        #endregion

        #region Properties

        #region State


        protected Dictionary<string, (MpUserAccountType, bool)> AccountTypeAddOnStoreIdLookup { get; } =
            new Dictionary<string, (MpUserAccountType, bool)>() {
#if DEBUG
                {"9N0M0CF894CV", (MpUserAccountType.Standard, true) },
                {"9NTBHV933F76", (MpUserAccountType.Standard, false) },

                {"9P06QJ00F7Q8", (MpUserAccountType.Unlimited, true) },
                {"9N2BVBP6MSP6", (MpUserAccountType.Unlimited, false) }
#else
                {"9PP3W114BHL5", (MpUserAccountType.Standard, true) },
                {"9N41GXV5HQQ2", (MpUserAccountType.Standard, false) },

                {"9PGVZ60KMDQ7", (MpUserAccountType.Unlimited, true) },
                {"9NN60Z6FX02H", (MpUserAccountType.Unlimited, false) }

#endif
            };
        //protected Dictionary<(MpUserAccountType, bool), string> AccountTypePriceLookup { get; } = new();
        //protected Dictionary<(MpUserAccountType, bool), int> AccountTypeTrialAvailabilityLookup { get; } = new();
        protected Dictionary<(MpUserAccountType, bool), string> AccountTypePriceLookup { get; } = new Dictionary<(MpUserAccountType, bool), string>() {
        {(MpUserAccountType.Free,true),"$0.00" },
        {(MpUserAccountType.Free,false),"$0.00" },
        {(MpUserAccountType.Standard,true),"$0.99" },
        {(MpUserAccountType.Standard,false),"$9.99" },
        {(MpUserAccountType.Unlimited,true),"$2.99" },
        {(MpUserAccountType.Unlimited,false),"$29.99" }
        };

        protected Dictionary<(MpUserAccountType, bool), int> AccountTypeTrialAvailabilityLookup { get; } = new Dictionary<(MpUserAccountType, bool), int>() {
        {(MpUserAccountType.Unlimited,true),DEFAULT_UNLIMITED_TRIAL_DAY_COUNT },
        {(MpUserAccountType.Unlimited,false),DEFAULT_UNLIMITED_TRIAL_DAY_COUNT }
        };

        public bool IsContentAddPausedByAccount { get; private set; }
        public MpContentCapInfo LastCapInfo => _lastCapInfo;
        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        private MpAvAccountTools() { }

        public int GetSubscriptionTrialLength(MpUserAccountType uat, bool isMonthly) {
            if (AccountTypeTrialAvailabilityLookup.TryGetValue((uat, isMonthly), out int dayCount)) {
                return dayCount;
            }
            return 0;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        #region Cap
        private async Task<int> GetNextToTrashAsync(int offset) {
            // Problem: if user gets trial and accumulates 100 items then trial ends.
            // They will end up always having a working set of 100 items.
            // Solution: Move query offset to 100 - cap so:
            // 1. Its ok to say you won't lose data after trial and they don't have to delete everything
            // 2. The benefit of accumulating 100 items is gone since criteria is based off last 5

            string to_trash_query = @"
select pk_MpCopyItemId 
from MpCopyItem 
where 
pk_MpCopyItemId not in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=? or fk_MpTagId=?)
order by LastCapRelatedDateTime limit 1 offset ?";

            var to_trash_result = await MpDb.QueryScalarsAsync<int>(to_trash_query, MpTag.TrashTagId, MpTag.FavoritesTagId, offset);
            if (to_trash_result.Any()) {
                return to_trash_result.First();
            }
            // no non-favorited items to trash or next to trash has no response,

            // requery allowing favorites
            to_trash_result = await MpDb.QueryScalarsAsync<int>(to_trash_query, MpTag.TrashTagId, 0, offset);
            if (to_trash_result.Any()) {
                return to_trash_result.First();
            }
            return 0;
        }

        private async Task<int> GetNextToRemoveAsync() {
            // select oldest and next oldest linked to trash(5)
            string to_remove_query = @"
select pk_MpCopyItemId 
from MpCopyItem 
where 
pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=?)
order by LastCapRelatedDateTime limit 1
";
            List<int> to_remove_result = await MpDb.QueryScalarsAsync<int>(to_remove_query, MpTag.TrashTagId);
            return to_remove_result.FirstOrDefault();
        }

        #endregion


        #endregion

    }
}
