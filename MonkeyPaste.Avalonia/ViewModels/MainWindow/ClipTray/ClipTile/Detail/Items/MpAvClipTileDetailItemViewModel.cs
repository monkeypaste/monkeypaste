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
        MpViewModelBase<MpAvClipTileDetailCollectionViewModel>, MpISelectableViewModel,MpIHoverableViewModel {
        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }
        #endregion

        #region Statics

        private static string[] _bgColors = {
            MpSystemColors.Red,
            MpSystemColors.Yellow,
            MpSystemColors.blue1,
            MpSystemColors.green1,
            MpSystemColors.orange1
        };
        #endregion

        #region Properties



        #region Appearance

        public string DetailText { get; private set; }

        public string DetailUri { get; private set; }


        public string BorderBgHexColor {
            get {
                if (Parent == null) {
                    return MpSystemColors.Transparent;
                }
                return _bgColors[(int)DetailType - 1];//.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity);
            }
        }

        #endregion

        #region State

        public bool IsUriEnabled { get; private set; }
        #endregion

        #region Model

        //public MpSize ItemSize {
        //    get {
        //        if(CopyItem == null) {
        //            return MpSize.Empty;
        //        }
        //        return CopyItem.ItemSize;
        //    }
        //}

        public MpCopyItemDetailType DetailType { get; private set; }

        //public MpCopyItem CopyItem {
        //    get {
        //        if(Parent == null) {
        //            return null;
        //        }
        //        return Parent.CopyItem;
        //    }
        //}

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
            //IsBusy = true;
            //await Task.Delay(1);
            //Reset();
            UpdateDetailTextCommand.Execute(null);
            //IsBusy = false;
        }

        public override string ToString() {
            return DetailText;
        }
        #endregion

        #region Private Methods

        private void MpAvClipTileDetailItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                    if(IsSelected && !IsHovering) {
                        Parent.CycleDetailCommand.Execute(null);
                    }
                    //IsSelected = IsHovering;
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    //if (IsHovering) {

                    //    Parent.SelectedItem = this;
                    //} else {
                    //    Parent.SelectedItem = null;
                    //}
                    break;
                    //case nameof(IsSelected):
                    //    if (IsSelected) {
                    //        LastSelectedDateTime = DateTime.Now;
                    //        if(string.IsNullOrEmpty(DetailText)) {
                    //            UpdateDetailTextCommand.Execute(null);
                    //        }
                    //    }
                    //    break;
            }
        }

        private void Reset() {
            DetailText = null;
            DetailUri = String.Empty;
            IsUriEnabled = false;
        }
        #endregion

        #region Commands

        public ICommand UpdateDetailTextCommand => new MpCommand(
            () => {
                //IsBusy = true;

                Reset();

                var ctvm = Parent.Parent;
                if(ctvm == null || ctvm.IsPlaceholder) {
                    return;
                }
                var CopyItem = ctvm.CopyItem;
                var ItemSize = CopyItem.ItemSize;
                
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
                                DetailText = $"{ctvm.CharCount} chars | {ctvm.LineCount} lines";
                                break;
                            case MpCopyItemType.FileList:

                                DetailText = $"{ctvm.LineCount} files | {ctvm.CharCount} MBs";
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
                        DetailText = $"Goto url '{ctvm.UrlViewModel.UrlTitle}'";
                        DetailUri = ctvm.UrlViewModel.UrlPath;
                        IsUriEnabled = true;
                        break;
                    case MpCopyItemDetailType.AppInfo:
                        if (ctvm.AppViewModel == null) {
                            break;
                        }
                        DetailText = $"Open folder for '{ctvm.AppViewModel.AppName}'";
                        IsUriEnabled = ctvm.AppViewModel.UserDeviceId == MpPrefViewModel.Instance.ThisUserDevice.Id;                        
                        if(IsUriEnabled) {

                            DetailUri = ctvm.AppViewModel.AppPath;
                        } else {
                            DetailText += " [EXTERNAL SOURCE]";                            
                        }
                        break;
                    default:
                        break;
                }
                if(DetailText == String.Empty) {
                    DetailText = null;
                }

                OnPropertyChanged(nameof(DetailText));
                OnPropertyChanged(nameof(DetailUri));
                OnPropertyChanged(nameof(IsUriEnabled));
            });
        #endregion
    }
}
