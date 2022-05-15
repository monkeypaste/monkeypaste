using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpPasteShortcutViewModel : 
        MpAppInteropSettingViewModelBase {

        #region Properties

        #region Model

        public MpAppInteropSettingType SettingType => MpAppInteropSettingType.PasteShortcut;

        // Arg2
        public string PasteShortcutKeyString {
            get {
                if (AppInteropSetting == null) {
                    return string.Empty;
                }
                return AppInteropSetting.Arg1;
            }
            set {
                if (PasteShortcutKeyString != value) {
                    AppInteropSetting.Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PasteShortcutKeyString));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpPasteShortcutViewModel(MpAppInteropSettingCollectionViewModel parent) : base(parent) {
        }
        
        #endregion

        #region Public Methods

        #endregion
    }
}
