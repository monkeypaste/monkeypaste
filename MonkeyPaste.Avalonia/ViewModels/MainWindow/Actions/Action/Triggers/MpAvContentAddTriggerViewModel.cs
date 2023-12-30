using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public interface MpIContentTypeDependant {
        bool IsContentTypeValid(MpCopyItemType cit);
    }
    public class MpAvContentAddTriggerViewModel :
        MpAvTriggerActionViewModelBase, MpIContentTypeDependant {
        #region Constants

        public const string CONTENT_TYPE_PARAM_ID = "SelectedContentType";
        public const string IGNORE_DUP_CONTENT_PARAM_ID = "IgnoreDupContent";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.TriggerLabel,
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = CONTENT_TYPE_PARAM_ID,
                                description = UiStrings.ActionContentAddTriggerHint,
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        label = UiStrings.ActionContentAddAllLabel,
                                        value = MpCopyItemType.None.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = UiStrings.ClipTileDefTitleTextPrefix,
                                        value = MpCopyItemType.Text.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = UiStrings.ClipTileDefTitleImagePrefix,
                                        value = MpCopyItemType.Image.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = UiStrings.ClipTileDefTitleFilesPrefix,
                                        value = MpCopyItemType.FileList.ToString()
                                    },
                                }
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionContentAddIgnoreDupLabel,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = false,
                                paramId = IGNORE_DUP_CONTENT_PARAM_ID,
                                description = UiStrings.ActionContentAddIgnoreDupHint,
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = true.ToString()
                                    }
                                }
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Interfaces

        #region MpIContentTypeDependant Implementation

        bool MpIContentTypeDependant.IsContentTypeValid(MpCopyItemType cit) {
            if (AddedContentType == MpCopyItemType.None) {
                return true;
            }
            return AddedContentType == cit;
        }

        #endregion

        #endregion

        #region Properties

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionContentAddHint;

        #endregion

        #region State
        protected override MpIActionComponent TriggerComponent =>
            MpAvClipTrayViewModel.Instance;

        #endregion

        #region Model

        public MpCopyItemType AddedContentType {
            get {
                if (ArgLookup.TryGetValue(CONTENT_TYPE_PARAM_ID, out var param_vm) &&
                    param_vm.CurrentValue is string curVal) {
                    return curVal.ToEnum<MpCopyItemType>();
                }
                return MpCopyItemType.None;
            }
            set {
                if (AddedContentType != value) {
                    ArgLookup[CONTENT_TYPE_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AddedContentType));
                }
            }
        }

        public bool IgnoreDupContent {
            get {
                if (ArgLookup.TryGetValue(IGNORE_DUP_CONTENT_PARAM_ID, out var param_vm) &&
                     param_vm.CurrentValue.ParseOrConvertToBool(true) is bool boolVal) {
                    return boolVal;
                }
                return true;
            }
            set {
                if (IgnoreDupContent != value) {
                    ArgLookup[IGNORE_DUP_CONTENT_PARAM_ID].CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IgnoreDupContent));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvContentAddTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Methods

        protected override bool ValidateStartAction(object arg, bool is_starting = true) {
            bool can_start = base.ValidateStartAction(arg, is_starting);
            if (can_start &&
                AddedContentType != MpCopyItemType.None &&
                arg is MpCopyItem ci) {
                // NOTE None is treated as all types
                if (ci.ItemType != AddedContentType) {
                    can_start = false;
                }
                if (ci.WasDupOnCreate && IgnoreDupContent) {
                    can_start = false;
                }
            }
            if (!can_start && is_starting) {
                IsPerformingAction = false;
            }
            return can_start;
        }

        #endregion
    }
}
