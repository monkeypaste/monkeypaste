using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClassifyActionViewModel :
        MpAvActionViewModelBase {
        #region Private Variables

        #endregion

        #region Constants

        public const string SELECTED_TAG_PARAM_ID = "SelectedTagId";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessComponent _actionComponentFormat;
        public override MpHeadlessComponent ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionClassifyLabel,
                                controlType = MpParameterControlType.ComponentPicker,
                                unitType = MpParameterValueUnitType.CollectionComponentId,
                                isRequired = true,
                                paramId = SELECTED_TAG_PARAM_ID,
                                description = UiStrings.ActionClassifyActionHint
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

        public MpAvTagTileViewModel SelectedTag =>
            MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);

        #endregion

        #region Appearance
        public override object IconResourceObj => SelectedTag == null ?
            base.IconResourceObj : SelectedTag.TagHexColor;
        public override string ActionHintText =>
            UiStrings.ActionClassifyHint;

        #endregion

        #region Model

        public int TagId {
            get {
                if (ArgLookup.TryGetValue(SELECTED_TAG_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (TagId != value) {
                    ArgLookup[SELECTED_TAG_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvClassifyActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvClassifyActionViewModel_PropertyChanged;
        }


        #endregion

        #region Protected Overrides
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is not MpTag t) {
                return;
            }
            if (ArgLookup.TryGetValue(SELECTED_TAG_PARAM_ID, out var pvmb)) {
                pvmb.InitializeAsync(pvmb.ParameterValue).FireAndForgetSafeAsync();
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is not MpTag t) {
            }
            if (ArgLookup.TryGetValue(SELECTED_TAG_PARAM_ID, out var pvmb)) {
                pvmb.InitializeAsync(pvmb.ParameterValue).FireAndForgetSafeAsync();
            }
        }
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Dispatcher.UIThread.Post(async () => {
                    await ValidateActionAsync();
                });
            }
        }
        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return;
            }
            if (TagId == 0) {
                //ValidationText = $"No Collection selected for Classifier '{FullName}'";
                ValidationText = string.Format(UiStrings.ActionClassifyValidation1, FullName);
            } else {
                if (SelectedTag == null) {
                    //ValidationText = $"Collection for Classifier '{FullName}' not found";
                    ValidationText = string.Format(UiStrings.ActionClassifyValidation2, FullName);
                } else {
                    ValidationText = string.Empty;
                }
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }
        protected override void Param_vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.Param_vm_PropertyChanged(sender, e);
            if (sender is not MpAvComponentPickerParameterViewModel cppvm) {
                return;
            }
            switch (e.PropertyName) {
                case nameof(cppvm.ComponentId):
                    TagId = cppvm.ComponentId;
                    break;
            }
        }

        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null && actionInput != null && actionInput.CopyItem != null) {
                ttvm.LinkCopyItemCommand.Execute(actionInput.CopyItem.Id);
            }

            await FinishActionAsync(new MpAvClassifyOutput() {
                Previous = arg as MpAvActionOutput,
                CopyItem = actionInput.CopyItem,
                TagId = TagId
            });
        }

        #endregion

        #region Private Methods


        private void MpAvClassifyActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    OnPropertyChanged(nameof(SelectedTag));
                    break;
                case nameof(SelectedTag):
                    OnPropertyChanged(nameof(IconResourceObj));
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
