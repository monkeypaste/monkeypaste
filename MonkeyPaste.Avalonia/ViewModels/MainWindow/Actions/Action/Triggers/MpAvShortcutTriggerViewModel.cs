using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutTriggerViewModel :
        MpAvTriggerActionViewModelBase,

        MpIApplicationCommandCollectionViewModel {

        #region Constants

        public const string CURRENT_SHORTCUT_PARAM_ID = "SelectedTagId";

        #endregion

        #region Interfaces


        #region MpAvIApplicationCommandViewModel Implementation

        public IEnumerable<MpApplicationCommand> Commands =>
            new MpApplicationCommand[] {
                new MpApplicationCommand() {
                    Command = MpAvTriggerCollectionViewModel.Instance.InvokeActionCommand,
                    CommandParameter = ActionId,
                    Tag = CURRENT_SHORTCUT_PARAM_ID
                }
            };

        #endregion


        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Shortcut",
                                controlType = MpParameterControlType.ShortcutRecorder,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = CURRENT_SHORTCUT_PARAM_ID,
                                description = "Triggered when the recorded shortcut is pressed at anytime with the current clipboard"
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

        public MpAvShortcutViewModel ShortcutViewModel {
            get {
                if (ArgLookup.TryGetValue(CURRENT_SHORTCUT_PARAM_ID, out var param_vm) &&
                    param_vm is MpAvShortcutRecorderParameterViewModel scpvm &&
                    MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcut(scpvm) is MpAvShortcutViewModel svm) {
                    return svm;
                }
                return null;
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvShortcutTriggerViewModel_PropertyChanged;
        }


        #endregion

        #region Protected Methods

        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
            if (ShortcutViewModel == null) {
                ValidationText = $"Shortcut for Trigger '{FullName}' not found";
            } else {

                //if (IsPerformingActionFromCommand) {
                //    if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                //        if (MpAvClipTrayViewModel.Instance.SelectedModels == null ||
                //       MpAvClipTrayViewModel.Instance.SelectedModels.Count == 0) {
                //            ValidationText = $"No content selected, cannot execute '{FullName}' ";
                //        }
                //    }
                //}
                ValidationText = string.Empty;
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }

        protected override void EnableTrigger() {
            if (ShortcutViewModel == null) {
                return;
            }
            ShortcutViewModel.RegisterActionComponent(this);
        }

        protected override void DisableTrigger() {
            if (ShortcutViewModel == null) {
                return;
            }
            ShortcutViewModel.UnregisterActionComponent(this);
        }

        #endregion

        #region Private Methods


        private void MpAvShortcutTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {

            //}
        }

        #endregion

        #region Commands





        #endregion
    }
}
