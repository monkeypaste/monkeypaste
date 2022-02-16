using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpCompareOutput : MpActionOutput {
        public string MatchValue { get; set; }
        public bool IsCaseSensitive { get; set; }
    }

    public class MpCompareActionViewModelBase : MpActionViewModelBase, MpIResizableViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        #region Menu Options

        //public MpMenuItemViewModel ContentTypeCompareDataMenuItemViewModel { get; private set; }

        //public MpMenuItemViewModel CompareTypesMenuItemViewModel { get; private set; }

        //public MpMenuItemViewModel ComparePropertyPathsMenuItemViewModel { get; private set; }

        #endregion

        #region Selected Menu Options

        //public MpMenuItemViewModel SelectedContentTypeMenuItemViewModel {
        //    get {
        //        if(ComparePropertyPathType != MpComparePropertyPathType.ItemType) {
        //            return null;
        //        }
        //        return ContentTypeCompareDataMenuItemViewModel.SubItems.FirstOrDefault(x => x.IsSelected);
        //    }
        //    set {
        //        if(SelectedContentTypeMenuItemViewModel != value) {
        //            ContentItemTypeIndex = ContentTypeCompareDataMenuItemViewModel.SubItems.IndexOf(value);
        //            OnPropertyChanged(nameof(SelectedContentTypeMenuItemViewModel));
        //        }
        //    }
        //}

        //public MpMenuItemViewModel SelectedCompareTypeMenuItemViewModel {
        //    get {
        //        return CompareTypesMenuItemViewModel.SubItems.FirstOrDefault(x => x.IsSelected);
        //    }
        //    set {
        //        if (SelectedCompareTypeMenuItemViewModel != value) {
        //            CompareType = (MpCompareType)CompareTypesMenuItemViewModel.SubItems.IndexOf(value);
        //            OnPropertyChanged(nameof(SelectedCompareTypeMenuItemViewModel));
        //        }
        //    }
        //}

        //public MpMenuItemViewModel SelectedComparePropertyPathMenuItemViewModel {
        //    get {
        //        return ComparePropertyPathsMenuItemViewModel.SubItems.FirstOrDefault(x => x.IsSelected);
        //    }
        //    set {
        //        if (SelectedComparePropertyPathMenuItemViewModel != value) {
        //            ComparePropertyPathType = (MpComparePropertyPathType)ComparePropertyPathsMenuItemViewModel.SubItems.IndexOf(value);
        //            OnPropertyChanged(nameof(SelectedComparePropertyPathMenuItemViewModel));
        //        }
        //    }
        //}
        #endregion

        #endregion

        #region Appearance

        public double CompareDataTextBoxWidth { get; set; } = 200;

        public double CompareDataTextBoxHeight { get; set; } = 30;
        #endregion

        #region State

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }
        #endregion

        #region Business Logic

        public string[] PhysicalComparePropertyPaths {
            get {
                var paths = new List<string>();
                for (int i = 0; i < Enum.GetNames(typeof(MpComparePropertyPathType)).Length; i++) {
                    string path = string.Empty;
                    MpComparePropertyPathType cppt = (MpComparePropertyPathType)i;
                    switch (cppt) {
                        case MpComparePropertyPathType.ItemData:
                        case MpComparePropertyPathType.ItemType:
                        case MpComparePropertyPathType.ItemDescription:
                        case MpComparePropertyPathType.Title:
                        case MpComparePropertyPathType.CopyDateTime:
                        case MpComparePropertyPathType.CopyCount:
                        case MpComparePropertyPathType.PasteCount:
                            path = cppt.ToString();
                            break;
                        case MpComparePropertyPathType.AppName:
                        case MpComparePropertyPathType.AppPath:
                            path = string.Format(@"Source.App.{0}", cppt.ToString());
                            break;
                        case MpComparePropertyPathType.UrlPath:
                        case MpComparePropertyPathType.UrlTitle:
                        case MpComparePropertyPathType.UrlDomain:
                            path = string.Format(@"Source.App.{0}", cppt.ToString());
                            break;
                        default:
                            continue;
                    }
                    paths.Add(path);
                }
                return paths.ToArray();
            }
        }

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

        public bool IsCompareTypeRegex => CompareType == MpCompareType.Regex;

        #endregion

        #region Model

        public bool IsCaseSensitive {
            get {
                if(Action == null) {
                    return false;
                }
                return Action.Arg3 == "1";
            }
            set {
                if(IsCaseSensitive != value) {
                    Action.Arg3 = value ? "1" : "0";
                    OnPropertyChanged(nameof(IsCaseSensitive));
                }
            }
        }

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

        public MpCopyItemType ContentItemType {
            get {
                if (Action == null) {
                    return 0;
                }
                if (ComparePropertyPathType != MpComparePropertyPathType.ItemType) {
                    return 0;
                }
                if (string.IsNullOrWhiteSpace(Action.Arg2)) {
                    return MpCopyItemType.None;
                }
                return (MpCopyItemType)Convert.ToInt32(Action.Arg2);
            }
            set {
                if (ContentItemType != value) {
                    Action.Arg2 = ((int)value).ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ContentItemType));
                }
            }
        }

        //public int ContentItemTypeIndex {
        //    get {
        //        if(Action == null) {
        //            return 0;
        //        }
        //        if(ComparePropertyPathType != MpComparePropertyPathType.ItemType) {
        //            return 0;
        //        }
        //        for (int i = 0; i < Enum.GetNames(typeof(MpCopyItemType)).Length; i++) {
        //            var contentTypeName = Enum.GetNames(typeof(MpCopyItemType))[i];
        //            if(CompareData == contentTypeName) {
        //                return i;
        //            }
        //        }
        //        return 0;
        //    }
        //    set {
        //        if(ContentItemTypeIndex != value) {
        //            Action.Arg2 = Enum.GetName(typeof(MpCopyItemType), value);
        //            OnPropertyChanged(nameof(ContentItemTypeIndex));
        //        }
        //    }
        //}

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

        public MpCompareActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpCompareActionViewModel_PropertyChanged;
        }

        public override async Task InitializeAsync(MpAction a) {
            await base.InitializeAsync(a);
            //////////////////////////////////
            //var amivml = new List<MpMenuItemViewModel>();
            //var contentTypeLabels = typeof(MpCopyItemType).EnumToLabels();
            //for (int i = 0; i < contentTypeLabels.Length; i++) {
            //    MpCopyItemType ct = (MpCopyItemType)i;

            //    amivml.Add(
            //        new MpMenuItemViewModel() {
            //            IsSelected = i == ContentItemTypeIndex,
            //            Header = Enum.GetName(typeof(MpCopyItemType), ct),
            //            Command = SetCompareDataContentTypeCommand,
            //            CommandParameter = ct,
            //            IsVisible = ct != MpCopyItemType.None
            //        });
            //}
            //ContentTypeCompareDataMenuItemViewModel = 
            //    new MpMenuItemViewModel() {
            //        SubItems = amivml
            //    };
            ////////////////////////////////
            //var amivml2 = new List<MpMenuItemViewModel>();
            //var triggerLabels = typeof(MpCompareType).EnumToLabels();
            //for (int i = 0; i < triggerLabels.Length; i++) {
            //    string resourceKey = string.Empty;
            //    MpCompareType ct = (MpCompareType)i;
            //    //switch (ct) {
            //    //    case MpCompareType.BeginsWith:
            //    //    case MpCompareType.EndsWith:
            //    //    case MpCompareType.Contains:
            //    //        resourceKey = "CaretIcon";
            //    //        break;
            //    //    case MpCompareType.Exact:
            //    //        resourceKey = "BullsEyeIcon";
            //    //        break;
            //    //    case MpCompareType.Regex:
            //    //        resourceKey = "BeakerIcon";
            //    //        break;
            //    //    case MpCompareType.Automatic:
            //    //        resourceKey = "AppendLineIcon";
            //    //        break;
            //    //    case MpCompareType.Wildcard:
            //    //        resourceKey = "AsteriskIcon";
            //    //        break;
            //    //}
            //    amivml2.Add(new MpMenuItemViewModel() {
            //        IsSelected = ct == CompareType,
            //        //IconResourceKey = Application.Current.Resources[resourceKey] as string,
            //        Header = triggerLabels[i],
            //        Command = ChangeCompareTypeCommand,
            //        CommandParameter = ct,
            //        IsVisible = !(ct == MpCompareType.None || ct == MpCompareType.Lexical)
            //    });
            //}
            //CompareTypesMenuItemViewModel = new MpMenuItemViewModel() {
            //    SubItems = amivml2
            //};
            /////////////////////////////////////
            //var amivml3 = new List<MpMenuItemViewModel>();

            //triggerLabels = typeof(MpComparePropertyPathType).EnumToLabels();
            //for (int i = 0; i < triggerLabels.Length; i++) {
            //    MpComparePropertyPathType ct = (MpComparePropertyPathType)i;
            //    var amivm = new MpMenuItemViewModel() {
            //        Header = triggerLabels[i],
            //        Command = ChangeComparePropertyPathCommand,
            //        CommandParameter = ct,
            //        IsSelected = ComparePropertyPathType == ct,
            //        IsVisible = !(ct == MpComparePropertyPathType.None)
            //    };
            //    //if (i < (int)MpComparePropertyPathType.AppPath) {
            //    //    amivm.HeaderedSeparatorLabel = "What";
            //    //} else if (i < (int)MpComparePropertyPathType.CopyDateTime) {
            //    //    amivm.HeaderedSeparatorLabel = "Where";
            //    //} else if (i < (int)MpComparePropertyPathType.CopyCount) {
            //    //    amivm.HeaderedSeparatorLabel = "When";
            //    //} else {
            //    //    amivm.HeaderedSeparatorLabel = "How (many)";
            //    //}
            //    amivml3.Add(amivm);
            //}
            //ComparePropertyPathsMenuItemViewModel = new MpMenuItemViewModel() {
            //    SubItems = amivml3
            //};
        }

        public override async Task PerformAction(object arg) {
            if(ComparePropertyPathType == MpComparePropertyPathType.None) {
                // TODO this should invalidate or notify user if node has children
                // that unset compare will never create match
                return;
            }
            MpCopyItem ci = null;
            if (arg is MpCopyItem) {
                ci = arg as MpCopyItem;
            } else if (arg is MpCompareOutput co) {
                ci = co.CopyItem;
            } else if (arg is MpAnalyzeOutput ao) {
                ci = ao.CopyItem;
            } else if (arg is MpClassifyOutput clo) {
                ci = clo.CopyItem;
            }

            object matchVal = ci.GetPropertyValue(PhysicalPropertyPath);
            string compareStr = string.Empty;
            if (matchVal != null) {
                compareStr = matchVal.ToString();
            }

            string matchStr = PerformMatch(compareStr);
            if (matchStr != null) {
                await base.PerformAction(new MpCompareOutput() {
                    Previous = arg as MpActionOutput,
                    CopyItem = ci,
                    MatchValue = matchStr,
                    IsCaseSensitive = this.IsCaseSensitive
                });
            }
        }

        #endregion

        #region Protected Overrides

        protected virtual string PerformMatch(string compareStr) {
            bool isCaseSensitive = IsCaseSensitive;

            string compareData = isCaseSensitive ? CompareData : CompareData.ToLower();
            string unmodifiedCompareStr = compareStr;
            compareStr = isCaseSensitive ? compareStr : compareStr.ToLower();
            switch (CompareType) {
                case MpCompareType.Contains:
                    if (compareStr.Contains(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpCompareType.Exact:
                    if (compareStr.Equals(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpCompareType.BeginsWith:
                    if (compareStr.StartsWith(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpCompareType.EndsWith:
                    if (compareStr.EndsWith(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpCompareType.Regex:
                    var regEx = new Regex(compareData, 
                                            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
                    var m = regEx.Match(compareStr);
                    if(m.Success) {
                        return m.Value;
                    }
                    break;
            }

            return null;
        }

        #endregion

        #region Private Methods

        private void MpCompareActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    //OnPropertyChanged(nameof(SelectedContentTypeMenuItemViewModel));
                    //OnPropertyChanged(nameof(SelectedComparePropertyPathMenuItemViewModel));
                    //OnPropertyChanged(nameof(SelectedCompareTypeMenuItemViewModel));
                    OnPropertyChanged(nameof(IsCompareTypeRegex));
                    OnPropertyChanged(nameof(IsCaseSensitive));
                    break;
                case nameof(ComparePropertyPathType):
                    OnPropertyChanged(nameof(IsItemTypeCompare));
                    break;
            }
        }
        
        #endregion

        #region Commands

        //public ICommand ShowCompareTypeChooserMenuCommand => new RelayCommand<object>(
        //     (args) => {
        //         var fe = args as FrameworkElement;
        //         var cm = new MpContextMenuView();
        //         cm.DataContext = CompareTypesMenuItemViewModel;
        //         fe.ContextMenu = cm;
        //         fe.ContextMenu.PlacementTarget = fe;
        //         fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
        //         fe.ContextMenu.IsOpen = true;
        //         fe.ContextMenu.Closed += ContextMenu_Closed;
        //     });

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
