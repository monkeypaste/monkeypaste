using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace MonkeyPaste.Avalonia {
    public class MpAvFileDataObjectItemViewModel : MpViewModelBase<MpAvFileItemCollectionViewModel>,
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
                if (_iconBase64 == null) {
                    if (!Path.IsFileOrDirectory()) {
                        if (Parent != null &&
                            Parent.Parent != null &&
                            Parent.Parent.TransactionCollectionViewModel != null) {
                            if (Parent.Parent.TransactionCollectionViewModel.SourceUserDeviceId == MpDefaultDataModelTools.ThisUserDeviceId) {
                                // this path is FROM this device but gone so show error icon
                                return MpBase64Images.MissingFile;
                            } else {
                                // this path is from a DIFFERENT device
                                // TODO maybe this should have a unique visual identifier to show 
                                // this isn't from device. For now though just pull cached file icon...
                            }
                        } else {
                            // create transaction not ready
                        }
                    }
                    if (PathIconId == 0) {
                        // fallback to question mark
                        return MpBase64Images.QuestionMark;
                    }
                    var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == PathIconId);
                    if (ivm == null) {
                        // this shouldn't happen and maybe race condition issues with startup or copy item create
                        MpDebug.Break();
                        _iconBase64 = MpBase64Images.Error;
                    } else {
                        _iconBase64 = ivm.IconBase64;
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
    }
}
