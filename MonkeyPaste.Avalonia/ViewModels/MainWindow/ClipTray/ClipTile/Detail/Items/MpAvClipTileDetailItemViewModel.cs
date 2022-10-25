using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileDetailItemViewModel : 
        MpViewModelBase<MpAvClipTileDetailCollectionViewModel>,
        MpISelectableViewModel {
        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region Properties

        

        #region Appearance

        public string DetailText { get; private set; }

        public string DetailUri { get; private set; }

        #endregion

        #region State

        public bool IsUriEnabled { get; private set; }
        #endregion

        #region Model

        public MpSize ItemSize {
            get {
                if(CopyItem == null) {
                    return MpSize.Empty;
                }
                return CopyItem.ItemSize;
            }
        }

        public MpCopyItemDetailType DetailType { get; private set; }

        public MpCopyItem CopyItem {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.CopyItem;
            }
        }

        #endregion

        #endregion

        #region Constructors
        public MpAvClipTileDetailItemViewModel(MpAvClipTileDetailCollectionViewModel parent, MpCopyItemDetailType detailType) : base(parent) {
            DetailType = detailType;
            PropertyChanged += MpAvClipTileDetailItemViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task IntializeAsync() {
            IsBusy = true;
            await Task.Delay(1);
            Reset();
            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpAvClipTileDetailItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if(string.IsNullOrEmpty(DetailText)) {
                            UpdateDetailTextCommand.Execute(null);
                        }
                    }
                    break;
            }
        }

        private void Reset() {

            DetailText = String.Empty;
            DetailUri = String.Empty;
            IsUriEnabled = false;
        }
        #endregion

        #region Commands

        public ICommand UpdateDetailTextCommand => new MpCommand(
            () => {
                IsBusy = true;

                Reset();

                var ctvm = Parent.Parent;
                if(CopyItem == null || ctvm.IsPlaceholder) {
                    return;
                }
                
                switch (DetailType) {
                    //created
                    case MpCopyItemDetailType.DateTimeCreated:
                        DetailText = "Copied " + CopyItem.CopyDateTime.ToReadableTimeSpan();
                        break;
                    case MpCopyItemDetailType.DataSize:
                        switch(CopyItem.ItemType) {
                            case MpCopyItemType.Image:
                                DetailText = $"({ItemSize.Width}px) | ({ItemSize.Height}px)";
                                break;
                            case MpCopyItemType.Text:
                                DetailText = $"{ItemSize.Width} chars | {ItemSize.Height} lines";
                                break;
                            case MpCopyItemType.FileList:

                                DetailText = $"{ItemSize.Width} files | {ItemSize.Height} MBs";
                                break;
                        }
                        break;
                    //# copies/# pastes
                    case MpCopyItemDetailType.UsageStats:
                        DetailText = $"{CopyItem.CopyCount} copies | {CopyItem.PasteCount} pastes";
                        break;
                    case MpCopyItemDetailType.UrlInfo:
                        if (ctvm.UrlViewModel == null) {
                            break;
                        }
                        DetailText = ctvm.UrlViewModel.UrlTitle;
                        DetailUri = ctvm.UrlViewModel.UrlPath;
                        IsUriEnabled = true;
                        break;
                    case MpCopyItemDetailType.AppInfo:
                        if (ctvm.AppViewModel == null) {
                            break;
                        }

                        DetailText = ctvm.AppViewModel.AppName;
                        DetailUri = ctvm.AppViewModel.AppPath;
                        IsUriEnabled = ctvm.AppViewModel.UserDeviceId == MpPrefViewModel.Instance.ThisUserDevice.Id;
                        break;
                    default:
                        break;
                }
            });
        #endregion
    }
}
