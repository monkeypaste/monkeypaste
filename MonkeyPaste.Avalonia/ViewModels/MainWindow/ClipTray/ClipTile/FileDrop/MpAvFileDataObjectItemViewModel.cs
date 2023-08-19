using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileDataObjectItemViewModel : MpAvViewModelBase<MpAvFileItemCollectionViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel {

        #region Properties

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }
        #endregion

        #region View Models 


        #endregion

        #region Appearance
        public string PathLabel {
            get {
                if (string.IsNullOrWhiteSpace(Path)) {
                    return string.Empty;
                }
                try {
                    return System.IO.Path.GetFileName(Path);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting file path '{Path}' to file name. ", ex);
                    return string.Empty;
                }
            }
        }
        public string FileItemBackgroundHexColor {
            get {
                if (IsHovering || IsSelected) {
                    return MpSystemColors.gainsboro;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string FileItemBorderHexColor {
            get {
                if (IsSelected) {
                    return MpSystemColors.Red;
                }
                if (IsHovering) {
                    return MpSystemColors.black;
                }
                return MpSystemColors.Transparent;
            }
        }
        #endregion

        #region Model
        public string Path {
            get {
                if (DataObjectItem == null) {
                    return string.Empty;
                }
                return DataObjectItem.ItemData;
            }
        }

        public int PathIconId {
            get {
                if (DataObjectItem == null) {
                    return 0;
                }
                return DataObjectItem.ItemDataIconId;
            }
        }


        private string _iconBase64 = null;
        public string IconBase64 {
            get {
                if (_iconBase64 == null &&
                    Path.IsFileOrDirectory() &&
                    MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == PathIconId)
                        is MpAvIconViewModel ivm) {
                    _iconBase64 = ivm.IconBase64;
                }
                if (_iconBase64 == null) {
                    if (MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Dark) {
                        _iconBase64 = MpBase64Images.MissingFile_white;
                    } else {
                        _iconBase64 = MpBase64Images.MissingFile;
                    }
                }
                return _iconBase64;
            }
        }

        public MpDataObjectItem DataObjectItem { get; private set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvFileDataObjectItemViewModel() : base(null) { }

        public MpAvFileDataObjectItemViewModel(MpAvFileItemCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpDataObjectItem dataObjectItem) {
            IsBusy = true;

            await Task.Delay(1);
            DataObjectItem = dataObjectItem;

            IsBusy = false;
        }

        #endregion

        #region Protected Overrides

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpIcon icon && icon.Id == PathIconId) {
                // path icon changed will need to notify content view
                OnPropertyChanged(nameof(IconBase64));
            }
        }
        #endregion

        #region COmmands 

        public ICommand NavigateToFileItemCommand => new MpCommand(
            () => {
                if (!Path.IsFileOrDirectory() ||
                    !Uri.IsWellFormedUriString(Path.ToFileSystemUriFromPath(), UriKind.Absolute) ||
                    new Uri(Path.ToFileSystemUriFromPath()) is not Uri fi_uri) {
                    return;
                }
                MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(fi_uri);
            }, () => {
                return MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown;
            });
        #endregion
    }
}
