using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCurrentCultureViewModel : MpAvViewModelBase, MpICultureInfo {
        #region Private Variable
        private IEnumerable<Type> _localizableTypes = new Type[] {
            typeof(UiStrings),
            typeof(EnumUiStrings)
        };
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvCurrentCultureViewModel _instance;
        public static MpAvCurrentCultureViewModel Instance => _instance ?? (_instance = new MpAvCurrentCultureViewModel());

        public bool SetAllCultures(CultureInfo ci) {
            var pre_def_titles = new string[] {
                string.Empty,
                UiStrings.ClipTileDefTitleTextPrefix,
                UiStrings.ClipTileDefTitleImagePrefix,
                UiStrings.ClipTileDefTitleFilesPrefix
            };

            UiStrings.Culture = ci;
            EnumUiStrings.Culture = ci;

            var lastOption = R.U.CurrentOption;
            R.U.CurrentOption = R.U.AvailableOptions.FirstOrDefault(x => x.CultureInfo.Name == ci.Name);
            if (R.U.CurrentOption != lastOption && Mp.Services.StartupState.IsReady) {
                RefreshUiAsync(pre_def_titles).FireAndForgetSafeAsync();
            }
            bool needs_restart1 = MpAvEnumUiStringResourceConverter.CheckEnumUiStrings();
            bool needs_restart2 = MpAvEditorUiStringBuilder.CheckJsUiStrings();
            return needs_restart1 || needs_restart2;
        }
        #endregion

        #region Interfaces
        #region MpICultureInfo Implementation
        string MpICultureInfo.CultureCode =>
            CurrentCulture.Name;

        #endregion
        #endregion

        #region Properties

        #region View Models

        private Dictionary<string, string> _langLookup;
        public Dictionary<string, string> LangLookup {
            get {
                if (_langLookup == null) {
                    var cl = MpLocalizationHelpers.FindCulturesInDirectory(UiStringDir);
                    _langLookup = cl.ToDictionary(x => x.Name, x => GetCultureDisplayValue(x));
                }
                return _langLookup;
            }
        }
        #endregion

        #region State

        public string UiStringDir {
            get {
                return Path.Combine(
                Path.GetDirectoryName(typeof(MpAvEnumUiStringResourceConverter).Assembly.Location),
                "Resources",
                "Localization",
                "UiStrings");
            }
        }
        public CultureInfo CurrentCulture { get; private set; }
        #endregion

        #endregion

        #region Constructors
        private MpAvCurrentCultureViewModel() {

        }

        #endregion

        #region Public Methods
        public void Init() {
            string culture_name = MpAvPrefViewModel.Instance.CurrentCultureCode;
            SetCultureCommand.Execute(culture_name);

        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private string GetCultureDisplayValue(CultureInfo culture) {
            if (culture == CultureInfo.InvariantCulture) {
                return new CultureInfo("en-US").NativeName;
            }
            return $"{culture.NativeName}";
        }

        private async Task RefreshUiAsync(string[] pre_def_titles) {
            MpAvTagTrayViewModel.Instance.Items.ForEach(x => x.OnPropertyChanged(nameof(x.TagName)));
            MpAvTriggerCollectionViewModel.Instance.Items.ForEach(x => x.OnPropertyChanged(nameof(x.Label)));
            MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.ForEach(x => x.OnPropertyChanged(nameof(x.Label)));
            MpAvClipTrayViewModel.Instance.UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();

            var post_def_titles = new string[] {
                string.Empty,
                UiStrings.ClipTileDefTitleTextPrefix,
                UiStrings.ClipTileDefTitleImagePrefix,
                UiStrings.ClipTileDefTitleFilesPrefix
            };
            var def_title_regex = pre_def_titles.Select(x => new Regex($"^{x}[0-9]*$")).ToArray();
            foreach (var ctvm in MpAvClipTrayViewModel.Instance.AllActiveItems) {
                var cur_regex = def_title_regex[(int)ctvm.CopyItemType];
                if (!cur_regex.IsMatch(ctvm.CopyItemTitle)) {
                    continue;
                }
                int num_idx = -1;
                for (int i = 0; i < ctvm.CopyItemTitle.Length; i++) {
                    if (ctvm.CopyItemTitle[i] >= '0' && ctvm.CopyItemTitle[i] <= '9') {
                        num_idx = i;
                        break;
                    }
                }
                if (num_idx < 0) {
                    continue;
                }
                ctvm.CopyItemTitle = $"{post_def_titles[(int)ctvm.CopyItemType]}{ctvm.CopyItemTitle.Substring(num_idx)}";
            }

            while (MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }
            await MpAvClipTrayViewModel.Instance.DisposeAndReloadAllCommand.ExecuteAsync();


        }

        #endregion

        #region Commands
        public ICommand SetCultureCommand => new MpCommand<object>(
            (args) => {
                if (args is not string culture_code) {
                    return;
                }
                if (!LangLookup.ContainsKey(culture_code)) {
                    // exact culture not available (can occur on initial startup using system default)
                    // fallback to parent or default
                    culture_code = MpLocalizationHelpers.FindClosestCultureCode(culture_code, UiStringDir);
                }
                CurrentCulture = new CultureInfo(culture_code);
                MpConsole.WriteLine($"Culture set to: {CurrentCulture}");

                bool needs_restart = SetAllCultures(CurrentCulture);

                // NOTE setting pref AFTER any changes so content re-init has updated data
                MpAvPrefViewModel.Instance.IsTextRightToLeft = CultureInfo.GetCultureInfo(culture_code).TextInfo.IsRightToLeft;
                string last_code = MpAvPrefViewModel.Instance.CurrentCultureCode;
                MpAvPrefViewModel.Instance.CurrentCultureCode = culture_code;

                if (needs_restart) {
                    // NOTE only triggered in debug

                    Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.EditorResourceUpdate, $"Culture Data changed");
                }
            });
        #endregion
    }
}
