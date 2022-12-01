using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAppendNotificationViewModel : MpNotificationViewModelBase {
        #region Statics

        private static MpAppendNotificationViewModel _instance;
        public static MpAppendNotificationViewModel Instance => _instance;

        public static async Task<MpAppendNotificationViewModel> InitAsync(MpNotificationFormat nf) {
            _instance = new MpAppendNotificationViewModel();
            await _instance.InitializeAsync(nf);
            return _instance;
        }
        #endregion
        #region Constructors
        private MpAppendNotificationViewModel() : base() { }
        #endregion

        #region Properties

        #region Model

        public override string Title {
            get {
                if (Body is MpITitledViewModel tvm) {
                    return tvm.Title;
                }
                return string.Empty;
            }
        }
        #endregion

        #endregion

        #region Public Methods

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            OnPropertyChanged(nameof(Title));

            if(base_result == MpNotificationDialogResultType.DoNotShow) {
                HideNotification();
                return base_result;
            }

            if (MaxShowTimeMs > 0) {
                DateTime startTime = DateTime.Now;
                while (DateTime.Now - startTime <= TimeSpan.FromMilliseconds(MaxShowTimeMs)) {
                    await Task.Delay(100);

                    while (IsHovering) {
                        await Task.Delay(100);
                        if(DoNotShowAgain) {
                            return MpNotificationDialogResultType.DoNotShow;
                        }
                    }
                }
                HideNotification();
            }
            return MpNotificationDialogResultType.None;
        }
        #endregion
    }
}
