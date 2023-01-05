
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
using MonkeyPaste.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//using Xamarin.Essentials;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public class MpAvComparisionMatch {
        public string Text { get; private set; } = string.Empty;
        public int Offset { get; private set; } = -1;
        public int Length { get; private set; } = 0;

        public MpAvComparisionMatch(int offset, int length) {
            Offset = offset;
            Length = length;
        }

        public MpAvComparisionMatch(string text, int offset, int length) : this(offset,length) {
            Text = text;
        }

        public override string ToString() {
            return $"OFFSET: {Offset} LENGTH: {Length} TEXT: {Text}";
        }
    }

    public class MpAvCompareActionViewModelBase : MpAvActionViewModelBase {
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
                return Arg2.ToEnum<MpCopyItemType>();
            }
            set {
                if (ContentItemType != value) {
                    Arg2 = value.ToString();
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

                return Arg1.ToEnum<MpCopyItemPropertyPathType>();
            }
            set {
                if (ComparePropertyPathType != value) {
                    Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComparePropertyPathType));
                }
            }
        }

        // Arg
        public MpComparisonOperatorType ComparisonOperatorType {
            get {
                var cot = Arg5.ToEnum<MpComparisonOperatorType>(notFoundValue: MpComparisonOperatorType.Contains);
                if(cot == MpComparisonOperatorType.None) {
                    cot = MpComparisonOperatorType.Contains;
                    Action.Arg5 = cot.ToString();
                }
                return cot;
            }
            set {
                if (ComparisonOperatorType != value) {
                    Action.Arg5 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComparisonOperatorType));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvCompareActionViewModelBase(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpCompareActionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }
            MpAvActionOutput ao = GetInput(arg);
            
            string compareStr = await GetCompareStr(ao);
            if(compareStr == null) {
                return;
            }
            var compareOutput = new MpAvCompareOutput() {
                Previous = ao,
                CopyItem = ao.CopyItem
            };

            compareOutput.Matches = GetMatches(compareStr);

            if (compareOutput.Matches != null && compareOutput.Matches.Count > 0) {
                base.PerformActionAsync(compareOutput).FireAndForgetSafeAsync(this);
            }
        }

        #endregion

        #region Protected Overrides

        protected override async Task ValidateActionAsync() {
            // TODO compare validation will only be needed for last output but not sure, need use case
            await Task.Delay(1);
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

        private async Task<string> GetCompareStr(MpAvActionOutput ao) {
            if(ao == null) {
                return null;
            }
            if (ComparePropertyPathType == MpCopyItemPropertyPathType.LastOutput) {
                if (ao != null) {
                    if (ao.OutputData is MpPluginResponseFormatBase prf && IsJsonQuery) {
                        try {
                            return MpJsonPathProperty.Query(prf, CompareDataJsonPath);
                        }
                        catch (Exception ex) {
                            MpConsole.WriteLine(@"Error parsing/querying json response:");
                            MpConsole.WriteLine(ao.OutputData.ToString().ToPrettyPrintJson());
                            MpConsole.WriteLine(@"For JSONPath: ");
                            MpConsole.WriteLine(CompareData);
                            MpConsole.WriteTraceLine(ex);

                            ValidationText = $"Error performing action '{RootTriggerActionViewModel.Label}/{Label}': {ex}";
                            ShowValidationNotification();
                        }
                    } else if(ao.OutputData != null) {
                        return ao.OutputData.ToString();
                    }
                }
            } else {
                var copyItemPropObj =  await MpCopyItem.QueryProperty(ao.CopyItem, ComparePropertyPathType);
                if (copyItemPropObj != null) {
                    return copyItemPropObj.ToString();
                }
            }
            return null;
        }

        private List<MpAvComparisionMatch> GetMatches(string compareStr) {
            object compareObj = null;
            if (compareStr.IsStringRichText()) {
                //compareObj = compareStr.ToFlowDocument();
            } else {
                compareObj = compareStr;
            }

            var matches = new List<MpAvComparisionMatch>();
            int idx = 0;
            switch (ComparisonOperatorType) {
                case MpComparisonOperatorType.Contains:
                case MpComparisonOperatorType.Exact:
                case MpComparisonOperatorType.BeginsWith:
                case MpComparisonOperatorType.EndsWith:
                    while (true) {
                        var subMatch = GetMatch(compareObj, CompareData, idx);
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
                        var match = GetMatch(compareObj, m.Value, idx);
                        if (match == null) {
                            Debugger.Break();
                            break;
                        }
                        matches.Add(match);
                        idx = match.Offset + match.Length + 1;
                    }
                    break;
            }

            return matches;
        }

        private MpAvComparisionMatch GetMatch(object compareObj,string matchStr, int idx = 0) {
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
                            return new MpAvComparisionMatch(compareData, compareStr.IndexOf(compareData) + idx, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.Exact:
                        if (compareStr.Equals(compareData)) {
                            return new MpAvComparisionMatch(compareData, 0, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.BeginsWith:
                        if (compareStr.StartsWith(compareData)) {
                            return new MpAvComparisionMatch(compareData, 0, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.EndsWith:
                        if (compareStr.EndsWith(compareData)) {
                            return new MpAvComparisionMatch(compareData, compareStr.Length - compareData.Length, compareData.Length);
                        }
                        break;
                } 
            } 
            //else if (compareObj is FlowDocument fd) {
            //    var tp = fd.ContentStart.GetPositionAtOffset(idx);
            //    if(tp == null) {
            //        return null;
            //    }

            //    var tr = tp.FindText(fd.ContentEnd, compareData, IsCaseSensitive);

            //    if(tr != null) {
            //        int offset = fd.ContentStart.GetOffsetToPosition(tr.Start);
            //        int length = fd.ContentStart.GetOffsetToPosition(tr.End) - offset;
            //        return new MpAvComparisionMatch(compareData, offset, length);
            //    }

            //}

            return null;
        }


        #endregion

        #region Commands
        #endregion
    }
}
