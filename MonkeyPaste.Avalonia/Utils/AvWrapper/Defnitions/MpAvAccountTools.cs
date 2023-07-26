
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpAccountCapCheckType {
        None = 0,
        Add,
        Link,
        RestoreBlock,
        AddBlock,
        Remove,
        Init,
        AccountTypeDowngraded,
        AccountTypeUpgraded,
    }
    public class MpAvAccountTools : MpIAccountTools {
        #region Private Variables
        private MpContentCapInfo _lastCapInfo = new();
        private int _lastContentCount = 0;
        private int _lastTrashCount = 0;

        #endregion

        #region Constants
        const MpUserAccountType TEST_ACCOUNT_TYPE = MpUserAccountType.Unlimited;
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIAccountTools Implementation
        public string AccountStateInfo {
            get {
                if (CurrentAccountType == MpUserAccountType.Unlimited) {
                    return $"{CurrentAccountType}-(Total {_lastContentCount})";
                }
                return $"{CurrentAccountType} - ({_lastContentCount} total / {GetContentCapacity(CurrentAccountType)} capacity {(GetContentCapacity(CurrentAccountType) - _lastContentCount)} remaining)";
            }
        }
        public void SetAccountType(MpUserAccountType newType) {
            // NOTE this maybe a good all around interface method, not sure though
            bool changed = CurrentAccountType != newType;
            if (changed) {
                bool is_upgrade = (int)newType > (int)CurrentAccountType;
                CurrentAccountType = newType;
                MpMessenger.SendGlobal(is_upgrade ? MpMessageType.AccountUpgrade : MpMessageType.AccountDowngrade);
            }
        }
        public MpUserAccountType CurrentAccountType { get; private set; } = TEST_ACCOUNT_TYPE;

        public int GetContentCapacity(MonkeyPaste.MpUserAccountType acctType) {
            switch (acctType) {
                case MpUserAccountType.Free:
                    return 5;
                case MpUserAccountType.Standard:
                    return 100;
                case MpUserAccountType.Trial:
                case MpUserAccountType.Unlimited:
                case MpUserAccountType.Admin:
                    return -1;
                default:
                case MpUserAccountType.None:
                    return 0;
            }
        }

        public int GetTrashCapacity(MonkeyPaste.MpUserAccountType acctType) {
            switch (acctType) {
                case MpUserAccountType.Free:
                    return 20;
                case MpUserAccountType.Standard:
                case MpUserAccountType.Trial:
                case MpUserAccountType.Unlimited:
                case MpUserAccountType.Admin:
                    return -1;
                default:
                case MpUserAccountType.None:
                    return 0;
            }
        }

        public async Task<MpContentCapInfo> RefreshCapInfoAsync() {
            int new_content_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.AllTagId);
            int new_trash_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.TrashTagId);
            bool has_changed = new_content_count != _lastContentCount || new_trash_count != _lastTrashCount;
            _lastContentCount = new_content_count;
            _lastTrashCount = new_trash_count;
            if (has_changed) {
                MpMessenger.SendGlobal(MpMessageType.AccountInfoChanged);
            }

            int content_cap = GetContentCapacity(CurrentAccountType);
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

            int totalCount = _lastContentCount;

            // examples (content cap)
            // content cap 5, actual 4 (needs next to trash)
            // content cap 5 actual 5 (needs both)
            // content cap 5 actual 3 (none)
            int cur_content_diff = content_cap - totalCount;
            bool needs_content_cap_info = cur_content_diff < 1;
            bool is_1_before_content_cap = cur_content_diff == 0;
            List<int> to_trash_result = Enumerable.Repeat(0, 2).ToList();
            if (needs_content_cap_info) {
                to_trash_result = await GetNowAndNextToTrashAsync();
                if (is_1_before_content_cap) {
                    // when not at content cap none goes to trash yet so leave to trash blank but shift to report most recent 
                    to_trash_result[1] = to_trash_result[0];
                    to_trash_result[0] = 0;
                }
            }

            // TO REMOVE //////////////////////////////////////////

            int totalTrash = _lastTrashCount;

            // examples (trash cap)
            // trash cap 100 actual 99 (needs next)
            // trash cap 100 actual 100 (both)
            // trash cap 100, actual 4 (none)
            int trash_cap = GetTrashCapacity(CurrentAccountType);
            int cur_trash_diff = trash_cap - totalTrash;
            bool needs_trash_cap_info = cur_trash_diff < 1;
            bool is_1_before_trash_cap = cur_trash_diff == 0;
            List<int> to_remove_result = Enumerable.Repeat(0, 2).ToList();
            if (needs_trash_cap_info) {
                to_remove_result = await GetNowAndNextToRemoveAsync();
                if (is_1_before_trash_cap) {
                    // when not at content cap none goes to trash yet so leave to trash blank but shift to report most recent 
                    to_remove_result[1] = to_remove_result[0];
                    to_remove_result[0] = 0;
                }
            }

            _lastCapInfo = new MpContentCapInfo() {
                ToBeTrashed_ciid = to_trash_result[0],
                NextToBeTrashed_ciid = to_trash_result[1],
                ToBeRemoved_ciid = to_remove_result[0],
                NextToBeRemoved_ciid = to_remove_result[1]
            };
            return _lastCapInfo;
        }

        #endregion

        #endregion

        #region Properties
        public bool IsContentAddPausedByAccount { get; private set; }
        public MpContentCapInfo LastCapInfo => _lastCapInfo;
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private async Task<List<int>> GetNowAndNextToTrashAsync() {
            // select oldest and next oldest created item not in trash(5) and not in favorites(3) [fallbacks] and not 
            string to_trash_query = @"
select pk_MpCopyItemId 
from MpCopyItem 
where 
pk_MpCopyItemId not in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=? or fk_MpTagId=?) and 
pk_MpCopyItemId != ? 
order by LastCapRelatedDateTime limit 2
";

            var to_trash_result = await MpDb.QueryScalarsAsync<int>(to_trash_query, MpTag.TrashTagId, MpTag.FavoritesTagId, 0);
            if (to_trash_result.Count < 2) {
                // no non-favorited items to trash or next to trash has no result,

                // requery allowing favorites
                int to_trash_ciid = to_trash_result.Count == 1 ? to_trash_result[0] : 0;
                var to_trash_fallback_result = await MpDb.QueryScalarsAsync<int>(to_trash_query, MpTag.TrashTagId, 0, to_trash_ciid);
                //MpDebug.Assert(to_trash_fallback_result.Count == 2, $"Account cap fallback error, should always have 2 results if reached this point");
                if (to_trash_result.Count == 0) {
                    // both to trash and next are favorites
                    to_trash_result = to_trash_fallback_result;
                } else {
                    // next to trash is a favorite
                    to_trash_result.Add(to_trash_fallback_result[0]);
                }
            }
            int to_add = 2 - to_trash_result.Count;
            while (to_add > 0) {
                to_trash_result.Add(0);
            }
            return to_trash_result;
        }

        private async Task<List<int>> GetNowAndNextToRemoveAsync() {
            // select oldest and next oldest linked to trash(5)
            string to_remove_query = @"
select pk_MpCopyItemId 
from MpCopyItem 
where 
pk_MpCopyItemId in (select fk_MpCopyItemId from MpCopyItemTag where fk_MpTagId=?)
order by LastCapRelatedDateTime limit 2
";
            List<int> to_remove_result = await MpDb.QueryScalarsAsync<int>(to_remove_query, MpTag.TrashTagId);

            int to_add = 2 - to_remove_result.Count;
            while (to_add > 0) {
                to_remove_result.Add(0);
            }
            return to_remove_result;
        }


        #endregion

        #region Commands
        #endregion
    }
}
