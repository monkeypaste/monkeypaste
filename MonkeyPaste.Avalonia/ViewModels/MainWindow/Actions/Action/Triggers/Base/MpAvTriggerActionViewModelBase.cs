
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Avalonia.Threading;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {

    public class MpAvTriggerActionViewModelBase : 
        MpAvActionViewModelBase,
        MpIResizableViewModel,
        //MpIAsyncComboBoxItemViewModel,
        MpIDesignerSettingsViewModel {

        //#region MpIAsyncComboBoxItemViewModel Implementation

        //int MpAvIComboBoxItemViewModel.IconId => IconId;
        //string MpAvIComboBoxItemViewModel.Label => Label;

        //#endregion


        #region Constants

        public const double DEFAULT_MIN_SCALE = 0.1;
        public const double DEFAULT_MAX_SCALE = 3.0d;
        #endregion

        #region MpIDesignerSettingsViewModel Implementation

        public double MinScale => DEFAULT_MIN_SCALE;
        public double MaxScale => DEFAULT_MAX_SCALE;
        public bool IsGridVisible {
            get {
                return ParseShowGrid(Arg2);
            }
            set {
                if (IsGridVisible != value) {
                    SetDesignerItemSettings(Scale, TranslateOffsetX, TranslateOffsetY, value);
                    OnPropertyChanged(nameof(IsGridVisible));
                }
            }
        }
        public double Scale {
            get {
                return ParseScale(Arg2);
            }
            set {
                if (Math.Abs(Scale - value) > 0.1) {
                    SetDesignerItemSettings(value, TranslateOffsetX, TranslateOffsetY, IsGridVisible);
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }
        public double TranslateOffsetX {
            get {
                return ParseTranslationOffset(Arg2).X;
            }
            set {
                if (Math.Abs(TranslateOffsetX - value) > 0.1) {
                    SetDesignerItemSettings(Scale, Math.Round(value,1), TranslateOffsetY, IsGridVisible);
                    OnPropertyChanged(nameof(TranslateOffsetX));
                }
            }
        }
        public double TranslateOffsetY {
            get {
                return ParseTranslationOffset(Arg2).Y;
            }
            set {
                if (Math.Abs(TranslateOffsetY - value) > 0.1) {
                    SetDesignerItemSettings(Scale, TranslateOffsetX, Math.Round(value, 1), IsGridVisible);
                    OnPropertyChanged(nameof(TranslateOffsetY));
                }
            }
        }


        #region Designer Helpers

        #region Designer Model Parsing Helpers

        private void SetDesignerItemSettings(double scale, double offsetX, double offsetY, bool showGrid) {
            string arg2 = string.Join(
                ",",
                new string[] {
                    scale.ToString(),
                    offsetX.ToString(), offsetY.ToString(),
                    showGrid.ToString()});
            Arg2 = arg2;
            if(!IsMoving) {
                // move extension triggers model change on pointer up
                HasModelChanged = true;
            }
        }

        private double ParseScale(string text) {
            if (string.IsNullOrEmpty(Arg2)) {
                return 1.0d;
            }
            var arg2Parts = Arg2.SplitNoEmpty(",");
            if(arg2Parts.Length > 0) {
                return double.Parse(arg2Parts[0]);
            }
            return 1.0d;
        }

        private MpPoint ParseTranslationOffset(string text) {
            if (string.IsNullOrEmpty(Arg2)) {
                return Parent.DesignerCenterLocation;
            }
            var arg2Parts = Arg2.SplitNoEmpty(",");
            if (arg2Parts.Length >= 3) {
                return new MpPoint() {
                    X = double.Parse(arg2Parts[1]),
                    Y = double.Parse(arg2Parts[2])
                };
            }
            return Parent.DesignerCenterLocation;
        }
        private bool ParseShowGrid(string text) {
            if (string.IsNullOrEmpty(Arg2)) {
                return false;
            }
            var arg2Parts = Arg2.SplitNoEmpty(",");
            if (arg2Parts.Length >= 4) {
                return arg2Parts[3].ToLower() == "true";
            }
            return false;
        }

        #endregion

        

        #endregion

        #endregion


        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion
        #region Properties

        //public SelectionModel<MpAvActionViewModelBase> Selection { get; private set; }

        #region View Models

        //public ObservableCollection<MpAvActionViewModelBase> Items { get; set; } = new ObservableCollection<MpAvActionViewModelBase>();
        //public MpAvActionViewModelBase SelectedAction { get; set; }
        #endregion


        #region Appearance

        #endregion

        #region Layout


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

        #endregion

        #endregion

        #region Constructors

        public MpAvTriggerActionViewModelBase(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvTriggerActionViewModelBase_PropertyChanged;
            //Items.CollectionChanged += Items_CollectionChanged;

            //Selection = new SelectionModel<MpAvActionViewModelBase>() { SingleSelect = true };
            //Selection.SelectionChanged += Selection_SelectionChanged;
        }


        #endregion

        #region Public Methods

        //public override async Task InitializeAsync(MpAction a) {
        //    await base.InitializeAsync(a);
        //    Items.Clear();
        //    SelfAndAllDescendants.ForEach(x => Items.Add(x));
        //    OnPropertyChanged(nameof(Items));
        //}

        #endregion

        #region Protected Methods

        protected override async Task EnableAsync() {
            await base.EnableAsync();
        }

        protected async Task ShowUserEnableChangeNotification() {
            string enabledText = IsEnabled.HasValue && IsEnabled.Value ?
                                    "ENABLED" :
                                    "DISABLED";
            string typeStr = ParentActionId == 0 ? "Trigger" : "Action";
            string notificationText = $"{typeStr} '{FullName}' is now  {enabledText}";
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = MpAvMainWindowViewModel.Instance.IsMainWindowOpen;

            await MpNotificationBuilder.ShowMessageAsync(
                iconSourceObj: IconResourceKeyStr.ToString(),
                title: $"{typeStr.ToUpper()} STATUS",
                body: notificationText);


            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
        }
        #endregion

        #region Private Methods

        private void MpAvTriggerActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    //Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItemIdx));
                    //OnPropertyChanged(nameof(SelectedItem));
                    break;
                case nameof(IsEnabled):
                    //if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    //    return;
                    //}                    
                    Dispatcher.UIThread.Post(async () => {
                        await ShowUserEnableChangeNotification();
                    });
                    break;
                case nameof(Children):
                    OnPropertyChanged(nameof(SelfAndAllDescendants));
                    break;
                case nameof(IsBusy):
                    if(Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    }
                    break;
                //case nameof(SelectedAction):
                //    Parent.OnPropertyChanged(nameof(Parent.FocusAction));
                //    break;
            }
        }


        //private void Selection_SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<MpAvActionViewModelBase> e) {
        //    Parent.SelectActionCommand.Execute(SelectedItem);
        //    Parent.OnPropertyChanged(nameof(Parent.SelectedAction));
        //    if(SelectedItem == null) {
        //        SelfAndAllDescendants.Cast<MpAvActionViewModelBase>().ForEach(x => x.IsSelected = false);
        //    } else {
        //        SelfAndAllDescendants.Cast<MpAvActionViewModelBase>().ForEach(x => x.IsSelected = x == SelectedItem);
        //    }
        //}
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(SelfAndAllDescendants));
        }
        #endregion

        #region Commands

        public ICommand ResetDesignerViewCommand => new MpCommand(() => {
            double l = SelfAndAllDescendants.Min(x => x.X);
            double t = SelfAndAllDescendants.Min(x => x.Y);
            double r = SelfAndAllDescendants.Max(x => x.X);
            double b = SelfAndAllDescendants.Max(x => x.Y);

            MpRect actual_bounds = new MpRect(l, t, r - l, b - t);
            
            //double r_x = Parent.ObservedDesignerBounds.Width / actual_bounds.Width;
            //double r_y = Parent.ObservedDesignerBounds.Height / actual_bounds.Height;
            
            double r_x = actual_bounds.Width / Parent.ObservedDesignerBounds.Width;
            double r_y = actual_bounds.Height / Parent.ObservedDesignerBounds.Height;

            double scale_pad = 0.2;

            Parent.Scale = Math.Max(Math.Min(r_x.IsNumber() ? r_x : 1, r_y.IsNumber() ? r_y : 1) - scale_pad, Parent.MinScale);

            var item_half_size = new MpSize(Parent.DesignerItemDiameter/2, Parent.DesignerItemDiameter/2);
            var view_center = (Parent.ObservedDesignerBounds.Size.ToPortablePoint() - item_half_size.ToPortablePoint()) * 0.5;
            var actual_center = actual_bounds.Centroid();

            var adj_center = (view_center * Parent.Scale) - actual_center;
            Parent.TranslateOffsetX = adj_center.X;
            Parent.TranslateOffsetY = adj_center.Y;
        });
        #endregion
    }
}
