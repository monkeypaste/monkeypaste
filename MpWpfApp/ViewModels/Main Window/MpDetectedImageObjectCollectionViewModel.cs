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

namespace MpWpfApp {
    public class MpDetectedImageObjectCollectionViewModel : MpObservableCollectionViewModel<MpDetectedImageObjectViewModel> {
        #region Private Variables

        #endregion
        #region Properties
        private BitmapSource _copyItemBmp = null;
        public BitmapSource CopyItemBmp {
            get {
                return _copyItemBmp;
            }
            set {
                if(_copyItemBmp != value) {
                    _copyItemBmp = value;
                    OnPropertyChanged(nameof(CopyItemBmp));
                    OnPropertyChanged(nameof(CopyItemBmpWidth));
                    OnPropertyChanged(nameof(CopyItemBmpHeight));
                    OnPropertyChanged(nameof(CopyItemBmpImageBrush));
                }
            }
        }

        public double CopyItemBmpHeight {
            get {
                if(CopyItemBmp == null) {
                    return 0;
                }
                return CopyItemBmp.Height;
            }
        }

        public double CopyItemBmpWidth {
            get {
                if (CopyItemBmp == null) {
                    return 0;
                }
                return CopyItemBmp.Width;
            }
        }

        public ImageBrush CopyItemBmpImageBrush {
            get {
                if (CopyItemBmp == null) {
                    return new ImageBrush();
                }
                return new ImageBrush(CopyItemBmp);
            }
        }
        #endregion

        #region Public Methods
        public MpDetectedImageObjectCollectionViewModel() : base() {
        }
        public MpDetectedImageObjectCollectionViewModel(MpCopyItem ci) : base() {
            if(ci.CopyItemType != MpCopyItemType.Image) {
                //not sure why this is getting called on non-images this shouldn't have to happen
                return;
            }
            foreach(var dio in ci.ImageItemObjectList) {
                dio.CopyItemId = ci.CopyItemId;
                this.Add(new MpDetectedImageObjectViewModel(dio));
            }
            CopyItemBmp = ci.ItemBitmapSource;
        }

        public void ClipTileImageDetectedObjectItemscontrol_Loaded(object sender, RoutedEventArgs args) {
            var itemsControl = (ItemsControl)sender;
            var itemsControlCanvas = (Canvas)itemsControl.FindName("ClipTileImageDetectedObjectsCanvas");
            var vbGrid = itemsControl.GetVisualAncestor<Grid>();
            var ctvm = (MpClipTileViewModel)vbGrid.DataContext;

            bool isCreatingNewItem = false;

            vbGrid.MouseMove += (s, e) => {
                if (!ctvm.IsSelected) {
                    return;
                }
                foreach (var diovm in this) {
                    if (diovm.IsHovering) {
                        return;
                    }
                }
                
                if(isCreatingNewItem) {
                    Application.Current.MainWindow.Cursor = Cursors.SizeNWSE;
                } else {
                    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                }                
            };
 
            vbGrid.MouseLeftButtonDown += (s, e) => {
                if (!ctvm.IsSelected) {
                    return;
                }
                foreach (var diovm in this) {
                    if (diovm.IsHovering) {
                        return;
                    }
                }
                //Mouse.Capture(vbGrid);
                var p = Mouse.GetPosition(itemsControl);
                this.Add(new MpDetectedImageObjectViewModel(
                    new MpDetectedImageObject(0, ctvm.CopyItemId, 1, p.X, p.Y, 1, 1, "New Item"),
                    true));
                isCreatingNewItem = true;
            };

            vbGrid.MouseLeftButtonUp += (s, e) => {
                if (!ctvm.IsSelected) {
                    return;
                }
                //Mouse.Capture(null);
                var p = Mouse.GetPosition(itemsControl);
                
                Application.Current.MainWindow.Cursor = Cursors.Arrow;

                if (isCreatingNewItem) {
                    var newItem = this[this.Count - 1];
                    if (newItem.Width * newItem.Height < 10) {
                        this.Remove(newItem);
                    } else {
                        newItem.IsNameReadOnly = false;
                    }
                    isCreatingNewItem = false;
                }
            };

            //CollectionChanged += (s, e) => {
            //    if(e.NewItems != null) {
            //        foreach(MpDetectedImageObjectViewModel diovm in e.NewItems) {
            //            int idx = this.IndexOf(diovm);
            //            if(idx < 0) {
            //                continue;
            //            }
            //            Canvas.SetZIndex(
            //                (Border)VisualTreeHelper.GetChild(
            //                    (DependencyObject)itemsControl,
            //                    idx),
            //                idx + 1);
            //        }
            //    }
            //};
        }
        #endregion

        #region Commands

        #endregion
    }
}
