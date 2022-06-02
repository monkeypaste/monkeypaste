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
using System.Windows.Documents;
using Xamarin.Essentials;

namespace MpWpfApp {
    public class MpComparisionMatch {
        public string Text { get; private set; } = string.Empty;
        public int Offset { get; private set; } = -1;

        public int Length { get; private set; } = 0;

        public MpComparisionMatch(int offset, int length) {
            Offset = offset;
            Length = length;
        }

        public MpComparisionMatch(string text, int offset, int length) : this(offset,length) {
            Text = text;
        }

        public override string ToString() {
            return $"OFFSET: {Offset} LENGTH: {Length} TEXT: {Text}";
        }
    }
    public class MpCompareOutput : MpActionOutput {
        public override object OutputData => Matches;
        public List<MpComparisionMatch> Matches { get; set; }

        public override string ActionDescription {
            get {
                if(Matches == null || Matches.Count == 0) {
                    return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was NOT a match";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was matched w/ Match Value: {string.Join(Environment.NewLine,Matches)}";
            }
        }
    }

    public class MpCompareActionViewModelBase : MpActionViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region Statics

        #endregion

        #region View Models

        #endregion

        #region Appearance

        public double CompareDataTextBoxWidth { get; set; } = 200;

        public double CompareDataTextBoxHeight { get; set; } = 30;

        #endregion

        #region Business Logic

        public string PhysicalPropertyPath {
            get {
                switch (ComparePropertyPathType) {
                    case MpCopyItemPropertyPathType.ItemData:
                    case MpCopyItemPropertyPathType.ItemType:
                    case MpCopyItemPropertyPathType.ItemDescription:
                    case MpCopyItemPropertyPathType.Title:
                    case MpCopyItemPropertyPathType.CopyDateTime:
                    case MpCopyItemPropertyPathType.CopyCount:
                    case MpCopyItemPropertyPathType.PasteCount:
                        return ComparePropertyPathType.ToString();
                    case MpCopyItemPropertyPathType.AppName:
                    case MpCopyItemPropertyPathType.AppPath:
                        return string.Format(@"Source.App.{0}", ComparePropertyPathType.ToString());
                    case MpCopyItemPropertyPathType.UrlPath:
                    case MpCopyItemPropertyPathType.UrlTitle:
                    case MpCopyItemPropertyPathType.UrlDomainPath:
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

        public bool IsItemTypeCompare => ComparePropertyPathType == MpCopyItemPropertyPathType.ItemType;

        public bool IsLastOutputCompare => ComparePropertyPathType == MpCopyItemPropertyPathType.LastOutput;

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
                if (ComparePropertyPathType != MpCopyItemPropertyPathType.ItemType) {
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

        public MpCopyItemPropertyPathType ComparePropertyPathType {
            get {
                if (Action == null) {
                    return MpCopyItemPropertyPathType.None;
                }
                if (string.IsNullOrWhiteSpace(Arg1)) {
                    return MpCopyItemPropertyPathType.None;
                }

                return (MpCopyItemPropertyPathType)Convert.ToInt32(Arg1);
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
            if(ComparePropertyPathType == MpCopyItemPropertyPathType.LastOutput) {
                if(ao != null) {
                    if(ao.OutputData is MpPluginResponseFormat prf && IsJsonQuery) {
                        try {
                            //string prfStr = JsonConvert.SerializeObject(prf);
                            //JObject jo;
                            //if (prfStr.StartsWith("[")) {
                            //    JArray a = JArray.Parse(prfStr);
                            //    jo = a.Children<JObject>().First();
                            //} else {
                            //    jo = JObject.Parse(prfStr);
                            //}

                            //var matchValPath = new MpJsonPathProperty() {
                            //    valuePath = CompareDataJsonPath
                            //};
                            //matchValPath.SetValue(jo, null);
                            matchVal = MpJsonPathProperty.Query(prf, CompareDataJsonPath);
                        } catch(Exception ex) {
                            matchVal = null;
                            MpConsole.WriteLine(@"Error parsing/querying json response:");
                            MpConsole.WriteLine(ao.OutputData.ToString().ToPrettyPrintJson());
                            MpConsole.WriteLine(@"For JSONPath: ");
                            MpConsole.WriteLine(CompareData);
                            MpConsole.WriteTraceLine(ex);

                            ValidationText = $"Error performing action '{RootTriggerActionViewModel.Label}/{Label}': {ex}";
                            await ShowValidationNotification();
                        }
                    } else {
                        matchVal = ao.OutputData;
                    }                    
                }                
            } else {
                matchVal = await MpCopyItem.QueryProperty(ao.CopyItem, ComparePropertyPathType);
                //matchVal = ao.CopyItem.GetPropertyValue(PhysicalPropertyPath);
            }
            string compareStr = string.Empty;
            if (matchVal != null) {
                compareStr = matchVal.ToString();
            }

            var matches = GetMatches(compareStr);
            if (matches != null && matches.Count > 0) {
                await base.PerformAction(new MpCompareOutput() {
                    Previous = ao,
                    CopyItem = ao.CopyItem,
                    Matches = matches
                });
            }
        }

        #endregion

        #region Protected Overrides

        protected virtual List<MpComparisionMatch> GetMatches(string compareStr) {
            object compareObj;
            if(compareStr.IsStringRichText()) {
                compareObj = compareStr.ToFlowDocument();
            } else {
                compareObj = compareStr;
            }

            var matches = new List<MpComparisionMatch>();
            int idx = 0;
            switch (ComparisonOperatorType) {
                case MpComparisonOperatorType.Contains:
                case MpComparisonOperatorType.Exact:
                case MpComparisonOperatorType.BeginsWith:
                case MpComparisonOperatorType.EndsWith:
                    while (true) {
                        var subMatch = GetNextMatch(compareObj,CompareData, idx);
                        if (subMatch == null) {
                            break;
                        }
                        matches.Add(subMatch);
                        idx = subMatch.Offset + subMatch.Length + 1;
                    }
                    break;
                case MpComparisonOperatorType.Regex:
                    Regex regex = new Regex(CompareData, RegexOptions.Compiled | RegexOptions.Multiline);
                    MatchCollection mc = regex.Matches(compareStr);
                    

                    foreach (Match m in mc) {
                        foreach (Group mg in m.Groups) {
                            foreach (Capture c in mg.Captures) {
                                var match = GetNextMatch(compareObj,c.Value, idx);
                                if(match == null) {
                                    Debugger.Break();
                                }
                                matches.Add(match);
                                idx = match.Offset + match.Length + 1;
                            }
                        }
                    }
                    break;
            }

            return matches;
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

        protected virtual MpComparisionMatch GetNextMatch(object compareObj,string matchStr, int idx = 0) {
            bool isCaseSensitive = IsCaseSensitive;
            string compareData = matchStr;
            if (compareData == null) {
                compareData = string.Empty;
            }
            compareData = isCaseSensitive ? compareData : compareData.ToLower();

            if (compareObj is string compareStr) {
                if(idx >= compareStr.Length) {
                    return null;
                }

                compareStr = compareStr.Substring(idx);

                string unmodifiedCompareStr = compareStr;
                compareStr = compareStr == null ? string.Empty : compareStr;
                compareStr = isCaseSensitive ? compareStr : compareStr.ToLower();

                switch (ComparisonOperatorType) {
                    case MpComparisonOperatorType.Regex:
                    case MpComparisonOperatorType.Contains:
                        if (compareStr.Contains(compareData)) {
                            return new MpComparisionMatch(compareData, compareStr.IndexOf(compareData) + idx, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.Exact:
                        if (compareStr.Equals(compareData)) {
                            return new MpComparisionMatch(compareData, 0, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.BeginsWith:
                        if (compareStr.StartsWith(compareData)) {
                            return new MpComparisionMatch(compareData, 0, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.EndsWith:
                        if (compareStr.EndsWith(compareData)) {
                            return new MpComparisionMatch(compareData, compareStr.Length - compareData.Length, compareData.Length);
                        }
                        break;
                } 
            } else if (compareObj is FlowDocument fd) {
                MpWpfRichDocumentExtensions.FindFlags flags = IsCaseSensitive ? MpWpfRichDocumentExtensions.FindFlags.MatchCase : MpWpfRichDocumentExtensions.FindFlags.None;

                var tp = fd.ContentStart.GetPositionAtOffset(idx);

                var tr = tp.FindText(fd.ContentEnd, compareData, flags);

                if(tr != null) {
                    int offset = fd.ContentStart.GetOffsetToPosition(tr.Start);
                    int length = fd.ContentStart.GetOffsetToPosition(tr.End) - offset;
                    return new MpComparisionMatch(compareData, offset, length);
                }

            }

            return null;
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
                 ComparePropertyPathType = (MpCopyItemPropertyPathType)args;
              });

        #endregion
    }
}
