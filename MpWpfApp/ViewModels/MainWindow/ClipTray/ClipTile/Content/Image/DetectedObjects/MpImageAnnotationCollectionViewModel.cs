using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using System.Collections.ObjectModel;

namespace MpWpfApp {
    public class MpImageAnnotationCollectionViewModel : 
        MpSelectorViewModelBase<MpContentItemViewModel,MpImageAnnotationViewModel> {
        #region Private Variables
        private bool _isEnabled = false;
        #endregion

        #region Properties

        #region View Models

        #endregion
        #endregion

        #region Public Methods
        public MpImageAnnotationCollectionViewModel() : base(null) {
        }
        public MpImageAnnotationCollectionViewModel(MpContentItemViewModel parent) : base(parent) {
           
        }

        public async Task InitializeAsync(MpCopyItem ci, bool isEnabled = false) {
            IsBusy = true;

            _isEnabled = isEnabled;
            if (ci.ItemType != MpCopyItemType.Image) {
                //not sure why this is getting called on non-images this shouldn't have to happen
                return;
            }
            var iiol = await MpDataModelProvider.GetImageAnnotationsByCopyItemId(ci.Id);
            foreach (var dio in iiol) {
                var diovm = await CreateDetectedImageObjectViewModel(dio);
                Items.Add(diovm);
            }
            
            OnPropertyChanged(nameof(Items));


            IsBusy = false;
        }

        public async Task<MpImageAnnotationViewModel> CreateDetectedImageObjectViewModel(MpImageAnnotation dio) {
            var diovm = new MpImageAnnotationViewModel(this);
            await diovm.InitializeAsync(dio);
            return diovm;
        }

        //public void ClipTileImageDetectedObjectItemscontrol_Loaded(object sender, RoutedEventArgs args) {
        //    if(!_isEnabled) {
        //        return;
        //    }
        //    var itemsControl = (ItemsControl)sender;
        //    var itemsControlCanvas = (Canvas)itemsControl.FindName("ClipTileImageDetectedObjectsCanvas");
        //    var vbGrid = itemsControl.GetVisualAncestor<Grid>();
        //    var ctvm = (MpClipTileViewModel)vbGrid.DataContext;

        //    bool isCreatingNewItem = false;

        //    vbGrid.MouseMove += (s, e) => {
        //        if (!ctvm.IsSelected && !ctvm.IsHovering) {
        //            Mouse.Capture(null);
        //            Application.Current.MainWindow.Cursor = Cursors.Arrow;
        //            return;
        //        }
        //        foreach (var diovm in this) {
        //            if (diovm.IsHovering) {
        //                return;
        //            }
        //        }
                
        //        if(isCreatingNewItem) {
        //            Application.Current.MainWindow.Cursor = Cursors.SizeNWSE;
        //        } else {
        //            Application.Current.MainWindow.Cursor = Cursors.Arrow;
        //        }                
        //    };
 
        //    vbGrid.MouseLeftButtonDown += (s, e) => {
        //        if (!ctvm.IsSelected) {
        //            return;
        //        }
        //        foreach (var diovm in this) {
        //            if (diovm.IsHovering) {
        //                return;
        //            }
        //        }
        //        //Mouse.Capture(vbGrid);
        //        var p = Mouse.GetPosition(itemsControl);
        //        this.Add(new MpImageAnnotationViewModel(
        //            new MpImageAnnotation(0, ctvm.CopyItemId, 1, p.X, p.Y, 1, 1, "New Item"),
        //            true));
        //        isCreatingNewItem = true;
        //    };

        //    vbGrid.MouseLeftButtonUp += (s, e) => {
        //        if (!ctvm.IsSelected) {
        //            return;
        //        }
        //        //Mouse.Capture(null);
        //        var p = Mouse.GetPosition(itemsControl);
                
        //        Application.Current.MainWindow.Cursor = Cursors.Arrow;

        //        if (isCreatingNewItem) {
        //            var newItem = this[this.Count - 1];
        //            if (newItem.Width * newItem.Height < 10) {
        //                this.Remove(newItem);
        //            } else {
        //                newItem.IsNameReadOnly = false;
        //            }
        //            isCreatingNewItem = false;
        //        }
        //    };
        //}
        #endregion

        #region Commands

        #endregion
    }
}
