using MonkeyPaste.Common;
////using Xamarin.Essentials;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvComparisionMatch {
        public string Text { get; private set; } = string.Empty;
        public int Offset { get; private set; } = -1;
        public int Length { get; private set; } = 0;

        public MpAvComparisionMatch(int offset, int length) {
            Offset = offset;
            Length = length;
        }

        public MpAvComparisionMatch(string text, int offset, int length) : this(offset, length) {
            Text = text;
        }

        public override string ToString() {
            return $"OFFSET: {Offset} LENGTH: {Length} TEXT: {Text}";
        }
    }

    public class MpAvCompareActionViewModelBase : MpAvActionViewModelBase {
        #region Private Variables
        #endregion

        #region Constants

        public const string SELECTED_COMPARE_PATH_PARAM_ID = "SelectedComparePath";
        public const string COMPARE_FILTER_TEXT_PARAM_ID = "CompareFilterText";
        public const string SELECTED_COMPARE_OP_PARAM_ID = "SelectedCompareOp";
        public const string IS_CASE_SENSITIVE_PARAM_ID = "IsCaseSensitive";
        public const string COMPARE_TEXT_PARAM_ID = "CompareText";

        #endregion

        #region MpIParameterHost Overrides

        private MpActionPluginFormat _actionComponentFormat;
        public override MpActionPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpActionPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Property",
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = SELECTED_COMPARE_PATH_PARAM_ID,
                                description = "",
                                values =
                                    typeof(MpContentQueryPropertyPathType)
                                    .GetEnumNames()
                                    .Select(x=>
                                        new MpPluginParameterValueFormat() {
                                            label = x.ToLabel(),
                                            value = x
                                        }
                                    ).ToList()
                            },new MpParameterFormat() {
                                label = "Filter",
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                isRequired = false,
                                paramId = COMPARE_FILTER_TEXT_PARAM_ID,
                                description = ""
                            },new MpParameterFormat() {
                                label = "Operation",
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = SELECTED_COMPARE_OP_PARAM_ID,
                                description = "",
                                values =
                                    typeof(MpComparisonOperatorType)
                                    .GetEnumNames()
                                    .Select(x=>
                                        new MpPluginParameterValueFormat() {
                                            label = x.ToLabel(),
                                            value = x
                                        }
                                    ).ToList()
                            },new MpParameterFormat() {
                                label = "Case Sensitive?",
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = false,
                                paramId = IS_CASE_SENSITIVE_PARAM_ID,
                                description = ""
                            },new MpParameterFormat() {
                                label = "Data",
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                isRequired = false,
                                paramId = COMPARE_TEXT_PARAM_ID,
                                description = ""
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

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
                    case MpContentQueryPropertyPathType.ItemData:
                    case MpContentQueryPropertyPathType.ItemType:
                    case MpContentQueryPropertyPathType.Title:
                    case MpContentQueryPropertyPathType.CopyDateTime:
                    case MpContentQueryPropertyPathType.CopyCount:
                    case MpContentQueryPropertyPathType.PasteCount:
                        return ComparePropertyPathType.ToString();
                    case MpContentQueryPropertyPathType.AppName:
                    case MpContentQueryPropertyPathType.AppPath:
                        return string.Format(@"Source.App.{0}", ComparePropertyPathType.ToString());
                    case MpContentQueryPropertyPathType.UrlPath:
                    case MpContentQueryPropertyPathType.UrlTitle:
                    case MpContentQueryPropertyPathType.UrlDomainPath:
                        return string.Format(@"Source.App.{0}", ComparePropertyPathType.ToString());
                }
                return string.Empty;
            }
        }

        #endregion

        #region State

        public bool HasInputFilter => !string.IsNullOrWhiteSpace(CompareFilterText);

        public bool IsItemTypeCompare => ComparePropertyPathType == MpContentQueryPropertyPathType.ItemType;

        public bool IsLastOutputCompare => ComparePropertyPathType == MpContentQueryPropertyPathType.LastOutput;


        public bool IsCompareTypeRegex => ComparisonOperatorType == MpComparisonOperatorType.Regex;

        #endregion

        #region Model
        //Arg4
        public string CompareFilterText {
            get {
                if (ArgLookup.TryGetValue(COMPARE_FILTER_TEXT_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal;
                }
                return string.Empty;
            }
            set {
                if (CompareFilterText != value) {
                    ArgLookup[COMPARE_FILTER_TEXT_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareFilterText));
                }
            }
        }

        //Arg3
        public bool IsCaseSensitive {
            get {
                if (ArgLookup.TryGetValue(IS_CASE_SENSITIVE_PARAM_ID, out var param_vm) &&
                    param_vm.BoolValue is bool boolVal) {
                    return boolVal;
                }
                return false;
            }
            set {
                if (IsCaseSensitive != value) {
                    ArgLookup[IS_CASE_SENSITIVE_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsCaseSensitive));
                }
            }
        }

        //Arg2
        //public MpCopyItemType ContentItemType {
        //    get {
        //        if (Action == null) {
        //            return 0;
        //        }
        //        if (ComparePropertyPathType != MpContentQueryPropertyPathType.ItemType) {
        //            return 0;
        //        }
        //        if (string.IsNullOrWhiteSpace(Arg2)) {
        //            return MpCopyItemType.None;
        //        }
        //        return Arg2.ToEnum<MpCopyItemType>();
        //    }
        //    set {
        //        if (ContentItemType != value) {
        //            Arg2 = value.ToString();
        //            HasModelChanged = true;
        //            OnPropertyChanged(nameof(ContentItemType));
        //        }
        //    }
        //}

        public string CompareData {
            get {
                if (ArgLookup.TryGetValue(COMPARE_TEXT_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal;
                }
                return string.Empty;
            }
            set {
                if (CompareData != value) {
                    ArgLookup[COMPARE_TEXT_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareData));
                }
            }
        }

        // Arg1
        public MpContentQueryPropertyPathType ComparePropertyPathType {
            get {
                if (ArgLookup.TryGetValue(SELECTED_COMPARE_PATH_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal &&
                    curVal.ToEnum<MpContentQueryPropertyPathType>() is MpContentQueryPropertyPathType pathType) {
                    return pathType;
                }
                return MpContentQueryPropertyPathType.None;
            }
            set {
                if (ComparePropertyPathType != value) {
                    ArgLookup[SELECTED_COMPARE_PATH_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComparePropertyPathType));
                }
            }
        }

        // Arg
        public MpComparisonOperatorType ComparisonOperatorType {
            get {
                if (ArgLookup.TryGetValue(SELECTED_COMPARE_OP_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal &&
                    curVal.ToEnum<MpComparisonOperatorType>() is MpComparisonOperatorType opType) {
                    return opType;
                }
                return MpComparisonOperatorType.Contains;
            }
            set {
                if (ComparisonOperatorType != value) {
                    ArgLookup[SELECTED_COMPARE_OP_PARAM_ID].CurrentValue = value.ToString();
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
            if (compareStr == null) {
                return;
            }
            MpConsole.WriteLine($"Comprarer '{Label}' match result:");
            MpConsole.WriteLine($"Op: '{ComparisonOperatorType}'");
            MpConsole.WriteLine($"compare string: '{CompareData}'");

            var compareOutput = new MpAvCompareOutput() {
                Previous = ao,
                CopyItem = ao.CopyItem
            };

            compareOutput.Matches = GetMatches(compareStr);
            MpConsole.WriteLine($"matches with: '{compareStr}' ");
            MpConsole.WriteLine($"Total: '{compareOutput.Matches.Count}' ");

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
            if (ao == null) {
                return null;
            }
            if (ComparePropertyPathType == MpContentQueryPropertyPathType.LastOutput) {
                if (ao.OutputData is MpPluginResponseFormatBase prf && HasInputFilter) {
                    try {
                        return MpJsonPathProperty.Query(prf, CompareFilterText);
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
                } else if (ao.OutputData != null) {
                    return ao.OutputData.ToString();
                }
            } else {
                var copyItemPropObj = await MpPluginParameterValueEvaluator.QueryPropertyAsync(ao.CopyItem, ComparePropertyPathType);
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

        private MpAvComparisionMatch GetMatch(object compareObj, string matchStr, int idx = 0) {
            bool isCaseSensitive = IsCaseSensitive;
            string compareData = matchStr;
            if (compareData == null) {
                compareData = string.Empty;
            }
            compareData = isCaseSensitive ? compareData : compareData.ToLower();

            if (compareObj is string compareStr) {
                if (idx >= compareStr.Length) {
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
