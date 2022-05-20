using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpAppClipboardFormatInfoViewModel : 
        MpViewModelBase<MpAppClipboardFormatInfoCollectionViewModel>,
        MpISelectableViewModel {

        #region Properties

        #region State

        private MpCopyItemType _contentFilerType = MpCopyItemType.Text;
        public MpCopyItemType ContentTypeFilter {
            get {
                switch(ClipboardFormatType) {
                    case MpClipboardFormatType.Text:
                    case MpClipboardFormatType.Rtf:
                    case MpClipboardFormatType.OemText:
                    case MpClipboardFormatType.UnicodeText:
                    case MpClipboardFormatType.Html:
                    case MpClipboardFormatType.Csv:
                        _contentFilerType = MpCopyItemType.Text;
                        break;
                    case MpClipboardFormatType.Custom:
                    case MpClipboardFormatType.FileDrop:
                        _contentFilerType = MpCopyItemType.FileList;
                        break;
                    case MpClipboardFormatType.Bitmap:
                        _contentFilerType = MpCopyItemType.Image;
                        break;
                    default:
                        _contentFilerType = MpCopyItemType.None;
                        break;
                }
                return _contentFilerType;
            }
            set {
                if(ContentTypeFilter != value) {
                    _contentFilerType = value;
                    ClipboardFormatType = MpClipboardFormatType.None;
                    OnPropertyChanged(nameof(ContentTypeFilter));
                }
            }
        }

        public string SelectedContentType { get; set; } = "Text";

        public ObservableCollection<string> ContentTypes { get; set; } = new ObservableCollection<string>() {
            "Text",
            "Image",
            "FileList",
            "Custom"
        };
        
        public ObservableCollection<string> AvailableFormatTypes {
            get {
                ObservableCollection<string> _availableFormatTypes = new ObservableCollection<string>();

                switch (SelectedContentType) {
                    case "Text":
                        _availableFormatTypes.Add("Text");
                        _availableFormatTypes.Add("Rtf");
                        _availableFormatTypes.Add("Html");
                        _availableFormatTypes.Add("Csv");
                        break;
                    case "Image":
                        _availableFormatTypes.Add("Bitmap");
                        break;
                    case "FileList":
                        _availableFormatTypes.Add("Default");
                        break;
                    default:
                        break;
                }
                _availableFormatTypes.Add("Custom");
                return _availableFormatTypes;
            }
        }

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

        public int Priority {
            get {
                if (AppClipboardFormatInfo == null) {
                    return 0;
                }
                return AppClipboardFormatInfo.Priority;
            }
            set {
                if(Priority != value) {
                    AppClipboardFormatInfo.Priority = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        public bool IsFormatIgnored => Priority < 0;

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
            }
        }
        #endregion
    }
}
