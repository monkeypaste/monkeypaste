
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

        #region MpIDesignerSettingsViewModel Implementation

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

        public MpRect ObservedDesignerBounds { get; set; } = MpRect.Empty;
        public double DesignerItemDiameter => 50;


        public MpPoint DefaultTriggerLocation => new MpPoint(
            (ObservedDesignerBounds.Width / 2) - (DesignerItemDiameter / 2),
            (ObservedDesignerBounds.Height / 2) - (DesignerItemDiameter / 2));

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
                return DefaultTriggerLocation;
            }
            var arg2Parts = Arg2.SplitNoEmpty(",");
            if (arg2Parts.Length >= 3) {
                return new MpPoint() {
                    X = double.Parse(arg2Parts[1]),
                    Y = double.Parse(arg2Parts[2])
                };
            }
            return DefaultTriggerLocation;
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

        #region DesignerItem Placement Methods

        public MpPoint FindOpenDesignerLocation(MpPoint anchorPoint, object ignoreItem = null) {
            int attempts = 0;
            int maxAttempts = 10;
            int count = 4;
            double dtheta = (2 * Math.PI) / count;
            double r = DesignerItemDiameter * 2;
            while (attempts <= maxAttempts) {
                double theta = 0;
                for (int i = 0; i < count; i++) {
                    var tp = new MpPoint();
                    tp.X = (double)(anchorPoint.X + r * Math.Cos(theta));
                    tp.Y = (double)(anchorPoint.Y + r * Math.Sin(theta));
                    if (!OverlapsItem(tp)) {
                        return tp;
                    }
                    theta += dtheta;
                }
                r += DesignerItemDiameter * 2;

                attempts++;
            }

            return new MpPoint(
                MpHelpers.Rand.NextDouble() * ObservedDesignerBounds.Width,
                MpHelpers.Rand.NextDouble() * ObservedDesignerBounds.Height);
        }

        public bool OverlapsItem(MpPoint targetTopLeft) {
            return GetItemNearPoint(targetTopLeft) != null;
        }

        public MpIBoxViewModel GetItemNearPoint(MpPoint targetTopLeft, object ignoreItem = null, double radius = 50) {
            MpPoint targetMid = new MpPoint(targetTopLeft.X, targetTopLeft.Y);
            foreach (var avm in SelfAndAllDescendants.Cast<MpIBoxViewModel>()) {
                MpPoint sourceMid = new MpPoint(avm.X, avm.Y);
                double dist = targetMid.Distance(sourceMid);
                if (dist < radius && avm != ignoreItem) {
                    return avm;
                }
            }
            return null;
        }

        //public void ClearAreaAtPoint(MpPoint p, object ignoreItem = null) {
        //    var overlapItem = GetItemNearPoint(p, ignoreItem);
        //    if (overlapItem != null) {
        //        MpPoint tempLoc = p;
        //        do {
        //            var overlapLoc = new MpPoint(overlapItem.X, overlapItem.Y);
        //            double distToMove = overlapLoc.Distance(tempLoc) + 10;

        //            var dir = overlapLoc - tempLoc;
        //            dir.Normalize();
        //            dir = new Vector(-dir.Y, dir.X);
        //            overlapLoc += dir * distToMove;
        //            overlapItem.X = overlapLoc.X;
        //            overlapItem.Y = overlapLoc.Y;

        //            overlapItem = GetItemNearPoint(overlapLoc, overlapItem);
        //            tempLoc = overlapLoc;
        //        } while (overlapItem != null && overlapItem != ignoreItem);
        //    }
        //}

        //public void ClearAllOverlaps() {
        //    foreach (var avm in AllSelectedTriggerActions) {
        //        ClearAreaAtPoint(avm.Location, avm);
        //    }
        //}
        //public void NotifyViewportChanged() {
        //    //CollectionViewSource.GetDefaultView(AllSelectedTriggerActions).Refresh();
        //    //CollectionViewSource.GetDefaultView(AllSelectedTriggerActions).Refresh();
        //    OnPropertyChanged(nameof(AllSelectedItemActions));
        //}
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

        public ObservableCollection<MpAvActionViewModelBase> Items { get; set; } = new ObservableCollection<MpAvActionViewModelBase>();
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

        public override async Task InitializeAsync(MpAction a) {
            await base.InitializeAsync(a);
            Items.Clear();
            SelfAndAllDescendants.ForEach(x => Items.Add(x));
            OnPropertyChanged(nameof(Items));
        }

        #endregion

        #region Protected Methods

        protected override async Task EnableAsync() {
            await base.EnableAsync();
        }

        protected async Task ShowUserEnableChangeNotification() {
            string enabledText = IsEnabled.HasValue && IsEnabled.Value ?
                                    "ENABLED" :
                                    "DISABLED";
            string notificationText = $"Action '{FullName}' is now  {enabledText}";
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = MpAvMainWindowViewModel.Instance.IsMainWindowOpen;

            await MpNotificationBuilder.ShowMessageAsync(
                iconSourceObj: IconResourceKeyStr.ToString(),
                title: "ACTION STATUS",
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
                case nameof(Items):
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
            Scale = 1.0d;
            TranslateOffsetX = -X;
            TranslateOffsetY = -Y;
        });
        #endregion
    }
}
