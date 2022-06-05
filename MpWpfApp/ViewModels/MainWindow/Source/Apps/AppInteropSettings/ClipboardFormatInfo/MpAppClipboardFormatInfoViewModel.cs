using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpAppClipboardFormatInfoViewModel : 
        MpViewModelBase<MpAppClipboardFormatInfoCollectionViewModel>,
        MpISelectableViewModel {

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
                for(int i = 0;i < Enum.GetNames(typeof(MpClipboardFormatType)).Length;i++) {
                    var cft = (MpClipboardFormatType)i;
                    if(cft == MpClipboardFormatType.Custom) {
                        // TODO need to query clipboard plugins for custom formats here and have all available
                        continue;
                    }
                    _availableFormatTypes.Add(cft.EnumToLabel());
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

        public MpClipboardFormatType ClipboardFormatType {
            get {
                if (AppClipboardFormatInfo == null) {
                    return MpClipboardFormatType.None;
                }
                return AppClipboardFormatInfo.FormatType;
            }
            set {
                if (ClipboardFormatType != value) {
                    AppClipboardFormatInfo.FormatType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ClipboardFormatType));
                }
            }
        }

        

        public string FormatInfo {
            get {
                if (AppClipboardFormatInfo == null) {
                    return string.Empty;
                }
                return AppClipboardFormatInfo.FormatInfo;
            }
            set {
                if (FormatInfo != value) {
                    AppClipboardFormatInfo.FormatInfo = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(FormatInfo));
                }
            }
        }

        public bool IgnoreFormat {
            get {
                if (AppClipboardFormatInfo == null) {
                    return false;
                }
                return AppClipboardFormatInfo.IgnoreFormat;
            }
            set {
                if(IgnoreFormat != value) {
                    AppClipboardFormatInfo.IgnoreFormat = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IgnoreFormat));
                }
            }
        }

        public int AppInteropSettingId {
            get {
                if (AppClipboardFormatInfo == null) {
                    return 0;
                }
                return AppClipboardFormatInfo.Id;
            }
        }

        public int AppId {
            get {
                if (AppClipboardFormatInfo == null) {
                    return 0;
                }
                return AppClipboardFormatInfo.AppId;
            }
        }

        public MpAppClipboardFormatInfo AppClipboardFormatInfo { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAppClipboardFormatInfoViewModel() : base(null) { }

        public MpAppClipboardFormatInfoViewModel(MpAppClipboardFormatInfoCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAppClipboardFormatInfoViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpAppClipboardFormatInfo ais) {
            IsBusy = true;

            await Task.Delay(1);
            AppClipboardFormatInfo = ais;

            IsBusy = false;
        }

        #endregion

        #region Private Methods
        
        private void MpAppClipboardFormatInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await AppClipboardFormatInfo.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
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
