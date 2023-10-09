using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvShortcutExtensions {

        public static IEnumerable<MpAvShortcutKeyGroupViewModel> ToKeyItems(this IEnumerable<string> keysArray, out string keystring) {
            keystring = string.Join(MpInputConstants.COMBO_SEPARATOR, keysArray);
            return keystring.ToKeyItems();
        }
        public static IEnumerable<MpAvShortcutKeyGroupViewModel> ToKeyItems(this string keyString) {
            var keyItems = new List<MpAvShortcutKeyGroupViewModel>();
            if (string.IsNullOrEmpty(keyString)) {
                return keyItems;
            }

            var combos = keyString.Split(new String[] { MpInputConstants.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            int maxComboIdx = combos.Length - 1;
            for (int comboIdx = 0; comboIdx < combos.Length; comboIdx++) {
                string combo = combos[comboIdx];
                var comboGroup = new MpAvShortcutKeyGroupViewModel() {
                    SortIdx = comboIdx
                };
                var keys = combo.Split(new String[] { MpInputConstants.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                for (int keyIdx = 0; keyIdx < keys.Length; keyIdx++) {
                    string key = keys[keyIdx];

                    var skvm = new MpAvShortcutKeyViewModel() {
                        KeyStr = key,

                    };
                    comboGroup.Items.Add(skvm);
                }
                keyItems.Add(comboGroup);
            }
            return keyItems;
        }

        public static bool IsShortcutCommand(this MpShortcut sc, MpIShortcutCommandViewModel scvm) {
            if (sc == null || scvm == null) {
                return false;
            }
            return sc.ShortcutType == scvm.ShortcutType && sc.CommandParameter == scvm.ShortcutCommandParameter.ToStringOrDefault();
        }

        public static bool IsRawInputShortcut(this MpShortcutType st, object arg) {
            if (arg is MpAvShortcutRecorderParameterViewModel srpvm) {
                return srpvm.IsRawInput;
            }
            return false;

        }
        public static async Task<string> GetShortcutTitleAsync(this MpShortcutType st, object arg) {
            if (arg is MpAvParameterViewModelBase pvmb) {
                // these will either be a shortcut trigger recorder or key sim action
                return pvmb.Label;
            }

            if ((int)st < (int)MpShortcutType.MAX_APP_SHORTCUT) {
                return st.EnumToUiString();
            }
            if (arg == null) {
                MpDebug.Break("Error should have vm");
                return st.EnumToUiString();
            }
            string template = string.Empty;
            string title_arg = string.Empty;

            int id = 0;
            if (arg is string strArg) {
                try {
                    id = int.Parse(strArg);
                }
                catch {
                    id = 0;
                }
            } else if (arg is MpIShortcutCommandViewModel scvm) {
                if (scvm.ShortcutCommandParameter is int paramInt) {
                    id = paramInt;
                } else if (scvm.ShortcutCommandParameter is string strParam) {
                    try {
                        id = int.Parse(strParam);
                    }
                    catch {
                        id = 0;
                    }
                }

            }
            if (id <= 0) {
                return string.Format(template, "<Missing Ref>");
            }

            switch (st) {
                case MpShortcutType.PasteCopyItem:
                    template = "Paste '{0}'";

                    var ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(id);
                    if (ci != null) {
                        title_arg = ci.Title;
                    }
                    break;

                case MpShortcutType.SelectTag:
                    template = "Select '{0}' Collection";
                    var t = await MpDataModelProvider.GetItemAsync<MpTag>(id);
                    if (t != null) {
                        title_arg = t.TagName;
                    }
                    break;

                case MpShortcutType.AnalyzeCopyItemWithPreset:
                    template = "Run '{0}' Analyzer";
                    var pp = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(id);
                    if (pp != null) {
                        title_arg = pp.Label;
                    }
                    break;

                case MpShortcutType.InvokeTrigger:
                    template = "Run '{0}' Trigger";
                    var a = await MpDataModelProvider.GetItemAsync<MpAction>(id);
                    if (a != null) {
                        title_arg = a.Label;
                    }
                    break;
                default:
                    MpDebug.Break($"unknown shortcut type: '{st}'");
                    break;
            }

            return string.Format(template, title_arg);
        }

    }
}
