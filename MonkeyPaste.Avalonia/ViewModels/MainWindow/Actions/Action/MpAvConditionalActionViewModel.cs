﻿using MonkeyPaste.Common;
////using Xamarin.Essentials;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvConditionalMatch {
        public string Text { get; private set; } = string.Empty;
        public int Offset { get; private set; } = -1;
        public int Length { get; private set; } = 0;

        public MpAvConditionalMatch(int offset, int length) {
            Offset = offset;
            Length = length;
        }

        public MpAvConditionalMatch(string text, int offset, int length) : this(offset, length) {
            Text = text;
        }

        public override string ToString() {
            return $"OFFSET: {Offset} LENGTH: {Length} TEXT: {Text}";
        }
    }

    public class MpAvConditionalActionViewModel : MpAvActionViewModelBase {
        #region Private Variables
        #endregion

        #region Constants

        public const string SELECTED_COMPARE_PATH_PARAM_ID = "SelectedComparePath";
        public const string COMPARE_FILTER_TEXT_PARAM_ID = "CompareFilterText";
        public const string SELECTED_COMPARE_OP_PARAM_ID = "SelectedCompareOp";
        public const string IS_CASE_SENSITIVE_PARAM_ID = "IsCaseSensitive";
        public const string COMPARE_TEXT_PARAM_ID = "CompareText";
        public const string IS_NOT_COND_PARAM_ID = "IsNotCond";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessComponent _actionComponentFormat;
        public override MpHeadlessComponent ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionCondInPropLabel,
                                controlType = MpParameterControlType.ComponentPicker,
                                unitType = MpParameterValueUnitType.ContentPropertyPathTypeComponentId,
                                isRequired = true,
                                paramId = SELECTED_COMPARE_PATH_PARAM_ID,
                                description = UiStrings.ActionCondInPropHint,
                                value = new MpParameterValueFormat(MpContentQueryPropertyPathType.ClipText.ToQueryFragmentString(),true)
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionCondNotLabel,
                                description = UiStrings.ActionCondNotHint,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = false,
                                paramId = IS_NOT_COND_PARAM_ID
                            },

                            new MpParameterFormat() {
                                label = UiStrings.ActionCondOpLabel,
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = SELECTED_COMPARE_OP_PARAM_ID,
                                description = UiStrings.ActionCondOpHint,
                                values =
                                    typeof(MpComparisonOperatorType)
                                    .GetEnumNames()
                                    .Select((x,idx)=>
                                        new MpParameterValueFormat() {
                                            label = x.ToEnum<MpComparisonOperatorType>().EnumToUiString(),
                                            value = x // NOTE!! 
                                        }
                                    ).ToList()
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionCondDataLabel,
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                isRequired = false,
                                paramId = COMPARE_TEXT_PARAM_ID,
                                description = UiStrings.ActionCondDataHint
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionCondCaseLabel,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = false,
                                paramId = IS_CASE_SENSITIVE_PARAM_ID
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionCondInFilterLabel,
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                isRequired = false,
                                paramId = COMPARE_FILTER_TEXT_PARAM_ID,
                                description = UiStrings.ActionCondInFilterHint
                            },
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

        public override MpActionDesignerShapeType DesignerShapeType =>
            MpActionDesignerShapeType.Diamond;

        public override string ActionHintText =>
            UiStrings.ActionConditionalHint;

        #endregion

        #region State
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
        public bool IsNotCond {
            get {
                if (ArgLookup.TryGetValue(IS_NOT_COND_PARAM_ID, out var param_vm) &&
                    param_vm.BoolValue is bool boolVal) {
                    return boolVal;
                }
                return false;
            }
            set {
                if (IsNotCond != value) {
                    ArgLookup[IS_NOT_COND_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsNotCond));
                }
            }
        }
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

        public MpAvConditionalActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpCompareActionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                await FinishActionAsync(arg);
                return;
            }
            MpAvActionOutput ao = GetInput(arg);

            string compareStr = await GetCompareStr(ao);
            var compareOutput = new MpAvConditionalOutput() {
                Flip = IsNotCond,
                Previous = ao,
                CopyItem = ao.CopyItem,
                Matches = GetMatches(compareStr)
            };

            MpConsole.WriteLine($"Comprarer '{Label}' match result: {compareOutput.WasConditionMet}");
            MpConsole.WriteLine($"Is Not: '{IsNotCond}'");
            MpConsole.WriteLine($"Op: '{ComparisonOperatorType}'");
            MpConsole.WriteLine($"compare string: '{CompareData}'");
            MpConsole.WriteLine($"matches with: '{compareStr.ToStringOrEmpty()}' ");
            MpConsole.WriteLine($"Total: '{compareOutput.Matches.Count}' ");

            await FinishActionAsync(compareOutput);
        }

        #endregion

        #region Protected Overrides

        protected override async Task ValidateActionAndDescendantsAsync() {
            await base.ValidateActionAndDescendantsAsync();
            if (!IsValid) {
                return;
            }
            int focus_arg_num = 0;

            // TODO compare validation will only be needed for last output but not sure, need use case
            //if (!CompareData.IsNullEmptyWhitespaceOrAlphaNumeric()) {
            //    var cdpvm = ArgLookup[COMPARE_TEXT_PARAM_ID];
            //    cdpvm.ValidationMessage = UiStrings.ActionConditionalValidation1;
            //    cdpvm.OnPropertyChanged(nameof(cdpvm.IsValid));
            //    ValidationText = cdpvm.ValidationMessage;
            //    focus_arg_num = ActionArgs.IndexOf(cdpvm);
            //}

            if (ComparePropertyPathType == MpContentQueryPropertyPathType.LastOutput &&
                !IsParentActionSupportLastOutput &&
                ParentActionViewModel != null) {
                ValidationText = string.Format(UiStrings.ActionCondLastOutputUnavailableInvalidText, ParentActionViewModel.Label);
            }
            if (!IsValid) {
                ShowValidationNotification(focus_arg_num);
            }
        }

        protected override void Param_vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.Param_vm_PropertyChanged(sender, e);
            if (sender is not MpAvComponentPickerParameterViewModel cppvm) {
                return;
            }
            switch (e.PropertyName) {
                case nameof(cppvm.ComponentId):
                    ComparePropertyPathType = (MpContentQueryPropertyPathType)cppvm.ComponentId;
                    break;
            }
        }
        #endregion

        #region Private Methods

        private void MpCompareActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
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
            try {
                var copyItemPropObj =
                    await MpPluginParameterValueEvaluator.QueryPropertyAsync(
                        ao.CopyItem,
                        ComparePropertyPathType,
                        new object[] { ao, CompareFilterText });
                return copyItemPropObj.ToStringOrDefault();
            }
            catch (Exception ex) {
                MpConsole.WriteLine(@"Error parsing/querying json response:");
                MpConsole.WriteLine(ao.OutputData.ToString().ToPrettyPrintJson());
                MpConsole.WriteLine(@"For JSONPath: ");
                MpConsole.WriteLine(CompareData);
                MpConsole.WriteTraceLine(ex);

                ValidationText = UiStrings.InvalidCondEx.Format(FullName, ex.Message);
                ShowValidationNotification();
            }
            return null;
        }

        private List<MpAvConditionalMatch> GetMatches(string compareStr) {
            if (compareStr == null) {
                return new();
            }
            object compareObj = null;
            if (compareStr.IsStringRtf()) {
                //compareObj = compareStr.ToFlowDocument();
            } else {
                compareObj = compareStr;
            }
            var matches = new List<MpAvConditionalMatch>();

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
                            MpDebug.Break();
                            break;
                        }
                        matches.Add(match);
                        idx = match.Offset + match.Length + 1;
                    }
                    break;
            }

            return matches;
        }

        private MpAvConditionalMatch GetMatch(object compareObj, string matchStr, int idx = 0) {
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
                            return new MpAvConditionalMatch(compareData, compareStr.IndexOf(compareData) + idx, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.Exact:
                        if (compareStr.Equals(compareData)) {
                            return new MpAvConditionalMatch(compareData, 0, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.BeginsWith:
                        if (compareStr.StartsWith(compareData)) {
                            return new MpAvConditionalMatch(compareData, 0, compareData.Length);
                        }
                        break;
                    case MpComparisonOperatorType.EndsWith:
                        if (compareStr.EndsWith(compareData)) {
                            return new MpAvConditionalMatch(compareData, compareStr.Length - compareData.Length, compareData.Length);
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
