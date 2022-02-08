using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpTriggerActionViewModelBase : 
        MpActionViewModelBase,
        MpIResizableViewModel,
        MpISidebarItemViewModel,
        MpIMenuItemViewModel {
        #region Properties

        #region View Models

        public ObservableCollection<MpActionViewModelBase> AllChildren => new ObservableCollection<MpActionViewModelBase>(this.FindAllChildren());

        #endregion

        #region MpISidebarItemViewModel Implementation

        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultDesignerWidth;
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultDesignerWidth;

        public bool IsSidebarVisible { get; set; }
        public MpISidebarItemViewModel NextSidebarItem => null;
        public MpISidebarItemViewModel PreviousSidebarItem => Parent;

        #endregion

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel TriggerTypeMenuItemViewModel {
            get {
                var tmivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpTriggerType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    switch ((MpTriggerType)i) {
                        case MpTriggerType.ContentAdded:
                            resourceKey = "ClipboardIcon";
                            break;
                        case MpTriggerType.ContentTagged:
                            resourceKey = "PinToCollectionIcon";
                            break;
                        case MpTriggerType.FileSystemChange:
                            resourceKey = "FolderEventIcon";
                            break;
                        case MpTriggerType.Shortcut:
                            resourceKey = "HotkeyIcon";
                            break;
                        case MpTriggerType.ParentOutput:
                            resourceKey = "ChainIcon";
                            break;
                    }
                    var tt = (MpTriggerType)i;
                    tmivml.Add(new MpMenuItemViewModel() {
                        IsSelected = tt == TriggerType,
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = SelectTriggerTypeCommand,
                        CommandParameter = tt,
                        IsVisible = tt != MpTriggerType.None && (tt != MpTriggerType.ParentOutput)// || (SelectedItem != null && !SelectedItem.IsRootAction))
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = tmivml
                };
            }
        }

        #endregion

        #region Appearance

        #endregion

        #region State

        #endregion

        #region Model

        public MpTriggerType TriggerType {
            get {
                if (Action == null) {
                    return MpTriggerType.None;
                }
                return (MpTriggerType)Action.ActionObjId;
            }
            set {
                if (TriggerType != value) {
                    Action.ActionObjId = (int)value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TriggerType));
                }
            }
        }

        public string Description {
            get {
                if(Action == null) {
                    return string.Empty;
                }
                return Action.Description;
            }
            set {
                if(Description != value) {
                    Action.Description = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpTriggerActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpTriggerActionViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Mehthods

        public override void Validate() {
            if(TriggerType != MpTriggerType.ParentOutput && string.IsNullOrWhiteSpace(Label)) {
                ValidationText = "Trigger Action Must Have Name";
            }
        }

        #endregion

        #region Private Methods

        private void MpTriggerActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    break;
            }
        }

        #endregion

        #region Commands


        public ICommand SelectTriggerTypeCommand => new RelayCommand<object>(
            async (args) => {
                ///IsDropDownOpen = false;

                TriggerType = (MpTriggerType)args;

                var thisTrigger = Parent.Items.FirstOrDefault(x => x.ActionId == ActionId);
                if (thisTrigger != null) {
                    MpHelpers.RunOnMainThread(async () => {
                        thisTrigger = await Parent.CreateTriggerViewModel(Action);
                    });
                }
            });
        #endregion
    }
}
