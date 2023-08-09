using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentTaggedTriggerViewModel :
        MpAvTriggerActionViewModelBase {


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

        #region State
        protected override MpIActionComponent TriggerComponent =>
            SelectedTag;

        #endregion

        #region Appearance
        public override string ActionHintText =>
            "Content Classified - Triggered when content is added to the selected collection";

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

        public MpAvContentTaggedTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvContentTaggedTriggerViewModel_PropertyChanged;
        }

        #endregion

        #region Protected Methods
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(ValidateActionAsync);
            }
        }
        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return;
            }
            if (TagId == 0) {
                ValidationText = $"No Collection selected for Classify Trigger '{FullName}'";
            } else {
                //while (MpAvTagTrayViewModel.Instance.IsAnyBusy) {
                //    await Task.Delay(100);
                //}
                //var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);

                if (SelectedTag == null) {
                    ValidationText = $"Collection for Classify Trigger '{FullName}' not found";
                } else {
                    ValidationText = string.Empty;
                }
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }

        #endregion

        #region Private Methods
        private void MpAvContentTaggedTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasArgsChanged):
                    //OnPropertyChanged(nameof(TagId));
                    break;
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
