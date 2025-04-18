﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentTaggedTriggerViewModel :
        MpAvTriggerActionViewModelBase {


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
                                label = UiStrings.ActionContentTaggedTriggerLabel,
                                controlType = MpParameterControlType.ComponentPicker,
                                unitType = MpParameterValueUnitType.CollectionComponentId,
                                isRequired = true,
                                paramId = SELECTED_TAG_PARAM_ID,
                                description = UiStrings.ActionContentTaggedTriggerHint
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
            UiStrings.ActionContentTaggedHint;

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
                Task.Run(ValidateActionAndDescendantsAsync);
            }
        }
        protected override async Task ValidateActionAndDescendantsAsync() {
            await base.ValidateActionAndDescendantsAsync();
            if (!IsValid) {
                return;
            }
            if (TagId == 0) {
                //ValidationText = $"No Collection selected for Classify Trigger '{FullName}'";
                ValidationText = string.Format(UiStrings.ActionContentTaggedValidation1, FullName);
            } else {
                if (SelectedTag == null) {
                    //ValidationText = $"Collection for Classify Trigger '{FullName}' not found";
                    ValidationText = string.Format(UiStrings.ActionContentTaggedValidation2, FullName);
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
