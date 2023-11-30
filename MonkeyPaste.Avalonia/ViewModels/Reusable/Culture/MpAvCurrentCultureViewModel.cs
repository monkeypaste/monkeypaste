using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCurrentCultureViewModel : MpAvViewModelBase {
        #region Private Variable
        private IEnumerable<Type> _localizableTypes = new Type[] {
            typeof(UiStrings),
            typeof(EnumUiStrings)
        };
        #endregion

        #region Constants
        public const string DEFAULT_CULTURE_NAME = "en-US";
        #endregion

        #region Statics
        private static MpAvCurrentCultureViewModel _instance;
        public static MpAvCurrentCultureViewModel Instance => _instance ?? (_instance = new MpAvCurrentCultureViewModel());

        public static bool IsDefaultCulture(CultureInfo c) {
            return c == null || c.Name == DEFAULT_CULTURE_NAME;
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        private Dictionary<string, string> _langLookup;
        public Dictionary<string, string> LangLookup {
            get {
                if (_langLookup == null) {
                    var cl = GetAvailableCultures();
                    _langLookup = cl.ToDictionary(x => x.Name, x => GetCultureDisplayValue(x));
                }
                return _langLookup;
            }
        }
        #endregion

        #region State

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

        private IEnumerable<CultureInfo> GetAvailableCultures() {
            string uistr_dir = Path.Combine(
                Path.GetDirectoryName(typeof(MpAvEnumToUiStringResourceConverter).Assembly.Location),
                "Resources",
                "Localization",
                "UiStrings");

            List<CultureInfo> cl = new List<CultureInfo>();
            var fil = new DirectoryInfo(uistr_dir).EnumerateFiles();
            foreach (var fi in fil) {
                var fn_parts = fi.Name.SplitNoEmpty(".");
                if (fn_parts.Length == 2) {
                    // default
                    cl.Add(new CultureInfo(DEFAULT_CULTURE_NAME));
                } else if (fn_parts.Length == 3) {
                    cl.Add(new CultureInfo(fn_parts[1]));
                } else {
                    MpDebug.Break("UiStrings error, weird file");
                    continue;
                }
            }
            return cl;
        }


        private string GetCultureDisplayValue(CultureInfo culture) {
            return $"{culture.EnglishName} - {culture.NativeName}";
        }

        private string ResolveMissingCulture(string culture_code) {
            CultureInfo closest_info = new CultureInfo(DEFAULT_CULTURE_NAME);
            foreach (var ac in GetAvailableCultures()) {
                if (GetSelfOrAncestorByCode(ac, culture_code) is CultureInfo match) {
                    closest_info = match;
                    break;
                }
            }
            return closest_info.Name;
        }
        private CultureInfo GetSelfOrAncestorByCode(CultureInfo ci, string culture_code) {
            if (ci == null || string.IsNullOrEmpty(culture_code)) {
                return null;
            }
            if (ci.Name == culture_code) {
                return ci;
            }
            return GetSelfOrAncestorByCode(ci.Parent, culture_code);
        }

        private void UpdateCultureDependencies() {
            if (!Mp.Services.StartupState.IsReady) {
                // shouldn't need to update
                return;
            }
            MpAvTagTrayViewModel.Instance
                .Items
                .Where(x => x.IsTagReadOnly)
                .ForEach(x => x.OnPropertyChanged(nameof(x.TagName)));

            MpAvTriggerCollectionViewModel.Instance
                .Items
                .Where(x => x.IsDefaultAction)
                .ForEach(x => x.OnPropertyChanged(nameof(x.Label)));


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
                    culture_code = ResolveMissingCulture(culture_code);
                }
                CurrentCulture = new CultureInfo(culture_code);
                MpConsole.WriteLine($"Culture set to: {CurrentCulture}");

                UiStrings.Culture = CurrentCulture;
                EnumUiStrings.Culture = CurrentCulture;

                bool needs_restart1 = MpAvEnumToUiStringResourceConverter.CheckEnumUiStrings();
                bool needs_restart2 = MpAvEditorUiStringBuilder.CheckJsUiStrings();

                // NOTE setting pref AFTER any changes so content re-init has updated data
                MpAvPrefViewModel.Instance.IsTextRightToLeft = CultureInfo.GetCultureInfo(culture_code).TextInfo.IsRightToLeft;
                string last_code = MpAvPrefViewModel.Instance.CurrentCultureCode;
                MpAvPrefViewModel.Instance.CurrentCultureCode = culture_code;

                if (needs_restart1 || needs_restart2) {
                    // NOTE only triggered in debug
                    string change_suffix = string.Empty;
                    if (needs_restart1) {
                        change_suffix += " Enums ";
                    }
                    if (needs_restart2) {
                        change_suffix += " Editor ";
                    }
                    Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.EditorResourceUpdate, $"Culture Data changed: {change_suffix}");
                } else if (culture_code != last_code) {
                    UpdateCultureDependencies();
                }
            });
        #endregion
    }
}
