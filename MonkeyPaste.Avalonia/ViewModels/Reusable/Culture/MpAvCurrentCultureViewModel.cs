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
using System.Threading.Tasks;
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
        #endregion

        #region Statics
        private static MpAvCurrentCultureViewModel _instance;
        public static MpAvCurrentCultureViewModel Instance => _instance ?? (_instance = new MpAvCurrentCultureViewModel());

        public static bool SetAllCultures(CultureInfo ci) {
            UiStrings.Culture = ci;
            EnumUiStrings.Culture = ci;

            bool needs_restart1 = MpAvEnumToUiStringResourceConverter.CheckEnumUiStrings();
            bool needs_restart2 = MpAvEditorUiStringBuilder.CheckJsUiStrings();
            return needs_restart1 || needs_restart2;
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
                    var cl = MpAvLocalizationHelpers.GetAvailableCultures(UiStringDir);
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
                Path.GetDirectoryName(typeof(MpAvEnumToUiStringResourceConverter).Assembly.Location),
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
            return $"{culture.EnglishName} - {culture.NativeName}";
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
                    culture_code = MpAvLocalizationHelpers.ResolveMissingCulture(culture_code, UiStringDir);
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
