using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpCompareActionViewModel : MpActionViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel ContentTypeCompareDataMenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var contentTypeLabels = typeof(MpCopyItemType).EnumToLabels();
                for (int i = 0; i < contentTypeLabels.Length; i++) {
                    MpCopyItemType ct = (MpCopyItemType)i;

                    amivml.Add(
                        new MpMenuItemViewModel() {
                            IsSelected = i == ContentItemTypeIndex,
                            Header = Enum.GetName(typeof(MpCopyItemType),ct),
                            Command = SetCompareDataContentTypeCommand,
                            CommandParameter = ct,
                            IsVisible = ct != MpCopyItemType.None
                        });
                }
                return new MpMenuItemViewModel() {
                    SubItems = amivml
                };
            }
        }

        public MpMenuItemViewModel CompareTypesMenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpCompareType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    MpCompareType ct = (MpCompareType)i;
                    switch (ct) {
                        case MpCompareType.BeginsWith:
                        case MpCompareType.EndsWith:
                        case MpCompareType.Contains:
                            resourceKey = "CaretIcon";
                            break;
                        case MpCompareType.Exact:
                            resourceKey = "BullsEyeIcon";
                            break;
                        case MpCompareType.Regex:
                            resourceKey = "BeakerIcon";
                            break;
                        case MpCompareType.Automatic:
                            resourceKey = "AppendLineIcon";
                            break;
                        case MpCompareType.Wildcard:
                            resourceKey = "AsteriskIcon";
                            break;
                    }
                    amivml.Add(new MpMenuItemViewModel() {
                        IsSelected = ct == CompareType,
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = ChangeCompareTypeCommand,
                        CommandParameter = ct,
                        IsVisible = !(ct == MpCompareType.None || ct == MpCompareType.Lexical)
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = amivml
                };
            }
        }

        public MpMenuItemViewModel ComparePropertyPathsMenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();

                var triggerLabels = typeof(MpComparePropertyPathType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    MpComparePropertyPathType ct = (MpComparePropertyPathType)i;
                    var amivm = new MpMenuItemViewModel() {
                        Header = triggerLabels[i],
                        Command = ChangeComparePropertyPathCommand,
                        CommandParameter = ct,
                        IsSelected = ComparePropertyPathType == ct,
                        IsVisible = !(ct == MpComparePropertyPathType.None)
                    };
                    //if (i < (int)MpComparePropertyPathType.AppPath) {
                    //    amivm.HeaderedSeparatorLabel = "What";
                    //} else if (i < (int)MpComparePropertyPathType.CopyDateTime) {
                    //    amivm.HeaderedSeparatorLabel = "Where";
                    //} else if (i < (int)MpComparePropertyPathType.CopyCount) {
                    //    amivm.HeaderedSeparatorLabel = "When";
                    //} else {
                    //    amivm.HeaderedSeparatorLabel = "How (many)";
                    //}
                    amivml.Add(amivm);
                }
                return new MpMenuItemViewModel() {
                    SubItems = amivml
                };
            }
        }
        #endregion

        #region Appearance

        public double CompareDataTextBoxWidth { get; set; } = 200;

        public double CompareDataTextBoxHeight { get; set; } = 30;
        #endregion

        #region State

        #endregion

        #region Business Logic

        public string PhysicalPropertyPath {
            get {
                switch (ComparePropertyPathType) {
                    case MpComparePropertyPathType.ItemData:
                    case MpComparePropertyPathType.ItemType:
                    case MpComparePropertyPathType.ItemDescription:
                    case MpComparePropertyPathType.Title:
                    case MpComparePropertyPathType.CopyDateTime:
                    case MpComparePropertyPathType.CopyCount:
                    case MpComparePropertyPathType.PasteCount:
                        return ComparePropertyPathType.ToString();
                    case MpComparePropertyPathType.AppName:
                    case MpComparePropertyPathType.AppPath:
                        return string.Format(@"Source.App.{0}", ComparePropertyPathType.ToString());
                    case MpComparePropertyPathType.UrlPath:
                    case MpComparePropertyPathType.UrlTitle:
                    case MpComparePropertyPathType.UrlDomain:
                        return string.Format(@"Source.App.{0}", ComparePropertyPathType.ToString());
                }
                return string.Empty;
            }
        }

        #endregion

        #region State

        public bool IsItemTypeCompare => ComparePropertyPathType == MpComparePropertyPathType.ItemType;

        #endregion

        #region Model

        public MpComparePropertyPathType ComparePropertyPathType {
            get {
                if (Action == null) {
                    return MpComparePropertyPathType.None;
                }
                if (string.IsNullOrWhiteSpace(Action.Arg1)) {
                    return MpComparePropertyPathType.None;
                }
                return (MpComparePropertyPathType)Convert.ToInt32(Action.Arg1);
            }
            set {
                if (ComparePropertyPathType != value) {
                    Action.Arg1 = ((int)value).ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComparePropertyPathType));
                }
            }
        }

        public int ContentItemTypeIndex {
            get {
                if(Action == null) {
                    return 0;
                }
                if(ComparePropertyPathType != MpComparePropertyPathType.ItemType) {
                    return 0;
                }
                for (int i = 0; i < Enum.GetNames(typeof(MpCopyItemType)).Length; i++) {
                    var contentTypeName = Enum.GetNames(typeof(MpCopyItemType))[i];
                    if(CompareData == contentTypeName) {
                        return i;
                    }
                }
                return 0;
            }
            set {
                if(ContentItemTypeIndex != value) {
                    Action.Arg2 = Enum.GetName(typeof(MpCopyItemType), value);
                    OnPropertyChanged(nameof(ContentItemTypeIndex));
                }
            }
        }

        public string CompareData {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Arg2;
            }
            set {
                if (CompareData != value) {
                    Action.Arg2 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareData));
                    //OnPropertyChanged(nameof(ContentItemTypeIndex));
                }
            }
        }

        public MpCompareType CompareType {
            get {
                if (Action == null) {
                    return MpCompareType.None;
                }
                if((MpCompareType)ActionObjId == MpCompareType.None) {
                    Action.ActionObjId = (int)MpCompareType.Contains;
                }
                return (MpCompareType)Action.ActionObjId;
            }
            set {
                if (CompareType != value) {
                    Action.ActionObjId = (int)value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareType));
                }
            }
        }

        #endregion

        #endregion


        #region Constructors

        public MpCompareActionViewModel(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpCompareActionViewModel_PropertyChanged;
        }

        private void MpCompareActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(ComparePropertyPathType):
                    OnPropertyChanged(nameof(IsItemTypeCompare));
                    break;
            }
        }

        #endregion

        #region Protected Overrides

        public virtual async Task PerformAction(MpCopyItem arg) {
            object matchVal = arg.GetPropertyValue(PhysicalPropertyPath);
            string compareStr = string.Empty;
            if (matchVal != null) {
                compareStr = matchVal.ToString();
            }

            if (IsMatch(compareStr)) {
                await base.PerformAction(arg);
            }
        }

        #endregion

        #region Private Methods

        private bool IsMatch(string compareStr) {
            switch (CompareType) {
                case MpCompareType.Contains:
                    if (compareStr.ToLower().Contains(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.Exact:
                    if (compareStr.ToLower().Equals(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.BeginsWith:
                    if (compareStr.ToLower().StartsWith(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.EndsWith:
                    if (compareStr.ToLower().EndsWith(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.Regex:
                    var regEx = new Regex(CompareData,
                                            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
                    if (regEx.IsMatch(compareStr)) {
                        return true;
                    }
                    break;
            }
            return false;
        }

        #endregion

        #region Commands

        public ICommand ShowCompareTypeChooserMenuCommand => new RelayCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;
                 var cm = new MpContextMenuView();
                 cm.DataContext = CompareTypesMenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
                 fe.ContextMenu.Closed += ContextMenu_Closed;
             });

        private void ContextMenu_Closed(object sender, RoutedEventArgs e) {
            return;
        }

        public ICommand SetCompareDataContentTypeCommand => new RelayCommand<object>(
             (args) => {
                 CompareData = Enum.GetName(typeof(MpCopyItemType), args);
             });

        public ICommand ChangeCompareTypeCommand => new RelayCommand<object>(
             (args) => {
                 CompareType = (MpCompareType)args;
             });

        public ICommand ChangeComparePropertyPathCommand => new RelayCommand<object>(
              (args) => {
                 ComparePropertyPathType = (MpComparePropertyPathType)args;
              });

        #endregion
    }
}
