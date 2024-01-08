using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvSetClipboardActionViewModel :
        MpAvActionViewModelBase {
        #region Constants
        const string IGNORE_CHANGE_PARAM_ID = "IgnoreClipboardChange";
        const string CONTENT_TO_SET_PARAM_ID = "ContentToSet";
        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessComponent _actionComponentFormat;
        public override MpHeadlessComponent ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionSetClipboardIgnoreChangeLabel,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = true,
                                paramId = IGNORE_CHANGE_PARAM_ID,
                                value = new MpPluginParameterValueFormat(true.ToString(),true),
                                description = UiStrings.ActionSetClipboardIgnoreChangeHint
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionSetClipboardContentLabel,
                                description = UiStrings.ActionSetClipboardContentHint,
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                paramId = CONTENT_TO_SET_PARAM_ID
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion
        #region Properties

        #region View Models

        #endregion

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionSetClipboardHint;

        #endregion

        #region State
        #endregion

        #region Model
        public string ContentToSet {
            get {
                if (ArgLookup.TryGetValue(CONTENT_TO_SET_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal;
                }
                return string.Empty;
            }
            set {
                if (ContentToSet != value) {
                    ArgLookup[CONTENT_TO_SET_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ContentToSet));
                }
            }
        }
        public bool IgnoreClipboardChange {
            get {
                if (ArgLookup.TryGetValue(IGNORE_CHANGE_PARAM_ID, out var param_vm) &&
                    param_vm.BoolValue is bool curVal) {
                    return curVal;
                }
                return true;
            }
            set {
                if (IgnoreClipboardChange != value) {
                    ArgLookup[IGNORE_CHANGE_PARAM_ID].BoolValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IgnoreClipboardChange));
                }
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvSetClipboardActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
        }
        #endregion

        #region Protected Overrides

        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);
            MpAvDataObject avdo;
            MpCopyItem new_ci = actionInput.CopyItem;

            if (string.IsNullOrWhiteSpace(ContentToSet)) {
                avdo = new_ci.ToAvDataObject(includeSelfRef: true, includeTitle: true);
            } else {
                string evald_content = await MpPluginParameterValueEvaluator
                        .GetParameterRequestValueAsync(
                            MpParameterControlType.TextBox,
                            MpParameterValueUnitType.PlainTextContentQuery,
                            ContentToSet,
                            actionInput.CopyItem,
                            new object[] { actionInput, null });
                avdo = new MpAvDataObject(MpPortableDataFormats.Text, evald_content);
            }

            await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, IgnoreClipboardChange);

            await FinishActionAsync(
                    new MpAvSetClipboardOutput() {
                        Previous = arg as MpAvActionOutput,
                        CopyItem = new_ci,
                        ClipboardDataObject = Mp.Services.ClipboardMonitor.LastClipboardDataObject
                    });
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpFileSystemTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //case nameof(FileSystemPath):
            //    if (IsBusy) {
            //        return;
            //    }
            //    if (IsEnabled.IsTrue()) {
            //        ReEnable().FireAndForgetSafeAsync(this);
            //    }
            //    break;
            //}
        }


        #endregion

        #region Commands

        #endregion
    }
}
