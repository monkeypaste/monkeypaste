using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MpWpfApp {
    public class MpCompareOutput : MpActionOutput {
        public string MatchValue { get; set; }
        public bool IsCaseSensitive { get; set; }
    }

    public class MpCompareActionViewModelBase : MpActionViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Appearance

        public double CompareDataTextBoxWidth { get; set; } = 200;

        public double CompareDataTextBoxHeight { get; set; } = 30;

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

        public bool IsJsonQuery {
            get => CompareDataJsonPath != null;
            set => CompareDataJsonPath = value ? string.Empty : null;
        }

        public bool IsItemTypeCompare => ComparePropertyPathType == MpComparePropertyPathType.ItemType;

        public bool IsLastOutputCompare => ComparePropertyPathType == MpComparePropertyPathType.LastOutput;

        public bool IsContentPropertyCompare => !IsItemTypeCompare && !IsLastOutputCompare;

        public bool IsCompareTypeRegex => ComparisonOperatorType == MpComparisonOperatorType.Regex;

        #endregion

        #region Model
        //Arg4

        public string CompareDataJsonPath {
            get {
                if (Action == null) {
                    return null;
                }
                return Arg4;
            }
            set {
                if (CompareDataJsonPath != value) {
                    Arg4 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareDataJsonPath));
                }
            }
        }

        //Arg3

        public bool IsCaseSensitive {
            get {
                if(Action == null) {
                    return false;
                }
                if(IsCompareTypeRegex) {
                    return false;
                }
                return Arg3 == "1";
            }
            set {
                if(IsCaseSensitive != value && !IsCompareTypeRegex) {
                    Arg3 = value ? "1" : "0";
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsCaseSensitive));
                }
            }
        }

        //Arg2

        public MpCopyItemType ContentItemType {
            get {
                if (Action == null) {
                    return 0;
                }
                if (ComparePropertyPathType != MpComparePropertyPathType.ItemType) {
                    return 0;
                }
                if (string.IsNullOrWhiteSpace(Arg2)) {
                    return MpCopyItemType.None;
                }
                return (MpCopyItemType)Convert.ToInt32(Arg2);
            }
            set {
                if (ContentItemType != value) {
                    Arg2 = ((int)value).ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ContentItemType));
                }
            }
        }

        public string CompareData {
            get {
                if (Action == null) {
                    return null;
                }
                if(IsItemTypeCompare) {
                    return ContentItemType.ToString();
                }
                return Arg2;
            }
            set {
                if (CompareData != value) {
                    Arg2 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareData));
                }
            }
        }

        // Arg1

        public MpComparePropertyPathType ComparePropertyPathType {
            get {
                if (Action == null) {
                    return MpComparePropertyPathType.None;
                }
                if (string.IsNullOrWhiteSpace(Arg1)) {
                    return MpComparePropertyPathType.None;
                }

                return (MpComparePropertyPathType)Convert.ToInt32(Arg1);
            }
            set {
                if (ComparePropertyPathType != value) {
                    Arg1 = ((int)value).ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComparePropertyPathType));
                }
            }
        }

        public MpComparisonOperatorType ComparisonOperatorType {
            get {
                if (Action == null) {
                    return MpComparisonOperatorType.None;
                }
                if((MpComparisonOperatorType)ActionObjId == MpComparisonOperatorType.None) {
                    Action.ActionObjId = (int)MpComparisonOperatorType.Contains;
                }
                return (MpComparisonOperatorType)Action.ActionObjId;
            }
            set {
                if (ComparisonOperatorType != value) {
                    Action.ActionObjId = (int)value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComparisonOperatorType));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpCompareActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpCompareActionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override async Task PerformAction(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            MpActionOutput ao = GetInput(arg);

            object matchVal = null;
            if(ComparePropertyPathType == MpComparePropertyPathType.LastOutput) {
                if(ao != null) {
                    if(ao.OutputData is MpPluginResponseFormat prf && IsJsonQuery) {
                        try {
                            string prfStr = JsonConvert.SerializeObject(prf);
                            JObject jo;
                            if (prfStr.StartsWith("[")) {
                                JArray a = JArray.Parse(prfStr);
                                jo = a.Children<JObject>().First();
                            } else {
                                jo = JObject.Parse(prfStr);
                            }

                            var matchValPath = new MpJsonPathProperty() {
                                valuePath = CompareDataJsonPath
                            };
                            matchValPath.SetValue(jo, null);
                            matchVal = matchValPath.value;
                        } catch(Exception ex) {
                            MpConsole.WriteLine(@"Error parsing/querying json response:");
                            MpConsole.WriteLine(ao.OutputData.ToString());
                            MpConsole.WriteLine(@"For JSONPath: ");
                            MpConsole.WriteLine(CompareData);
                            MpConsole.WriteTraceLine(ex);
                            matchVal = null;
                        }
                    } else {
                        matchVal = ao.OutputData;
                    }
                    
                }                
            } else {
                matchVal = ao.CopyItem.GetPropertyValue(PhysicalPropertyPath);
            }
            string compareStr = string.Empty;
            if (matchVal != null) {
                compareStr = matchVal.ToString();
            }

            string matchStr = PerformMatch(compareStr);
            if (matchStr != null) {
                await base.PerformAction(new MpCompareOutput() {
                    Previous = ao,
                    CopyItem = ao.CopyItem,
                    MatchValue = matchStr,
                    IsCaseSensitive = this.IsCaseSensitive
                });
            }
        }

        #endregion

        #region Protected Overrides

        protected virtual string PerformMatch(string compareStr) {
            bool isCaseSensitive = IsCaseSensitive;
            string compareData = CompareData;
            if(compareData == null) {
                compareData = string.Empty;
            }
            compareData = isCaseSensitive ? compareData : compareData.ToLower();

            string unmodifiedCompareStr = compareStr;
            compareStr = compareStr == null ? string.Empty : compareStr;
            compareStr = isCaseSensitive ? compareStr : compareStr.ToLower();

            switch (ComparisonOperatorType) {
                case MpComparisonOperatorType.Contains:
                    if (compareStr.Contains(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpComparisonOperatorType.Exact:
                    if (compareStr.Equals(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpComparisonOperatorType.BeginsWith:
                    if (compareStr.StartsWith(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpComparisonOperatorType.EndsWith:
                    if (compareStr.EndsWith(compareData)) {
                        return unmodifiedCompareStr;
                    }
                    break;
                case MpComparisonOperatorType.Regex:
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
                    ResetArgs(2, 3, 4, 5);
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
                 ComparisonOperatorType = (MpComparisonOperatorType)args;
             });

        public ICommand ChangeComparePropertyPathCommand => new RelayCommand<object>(
              (args) => {
                 ComparePropertyPathType = (MpComparePropertyPathType)args;
              });

        #endregion
    }
}
