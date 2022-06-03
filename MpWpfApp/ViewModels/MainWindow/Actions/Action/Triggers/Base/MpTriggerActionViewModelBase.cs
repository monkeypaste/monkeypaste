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
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpTriggerInput : MpActionOutput {
        public override object OutputData => CopyItem;
        public override string ActionDescription => "Trigger Activated...";
    }

    public class MpTriggerActionViewModelBase : 
        MpActionViewModelBase,
        MpIResizableViewModel,
        MpISidebarItemViewModel,
        MpIMenuItemViewModel,
        MpIDesignerItemSettingsViewModel {
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

        public bool IsContentAddTrigger => TriggerType == MpTriggerType.ContentAdded;

        public bool IsFileSystemTrigger => TriggerType == MpTriggerType.FileSystemChange;

        #endregion

        #region Model


        #region MpIDesignerItemSettingsViewModel Implementation

        private void SetDesignerItemSettings(double scaleX, double scaleY, double offsetX, double offsetY) {
            string arg2 = string.Join(",", new string[] { scaleX.ToString(), scaleY.ToString(), offsetX.ToString(), offsetY.ToString() });
            Arg2 = arg2;
        }

        private MpPoint ParseScale(string text) {
            if(string.IsNullOrEmpty(Arg2)) {
                return new MpPoint(1,1);
            }
            var arg2Parts = Arg2.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            MpPoint scale = new MpPoint(1,1);
            try {
                scale.X = Convert.ToDouble(arg2Parts[0]);
                scale.Y = Convert.ToDouble(arg2Parts[1]);
            }catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error parsing scale from arg2: '{Arg2}'");
                MpConsole.WriteLine(ex);
            }
            return scale;
        }

        private MpPoint ParseTranslationOffset(string text) {
            if (string.IsNullOrEmpty(Arg2)) {
                return new MpPoint();
            }
            var arg2Parts = Arg2.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            MpPoint offset = new MpPoint();
            try {
                offset.X = Convert.ToDouble(arg2Parts[2]);
                offset.Y = Convert.ToDouble(arg2Parts[3]);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error parsing offset from arg2: '{Arg2}'");
                MpConsole.WriteLine(ex);
            }
            return offset;
        }

        public double ScaleX {
            get {
                return ParseScale(Arg2).X;
            }
            set {
                if(ScaleX != value) {
                    SetDesignerItemSettings(value, ScaleY, TranslateOffsetX, TranslateOffsetY);
                    OnPropertyChanged(nameof(ScaleX));
                }
            }
        }
        public double ScaleY {
            get {
                return ParseScale(Arg2).Y;
            }
            set {
                if (ScaleY != value) {
                    SetDesignerItemSettings(ScaleX, value, TranslateOffsetX, TranslateOffsetY);
                    OnPropertyChanged(nameof(ScaleY));
                }
            }
        }

        public double TranslateOffsetX {
            get {
                return ParseTranslationOffset(Arg2).X;
            }
            set {
                if (TranslateOffsetX != value) {
                    SetDesignerItemSettings(ScaleX, ScaleY, value, TranslateOffsetY);
                    OnPropertyChanged(nameof(TranslateOffsetX));
                }
            }
        }
        public double TranslateOffsetY {
            get {
                return ParseTranslationOffset(Arg2).Y;
            }
            set {
                if (TranslateOffsetY != value) {
                    SetDesignerItemSettings(ScaleX, ScaleY, TranslateOffsetX, value);
                    OnPropertyChanged(nameof(TranslateOffsetY));
                }
            }
        }


        #endregion

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

        #endregion

        #endregion

        #region Constructors

        public MpTriggerActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpTriggerActionViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
        }

        protected async Task ShowUserEnableChangeNotification() {
            string enabledText = IsEnabled.HasValue && IsEnabled.Value ?
                                    "ENABLED" :
                                    "DISABLED";
            string notificationText = $"Action '{FullName}' is now  {enabledText}";
            MpMainWindowViewModel.Instance.IsShowingDialog = MpMainWindowViewModel.Instance.IsMainWindowOpen;

            await MpNotificationCollectionViewModel.Instance.ShowMessage(
                title: "ACTION STATUS",
                msg: notificationText);


            MpMainWindowViewModel.Instance.IsShowingDialog = false;
        }
        #endregion

        #region Private Methods

        private void MpTriggerActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    break;
                case nameof(IsEnabled):
                    //if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    //    return;
                    //}                    
                    MpHelpers.RunOnMainThread(async () => {
                        await ShowUserEnableChangeNotification();
                    });
                    break;
            }
        }

        #endregion

        #region Commands


        public ICommand SelectTriggerTypeCommand => new RelayCommand<object>(
            (args) => {
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
