using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutTriggerViewModel :
        MpAvTriggerActionViewModelBase,
        //MpIShortcutCommandViewModel,
        //MpAvIKeyGestureViewModel,
        MpIApplicationCommandCollectionViewModel {

        #region Constants

        public const string CURRENT_SHORTCUT_PARAM_ID = "TriggerShortcutId";

        #endregion

        #region Interfaces

        //#region MpAvIKeyGestureViewModel Implementation

        //public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
        //    new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyString.ToKeyItems());
        //#endregion

        //#region MpIShortcutCommandViewModel Implementation
        //public MpShortcutType ShortcutType =>
        //    MpShortcutType.InvokeTrigger;

        //public string KeyString =>
        //    MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);

        //public object ShortcutCommandParameter =>
        //    ActionId;
        //ICommand MpIShortcutCommandViewModel.ShortcutCommand =>
        //    InvokeThisActionCommand;
        //#endregion


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
                                unitType = MpParameterValueUnitType.Integer,
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

        public MpAvShortcutViewModel ShortcutViewModel =>
            MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
        #endregion

        #region State

        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model

        public int ShortcutId {
            get {
                if (ArgLookup.TryGetValue(CURRENT_SHORTCUT_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (ShortcutId != value) {
                    ArgLookup[CURRENT_SHORTCUT_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvShortcutTriggerViewModel_PropertyChanged;
            ActionArgs.CollectionChanged += ActionArgs_CollectionChanged;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            //if(e is MpShortcut s && s.Id == ShortcutId) {
            //    if (ArgLookup[CURRENT_SHORTCUT_PARAM_ID] is MpAvShortcutRecorderParameterViewModel scrpvm) {
            //        scrpvm.OnPropertyChanged(nameof())
            //    }
            //}
        }
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut s && s.Id == ShortcutId) {
                Task.Run(ValidateActionAsync);
            }
        }
        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            //if (!IsValid) {
            //    return;
            //}
            //if (ShortcutViewModel == null) {
            //    ValidationText = $"Shortcut for Trigger '{FullName}' not found";
            //} else {

            //    ValidationText = string.Empty;
            //}
            //if (!IsValid) {
            //    ShowValidationNotification();
            //}
        }

        protected override void EnableTrigger() {
            //if (ShortcutViewModel == null) {
            //    return;
            //}
            //ShortcutViewModel.RegisterActionComponent(this);

            // NOTE nothing to enable
            // invoke is handled by shortcut listener and invokeCmd.CanExec
        }

        protected override void DisableTrigger() {
            //if (ShortcutViewModel == null) {
            //    return;
            //}
            //ShortcutViewModel.UnregisterActionComponent(this);
        }

        #endregion

        #region Private Methods


        private void MpAvShortcutTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    OnPropertyChanged(nameof(ShortcutViewModel));
                    if (ArgLookup[CURRENT_SHORTCUT_PARAM_ID] is MpAvShortcutRecorderParameterViewModel scrpvm) {
                        scrpvm.OnPropertyChanged(nameof(scrpvm.KeyGroups));
                    }
                    break;
            }
        }

        private void ActionArgs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (MpAvParameterViewModelBase pvm in e.NewItems) {
                    if (pvm is MpAvShortcutRecorderParameterViewModel srpvm) {
                        srpvm.ShortcutCommand = Parent.InvokeActionCommand;
                        srpvm.ShortcutCommandParameter = ActionId;
                        srpvm.ShortcutType = MpShortcutType.InvokeTrigger;
                        srpvm.OnPropertyChanged(nameof(srpvm.KeyString));
                    }
                }
            }
        }

        #endregion

        #region Commands


        #endregion
    }
}
