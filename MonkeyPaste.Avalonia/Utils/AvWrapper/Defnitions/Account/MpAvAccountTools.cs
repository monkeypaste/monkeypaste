using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpUserAccountState {
        Connected,
        Disconnected,
        Unregistered
    }

    public partial class MpAvAccountTools {
        #region Private Variables
        private MpContentCapInfo _lastCapInfo = new();
        private int _lastContentCount = 0;
        private int _lastTrashCount = 0;

        #endregion

        #region Constants
        public const string EMPTY_RATE_TEXT = "???";
        public const int MIN_PASSWORD_LENGTH = 6;

        const string SUCCESS_PREFIX = "[SUCCESS]";
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
        //        public void Init() {
        //#if DEBUG
        //            SetAccountType(MpAvPrefViewModel.Instance.TestAccountType);
        //#endif
        //        }


        public string AccountStateInfo {
            get {
                if (MpAvAccountViewModel.Instance == null) {
                    return string.Empty;
                }
                var uat = MpAvAccountViewModel.Instance.AccountType;
                if (uat == MpUserAccountType.Unlimited) {
                    return $"{uat}-(Total {_lastContentCount})";
                }
                return $"{uat} - ({_lastContentCount} total / {GetContentCapacity(uat)} capacity {(GetContentCapacity(uat) - _lastContentCount)} remaining)";
            }
        }

        public MpUserAccountType CurrentAccountType {
            get => MpAvPrefViewModel.Instance.AccountType;
            private set {
                if (CurrentAccountType != value) {
                    MpAvPrefViewModel.Instance.AccountType = value;
                }
            }
        }

        public int GetContentCapacity(MonkeyPaste.MpUserAccountType acctType) {
            switch (acctType) {
                case MpUserAccountType.Free:
                    return MAX_FREE_CLIP_COUNT;
                case MpUserAccountType.Standard:
                    return MAX_STANDARD_CLIP_COUNT;
                case MpUserAccountType.Unlimited:
                    return MAX_UNLIMITED_CLIP_COUNT;
                default:
                case MpUserAccountType.None:
                    return 0;
            }
        }

        public int GetTrashCapacity(MonkeyPaste.MpUserAccountType acctType) {
            switch (acctType) {
                case MpUserAccountType.Free:
                    return MAX_FREE_TRASH_COUNT;
                case MpUserAccountType.Standard:
                    return MAX_STANDARD_TRASH_COUNT;
                case MpUserAccountType.Unlimited:
                    return MAX_UNLIMITED_TRASH_COUNT;
                default:
                case MpUserAccountType.None:
                    return 0;
            }
        }

        public string GetAccountRate(MpUserAccountType acctType, bool isMonthly) {
            if (acctType == MpUserAccountType.None ||
                acctType == MpUserAccountType.Free) {
                return string.Empty;
            }
            if (AccountTypePriceLookup.TryGetValue((acctType, isMonthly), out string rate)) {
                return rate;
            }
            // NOTE this should only happen when not offline
            return EMPTY_RATE_TEXT;
        }

        public async Task<MpContentCapInfo> RefreshCapInfoAsync(MpUserAccountType cur_uat) {
            int new_content_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.AllTagId);
            int new_trash_count = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.TrashTagId);
            bool has_changed = new_content_count != _lastContentCount || new_trash_count != _lastTrashCount;
            _lastContentCount = new_content_count;
            _lastTrashCount = new_trash_count;
            if (has_changed) {
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
            int trash_cap = GetTrashCapacity(cur_uat);
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

        #region State
        Dictionary<(MpUserAccountType, bool), string> AccountTypePriceLookup { get; } = new Dictionary<(MpUserAccountType, bool), string>() {
            //{(MpUserAccountType.Free,true),"$0.00" },
            //{(MpUserAccountType.Free,false),"$0.00" },
            //{(MpUserAccountType.Standard,true),"$0.99" },
            //{(MpUserAccountType.Standard,false),"$9.99" },
            //{(MpUserAccountType.Unlimited,true),"$2.99" },
            //{(MpUserAccountType.Unlimited,false),"$29.99" }
        };

        Dictionary<(MpUserAccountType, bool), int> AccountTypeTrialAvailabilityLookup { get; } = new Dictionary<(MpUserAccountType, bool), int>() {
            //{(MpUserAccountType.Unlimited,true),DEFAULT_UNLIMITED_TRIAL_DAY_COUNT },
            //{(MpUserAccountType.Unlimited,false),DEFAULT_UNLIMITED_TRIAL_DAY_COUNT }
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
        public MpAvAccountTools() { }

        public bool IsValidEmail(string email) {
            return MpRegEx.RegExLookup[MpRegExType.ExactEmail].IsMatch(email);
        }

        public bool IsValidPassword(string str) {
            return str != null && str.Length >= MIN_PASSWORD_LENGTH;
        }

        public async Task<MpSubscriptionFormat> GetUserSubscriptionAsync() {
            var acct = await GetStoreUserLicenseInfoAsync();
            return acct;
        }
        public int GetSubscriptionTrialLength(MpUserAccountType uat, bool isMonthly) {
            if (AccountTypeTrialAvailabilityLookup.TryGetValue((uat, isMonthly), out int dayCount)) {
                return dayCount;
            }
            return 0;
        }

        public async Task<bool?> PurchaseSubscriptionAsync(MpUserAccountType uat, bool isMonthly) {
            if (uat == MpUserAccountType.None ||
                uat == MpUserAccountType.Free) {
                // ignore free or none
                return true;
            }
            var result = await PerformPlatformPurchaseAsync(uat, isMonthly);
            return result;
        }



        public async Task<bool> RegisterUserAsync() {
            string register_url = $"https://www.monkeypaste.com/accounts/register.php";
            string response = await PostDataToUrlAsync(
                url: register_url,
                keyValuePairs: new Dictionary<string, string>() {
                    {"username", MpAvPrefViewModel.Instance.AccountUsername },
                    {"email", MpAvPrefViewModel.Instance.AccountEmail },
                    {"password", MpAvPrefViewModel.Instance.AccountPassword },
                    {"password2", MpAvPrefViewModel.Instance.AccountPassword2 },
                    {"device_guid", MpDefaultDataModelTools.ThisUserDeviceGuid },
                    {"sub_type", MpAvPrefViewModel.Instance.AccountType.ToString() },
                    {"monthly", MpAvPrefViewModel.Instance.AccountBillingCycleType == MpBillingCycleType.Monthly ? "1":"0" },
                    {"expires_utc_dt", MpAvPrefViewModel.Instance.AccountNextPaymentDateTime.ToString() },
                    {"detail1", MpAvPrefViewModel.arg1 },
                    {"detail2", MpAvPrefViewModel.arg2 },
                    {"detail3", MpAvPrefViewModel.arg3 },
                });
            bool success = response == SUCCESS_PREFIX;
            if (success) {
                MpConsole.WriteLine($"Registration successful for user '{MpAvPrefViewModel.Instance.AccountEmail}' deviceid '{MpDefaultDataModelTools.ThisUserDeviceGuid}' acct_type '{MpAvPrefViewModel.Instance.AccountType}'");
            }


            return success;
        }

        public async Task<bool> LoginUserAsync() {
            string login_url = $"https://www.monkeypaste.com/accounts/login.php";
            string response = await PostDataToUrlAsync(
                url: login_url,
                keyValuePairs: new Dictionary<string, string>() {
                    {"username", MpAvPrefViewModel.Instance.AccountUsername },
                    {"password", MpAvPrefViewModel.Instance.AccountPassword },
                    {"device_guid", MpDefaultDataModelTools.ThisUserDeviceGuid },
                    {"sub_type", MpAvPrefViewModel.Instance.AccountType.ToString() },
                    {"monthly", MpAvPrefViewModel.Instance.AccountBillingCycleType == MpBillingCycleType.Monthly ? "1":"0" },
                    {"expires_utc_dt", MpAvPrefViewModel.Instance.AccountNextPaymentDateTime.ToString() },
                });

            if (response.StartsWith(SUCCESS_PREFIX) &&
                response.Replace(SUCCESS_PREFIX, string.Empty) is string updateText &&
                updateText.Split(",") is string[] updateParts) {
                // login and server updated
                MpDebug.Assert(updateParts.Length == 2, $"Login reponse error. Should be 'type,expire_dt' but is '{updateText}'");
                return true;
            }
            return false;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private async Task<string> PostDataToUrlAsync(string url, Dictionary<string, string> keyValuePairs) {
            // from https://stackoverflow.com/a/62640006/105028
            using (HttpClient httpClient = new HttpClient())
            using (MultipartFormDataContent formDataContent = new MultipartFormDataContent()) {
                foreach (var keyValuePair in keyValuePairs) {
                    formDataContent.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }

                // Post Request And Wait For The Response.
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, formDataContent);

                // Check If Successful Or Not.
                if (httpResponseMessage.IsSuccessStatusCode) {
                    // Return Byte Array To The Caller.
                    return await httpResponseMessage.Content.ReadAsStringAsync();
                } else {
                    // Throw Some Sort of Exception?
                    return string.Empty;
                }
            }
        }

        #region Cap
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
                // no non-favorited items to trash or next to trash has no response,

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
                to_add--;
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
                to_add--;
            }
            return to_remove_result;
        }

        #endregion


        #endregion

    }
}
