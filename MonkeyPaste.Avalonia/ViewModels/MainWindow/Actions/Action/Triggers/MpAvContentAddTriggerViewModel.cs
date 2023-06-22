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
                                label = "Trigger",
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = CONTENT_TYPE_PARAM_ID,
                                description = "Content (not meeting rejection criteria) of this type will trigger this action.",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        label = "All",
                                        value = MpCopyItemType.None.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = "Text",
                                        value = MpCopyItemType.Text.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = "Image",
                                        value = MpCopyItemType.Image.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = "Files",
                                        value = MpCopyItemType.FileList.ToString()
                                    },
                                }
                            },
                            new MpParameterFormat() {
                                label = "Ignore Duplicate",
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = false,
                                paramId = IGNORE_DUP_CONTENT_PARAM_ID,
                                description = "Only execute this trigger if clipboard is new and not been already copied and processed. This is independant of any preferene setting of whether new content is ignored or not.",
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
            "Content Added - Triggered when content of the selected type is added";

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

        protected override void EnableTrigger() {
            MpAvClipTrayViewModel.Instance.RegisterActionComponent(this);
        }

        protected override void DisableTrigger() {
            MpAvClipTrayViewModel.Instance.UnregisterActionComponent(this);
        }

        protected override bool ValidateStartAction(object arg) {
            if (!base.ValidateStartAction(arg)) {
                return false;
            }
            if (AddedContentType == MpCopyItemType.None) {
                // NOTE Default is treated as all types
                return true;
            }
            if (arg is MpCopyItem ci) {
                if (ci.ItemType != AddedContentType) {
                    return false;
                }
                if (ci.WasDupOnCreate && IgnoreDupContent) {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
