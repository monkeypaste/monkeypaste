using MonkeyPaste.Common;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleFormatInfoViewModel :
        MpAvViewModelBase<MpAvAppOleFormatInfoCollectionViewModel> {

        #region Interfaces
        #endregion

        #region Properties

        #region State

        public bool IsCustomFormatSelected => SelectedFormatType == "Custom";

        public bool IsFileListTypeSelected => SelectedClipboardFormatContentType == MpCopyItemType.FileList;

        public bool IsAddNewFileTypeSelected => SelectedFileType == "add new...";

        public string SelectedFormatType { get; set; }

        public string SelectedFileType { get; set; }
        public ObservableCollection<string> AvailableFileTypes {
            get {
                // TODO this should be populated from Clipboard Colllection
                return new ObservableCollection<string>() {
                    "",
                    ".txt",
                    ".rtf",
                    ".bmp",
                    ".png",
                    "default",
                    "add new..."
                };
            }
        }

        public ObservableCollection<string> AvailableFormatTypes {
            get {
                ObservableCollection<string> _availableFormatTypes = new ObservableCollection<string>();
                for (int i = 0; i < Enum.GetNames(typeof(MpClipboardFormatType)).Length; i++) {
                    var cft = (MpClipboardFormatType)i;
                    if (cft == MpClipboardFormatType.Custom) {
                        // TODO need to query clipboard plugins for custom formats here and have all available
                        continue;
                    }
                    _availableFormatTypes.Add(cft.EnumToUiString());
                }
                return _availableFormatTypes;
            }
        }

        public MpCopyItemType SelectedClipboardFormatContentType { get; set; }

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

        public int AppOleInfoId {
            get {
                if (AppOleFormatInfo == null) {
                    return 0;
                }
                return AppOleFormatInfo.Id;
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
                case nameof(SelectedClipboardFormatContentType):
                    OnPropertyChanged(nameof(AvailableFormatTypes));
                    OnPropertyChanged(nameof(IsFileListTypeSelected));
                    OnPropertyChanged(nameof(IsCustomFormatSelected));
                    break;
                case nameof(SelectedFormatType):
                    OnPropertyChanged(nameof(IsFileListTypeSelected));
                    OnPropertyChanged(nameof(IsCustomFormatSelected));

                    break;
            }
        }

        private MpCopyItemType GetFormatContentType(MpClipboardFormatType cft) {
            switch (cft) {
                case MpClipboardFormatType.Text:
                case MpClipboardFormatType.Rtf:
                case MpClipboardFormatType.OemText:
                case MpClipboardFormatType.UnicodeText:
                case MpClipboardFormatType.Html:
                case MpClipboardFormatType.Csv:
                    return MpCopyItemType.Text;
                case MpClipboardFormatType.Custom:
                case MpClipboardFormatType.FileDrop:
                    return MpCopyItemType.FileList;
                case MpClipboardFormatType.Bitmap:
                    return MpCopyItemType.Image;
                default:
                    return MpCopyItemType.None;
            }
        }

        #endregion
    }
}
