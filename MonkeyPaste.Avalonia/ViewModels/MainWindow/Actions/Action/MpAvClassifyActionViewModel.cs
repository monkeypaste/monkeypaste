using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
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

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Collection",
                                controlType = MpParameterControlType.ComponentPicker,
                                unitType = MpParameterValueUnitType.CollectionComponentId,
                                isRequired = true,
                                paramId = SELECTED_TAG_PARAM_ID,
                                description = "Triggered when content is added to the selected collection"
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
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(ValidateActionAsync);
            }
        }
        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
            if (TagId == 0) {
                ValidationText = $"No Collection selected for Classifier '{FullName}'";
            } else {
                //while (MpAvTagTrayViewModel.Instance.IsAnyBusy) {
                //    await Task.Delay(100);
                //}
                //var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);

                if (SelectedTag == null) {
                    ValidationText = $"Collection for Classifier '{FullName}' not found";
                } else {
                    ValidationText = string.Empty;
                }
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }


        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null && actionInput != null && actionInput.CopyItem != null) {
                ttvm.LinkCopyItemCommand.Execute(actionInput.CopyItem.Id);
            }

            await base.PerformActionAsync(new MpAvClassifyOutput() {
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
