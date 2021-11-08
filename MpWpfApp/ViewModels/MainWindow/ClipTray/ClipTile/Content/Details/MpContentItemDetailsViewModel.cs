using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpContentItemDetailsViewModel : MpViewModelBase<MpContentItemViewModel> {
        #region Properties

        public Size ItemSize {get;set;}

        public DateTime CreatedDateTime { 
            get {
                if(Parent == null || Parent.IsPlaceholder) {
                    return DateTime.MaxValue;
                }
                return Parent.CopyItemCreatedDateTime;
            }
        }

        public DateTime ModifiedDateTime {
            get {
                if (Parent == null || Parent.IsPlaceholder) {
                    return DateTime.MaxValue;
                }
                return Parent.CopyItemCreatedDateTime;
            }
        }

        public int DetailIdx { 
            get {
                if(Parent == null || Parent.Parent == null) {
                    return 0;
                }
                return Parent.Parent.DetailIdx;
            }
            set {
                if(Parent != null && Parent.Parent != null && Parent.Parent.DetailIdx != value) {
                    Parent.Parent.DetailIdx = value;
                    OnPropertyChanged(nameof(DetailIdx));
                }
            }
        }

        public MpCopyItemDetailType CurDetailType {
            get {
                return (MpCopyItemDetailType)DetailIdx;
            }
        }

        public string DetailText {
            get {
                return ToString();
            }
        }

        public Brush DetailTextColor {
            get {
                if(Parent == null || Parent.Parent == null) {
                    return Brushes.Transparent;
                }
                if (Parent.IsSelected || Parent.Parent.IsSelected) {
                    return Brushes.Black;//Brushes.DarkGray;
                }
                if (Parent.IsHovering || Parent.Parent.IsHovering) {
                    return Brushes.Black;//Brushes.DimGray;
                }
                return Brushes.Transparent;
            }
        }

        #endregion

        #region Constructors
        public MpContentItemDetailsViewModel(MpContentItemViewModel parent) : base(parent) {
            //PropertyChanged += MpContentItemDetailsViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task Initialize() {
            await MpHelpers.Instance.RunOnMainThreadAsync(ResetDetails, DispatcherPriority.Background);
        }

        public async Task ResetDetails() {
            if (Parent == null || Parent.IsPlaceholder) {
                return;
            }

            DetailIdx = 1;
            switch (Parent.CopyItem.ItemType) {
                case MpCopyItemType.Image:
                    var bmp = Parent.CopyItem.ItemData.ToBitmapSource();
                    ItemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MpCopyItemType.FileList:
                    var fl = await MpCopyItemMerger.Instance.GetFileList(Parent.CopyItem);
                    ItemSize = new Size(fl.Count,MpHelpers.Instance.FileListSize(fl.ToArray()));
                    break;
                case MpCopyItemType.RichText:
                    ItemSize = new Size(
                        Parent.CopyItem.ItemData.ToPlainText().Length,
                        MpHelpers.Instance.GetRowCount(Parent.CopyItem.ItemData.ToPlainText()));
                    break;
            }
        }

        public override string ToString() {
            if (Parent == null || Parent.IsPlaceholder) {
                return string.Empty;
            }

            MpCopyItemDetailType detailType = (MpCopyItemDetailType)DetailIdx;
            string infoStr = string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc

                    infoStr = "Copied " + CreatedDateTime.ToString();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (Parent.CopyItem.ItemType == MpCopyItemType.Image) {
                        infoStr = "(" + (int)ItemSize.Width + "px) x (" + (int)ItemSize.Height + "px)";
                    } else if (Parent.CopyItem.ItemType == MpCopyItemType.RichText) {
                        infoStr = (int)ItemSize.Width + " chars | " + (int)ItemSize.Height + " lines";
                    } else if (Parent.CopyItem.ItemType == MpCopyItemType.FileList) {
                        infoStr = (int)ItemSize.Width + " files | " + Math.Round(ItemSize.Height,2) + " MB";
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    infoStr = Parent.CopyItem.CopyCount + " copies | " + Parent.CopyItem.PasteCount + " pastes";
                    break;
                case MpCopyItemDetailType.UrlInfo:
                    if (Parent.CopyItem.Source.Url == null) {
                        DetailIdx++;
                        infoStr = ToString();
                    } else {
                        infoStr = Parent.CopyItem.Source.Url.UrlPath;
                    }
                    break;
                case MpCopyItemDetailType.AppInfo:
                    if (Parent.CopyItem.Source.App.UserDevice.Guid == MpPreferences.Instance.ThisDeviceGuid) {
                        infoStr = Parent.CopyItem.Source.App.AppPath;
                    } else {
                        infoStr = Parent.CopyItem.Source.App.AppPath;
                    }

                    break;
                default:
                    infoStr = "Unknown detailId: " + (int)detailType;
                    break;
            }

            return infoStr;
        }
        #endregion


        #region Commands


        public ICommand CycleDetailCommand => new RelayCommand(
            () => {
                DetailIdx++;
                if (DetailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    DetailIdx = 1;
                }

                // TODO this should aggregate details over all sub items 
                OnPropertyChanged(nameof(DetailText));
            });
        #endregion
    }
}
