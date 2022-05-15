using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpClipboardFormatPriorityViewModel : 
        MpAppInteropSettingViewModelBase {

        #region Properties

        #region State


        #endregion

        #region Appearance

        #endregion

        #region Model

        public MpAppInteropSettingType SettingType => MpAppInteropSettingType.ClipboardFormatPriority;

        // Arg1
        public MpClipboardFormatType ClipboardFormatType {
            get {
                MpClipboardFormatType clipboardFormatType = MpClipboardFormatType.None;
                if (AppInteropSetting == null) {
                    return clipboardFormatType;
                }
                try {
                    clipboardFormatType = (MpClipboardFormatType)Convert.ToInt32(AppInteropSetting.Arg1);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(@"Error converting Arg1 " + AppInteropSetting.Arg1 + " to MpClipboardFormatType ", ex);
                    return clipboardFormatType;
                }
                return clipboardFormatType;
            }
            set {
                if (ClipboardFormatType != value) {
                    AppInteropSetting.Arg1 = value.EnumToInt().ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ClipboardFormatType));
                }
            }
        }

        // Arg2
        public string FormatInfo {
            get {
                if (AppInteropSetting == null) {
                    return string.Empty;
                }
                return AppInteropSetting.Arg2;
            }
            set {
                if (FormatInfo != value) {
                    AppInteropSetting.Arg2 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FormatInfo));
                }
            }
        }

        // Arg3
        public int Priority {
            get {
                int priority = int.MinValue;
                if (AppInteropSetting == null) {
                    return priority;
                }
                if(string.IsNullOrEmpty(AppInteropSetting.Arg3)) {
                    return 0;
                }
                try {
                    priority = Convert.ToInt32(AppInteropSetting.Arg3);
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine(@"Error converting Arg3 " + AppInteropSetting.Arg3 + " to int ", ex);
                    return priority;
                }
                return priority;
            }
            set {
                if(Priority != value) {
                    AppInteropSetting.Arg3 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        public bool IsFormatIgnored => Priority < 0;

        #endregion

        #endregion

        #region Constructors

        public MpClipboardFormatPriorityViewModel(MpAppInteropSettingCollectionViewModel parent) : base(parent) {
        }

        #endregion
    }
}
