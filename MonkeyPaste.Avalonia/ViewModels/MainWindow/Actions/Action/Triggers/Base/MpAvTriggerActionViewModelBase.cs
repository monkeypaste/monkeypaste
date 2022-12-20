
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

namespace MonkeyPaste.Avalonia {

    public class MpAvTriggerActionViewModelBase : 
        MpAvActionViewModelBase,
        MpIResizableViewModel,
        MpIAsyncComboBoxItemViewModel,
        MpISidebarItemViewModel,
        MpIDesignerSettingsViewModel {

        #region MpIAsyncComboBoxItemViewModel Implementation


        int MpIComboBoxItemViewModel.IconId => IconId;
        string MpIComboBoxItemViewModel.Label => Label;

        #endregion

        #region MpIDesignerSettingsViewModel Implementation

        public double Scale {
            get {
                return ParseScale(Arg2);
            }
            set {
                if (Math.Abs(Scale - value) > 0.1) {
                    SetDesignerItemSettings(value, TranslateOffsetX, TranslateOffsetY);
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
                    SetDesignerItemSettings(Scale, value, TranslateOffsetY);
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
                    SetDesignerItemSettings(Scale, TranslateOffsetX, value);
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

        private void SetDesignerItemSettings(double scale, double offsetX, double offsetY) {
            string arg2 = string.Join(
                ",",
                new string[] {
                    scale.ToString(),
                    offsetX.ToString(), offsetY.ToString() });
            Arg2 = arg2;
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
                return MpPoint.Zero;
            }
            var arg2Parts = Arg2.SplitNoEmpty(",");
            if (arg2Parts.Length >= 3) {
                return new MpPoint() {
                    X = double.Parse(arg2Parts[1]),
                    Y = double.Parse(arg2Parts[2])
                };
            }
            return MpPoint.Zero;
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

        #region Properties

        #region View Models

        //public IEnumerable<MpAvActionViewModelBase> AllChildren => this.FindAllChildren().Cast<MpAvActionViewModelBase>();//new ObservableCollection<MpAvActionViewModelBase>();

        public override MpAvActionViewModelBase SelectedItem {
            get {
                if(Parent == null || !IsSelected) {
                    return null;
                }
                //if(base.SelectedItem == null) {
                //    return this;
                //}
                //return base.SelectedItem;
                var sel_descendant = AllDescendants.Cast<MpISelectableViewModel>().FirstOrDefault(x => x.IsSelected);
                if(sel_descendant != null) {
                    return sel_descendant as MpAvActionViewModelBase;
                }
                return this;
            }
            set {
                if(value == null) {
                    IsSelected = false;
                    AllDescendants.Cast<MpISelectableViewModel>().ForEach(x => x.IsSelected = false);
                    OnPropertyChanged(nameof(SelectedItem));
                    return;
                }
                if(SelfAndAllDescendants.Cast<MpAvActionViewModelBase>().All(x=>x.ActionId != value.ActionId)) {
                    // not part of this trigger
                    Debugger.Break();
                    return;
                }
                IsSelected = true;
                AllDescendants
                    .Cast<MpAvActionViewModelBase>()
                    .ForEach(x => x.IsSelected = x.ActionId == value.ActionId);
                OnPropertyChanged(nameof(SelectedItem));
            } 
        }

        //public override IEnumerable<MpAvActionViewModelBase> SelfAndAllDescendants =>
        //    SelfAndAllDescendants.Cast<MpAvActionViewModelBase>();

        #endregion

        #region MpISidebarItemViewModel Implementation

        public double DefaultSidebarWidth => 800;// MpMeasurements.Instance.DefaultDesignerWidth;
        public double SidebarWidth { get; set; } = 0;// MpMeasurements.Instance.DefaultDesignerWidth;

        public bool IsSidebarVisible { get; set; }
        public MpISidebarItemViewModel NextSidebarItem => null;
        public MpISidebarItemViewModel PreviousSidebarItem => Parent;

        #endregion

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region Appearance

        #endregion

        #region Layout


        #endregion

        #region State

        public bool IsContentAddTrigger => TriggerType == MpTriggerType.ContentAdded;

        public bool IsFileSystemTrigger => TriggerType == MpTriggerType.FileSystemChange;

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
            Items.CollectionChanged += Items_CollectionChanged;
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
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = MpAvMainWindowViewModel.Instance.IsMainWindowOpen;

            await MpNotificationBuilder.ShowMessageAsync(
                iconSourceStr: IconResourceKeyStr,
                title: "ACTION STATUS",
                body: notificationText);


            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
        }
        #endregion

        #region Private Methods

        private void MpAvTriggerActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
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
            }
        }


        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(SelfAndAllDescendants));
        }
        #endregion

        #region Commands

        #endregion
    }
}
