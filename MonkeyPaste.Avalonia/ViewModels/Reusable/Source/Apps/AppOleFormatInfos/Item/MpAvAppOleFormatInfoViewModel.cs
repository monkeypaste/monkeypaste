using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleFormatInfoViewModel :
        MpAvViewModelBase<MpAvAppOleFormatInfoCollectionViewModel> {

        #region Interfaces
        #endregion

        #region Properties

        #region State
        #endregion

        #region Appearance

        #endregion

        #region MpISelectableViewModel Implementaton
        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region Model

        public string FormatName {
            get {
                if (AppOleFormatInfo == null) {
                    return string.Empty;
                }
                return AppOleFormatInfo.FormatName;
            }
            set {
                if (FormatName != value) {
                    AppOleFormatInfo.FormatName = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FormatName));
                }
            }
        }

        public string FormatInfo {
            get {
                if (AppOleFormatInfo == null) {
                    return string.Empty;
                }
                return AppOleFormatInfo.FormatInfo;
            }
            set {
                if (FormatInfo != value) {
                    AppOleFormatInfo.FormatInfo = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FormatInfo));
                }
            }
        }

        public bool IgnoreFormat {
            get {
                if (AppOleFormatInfo == null) {
                    return false;
                }
                return AppOleFormatInfo.IgnoreFormat;
            }
            set {
                if (IgnoreFormat != value) {
                    AppOleFormatInfo.IgnoreFormat = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IgnoreFormat));
                }
            }
        }


        public int WriterPresetId {
            get {
                if (AppOleFormatInfo == null) {
                    return 0;
                }
                return AppOleFormatInfo.WriterPresetId;
            }
        }

        public int AppId {
            get {
                if (AppOleFormatInfo == null) {
                    return 0;
                }
                return AppOleFormatInfo.AppId;
            }
        }

        public int AppOleInfoId {
            get {
                if (AppOleFormatInfo == null) {
                    return 0;
                }
                return AppOleFormatInfo.Id;
            }
        }
        public MpAppOleFormatInfo AppOleFormatInfo { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvAppOleFormatInfoViewModel() : base(null) { }

        public MpAvAppOleFormatInfoViewModel(MpAvAppOleFormatInfoCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAppClipboardFormatInfoViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppOleFormatInfo ais) {
            IsBusy = true;

            await Task.Delay(1);
            AppOleFormatInfo = ais;

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpAppClipboardFormatInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        if (AppId < 0) {
                            MpDebug.Break("Trying to set non-app specific clipboard format, ignoring");
                            break;
                        }
                        Task.Run(async () => {
                            await AppOleFormatInfo.WriteToDatabaseAsync();
                            HasModelChanged = false;

                        });
                    }
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
            }
        }
        #endregion
    }
}
