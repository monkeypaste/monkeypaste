using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpActionDesignerItemDropBehavior : MpDropBehaviorBase<FrameworkElement> {
        #region Private Variables

        private double _autoScrollAccumulator = 5;
        private double _autoScrollVelocity = 25;

        #endregion

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Action;

        public override UIElement RelativeToElement => AssociatedObject;

        public override FrameworkElement AdornedElement => AssociatedObject;

        public override Orientation AdornerOrientation => Orientation.Vertical;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        public override void OnLoaded() {
            base.OnLoaded();
            //IsDebugEnabled = true;
            _dataContext = AssociatedObject.DataContext;

            MpMessenger.Register<MpMessageType>(
                AssociatedObject.DataContext,
                ReceivedAssociateObjectViewModelMessage,
                AssociatedObject.DataContext);
        }

        public override void OnUnloaded() {
            base.OnUnloaded();
        }

        private void ReceivedAssociateObjectViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentItemsChanged:
                case MpMessageType.ContentListScrollChanged:
                    //RefreshDropRects();
                    break;
            }
        }


        public override List<Rect> GetDropTargetRects() {
            var designerItemRect = new List<Rect>() {
                new Rect(0,0,AssociatedObject.ActualWidth,AssociatedObject.ActualHeight)
            };
            return designerItemRect;
        }

        public override int GetDropTargetRectIdx() {
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, AssociatedObject);
            if (GetDropTargetRects()[0].Contains(mp)) {
                return 0;
            }
            return -1;
        }
        public override MpShape[] GetDropTargetAdornerShape() {
            // NOTE since actions only have 1 drop rect the cursor change is sufficient

            return new MpShape[] { };

            //var drl = GetDropTargetRects();
            //if (DropIdx < 0 || DropIdx >= drl.Count) {
            //    return null;
            //}
            //var s = new MpSize(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            //return  new MpEllipse(new MpPoint(s.Width / 2, s.Height / 2), s).ToArray<MpShape>();
        }

        public override bool IsDragDataValid(bool isCopy,object dragData) {
            if(AssociatedObject.DataContext is MpActionViewModelBase avm && !avm.IsValid) {
                return false;
            }

            if(base.IsDragDataValid(isCopy, dragData)) {
                return true;
            }
            if(dragData is MpTagTileViewModel) {
                return true;
            }
            return false;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await base.Drop(isCopy, dragData);

            List<MpCopyItem> dragModels = new List<MpCopyItem>();
            if(dragData is MpPortableDataObject mpdo) {
                if (MpRichTextBox.DraggingRtb != null) {
                    await Drop(isCopy, MpRichTextBox.DraggingRtb.DataContext);
                    return;
                }
                // from external source
                var tempCopyItem = await MpWpfCopyItemBuilder.CreateFromDataObjectAsync(mpdo, true);
                if (tempCopyItem == null) {
                    return;
                }
                dragModels.Add(tempCopyItem);
            } else if(dragData is MpClipTileViewModel ctvm) {
                dragModels.Add(ctvm.CopyItem);
            } else if(dragData is MpTagTileViewModel ttvm) {
                dragModels = await MpDataModelProvider.GetCopyItemsForTagAsync(ttvm.TagId);
            }

            var avm = AssociatedObject.DataContext as MpActionViewModelBase;

            await Task.WhenAll(dragModels.Select(x => avm.PerformActionAsync(x)));
        }


        public override async Task StartDrop() {
            await Task.Delay(1);
        }

        public override void AutoScrollByMouse() {
            double _minScrollDist = 5;

            var zoomBorder = AssociatedObject.GetVisualAncestor<MpZoomBorder>();
            
            var zapc_mp = Mouse.GetPosition(zoomBorder);
            Rect zapc_rect = new Rect(0, 0, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            if (!zapc_rect.Contains(zapc_mp)) {
                return;
            }

            bool hasChanged = false;

            Point translateOffset = new Point();
            if (Math.Abs(zapc_rect.Right - zapc_mp.X) <= _minScrollDist) {
                translateOffset.X -= _autoScrollVelocity;
                hasChanged = true;
            } else if (Math.Abs(zapc_rect.Left - zapc_mp.X) <= _minScrollDist) {
                translateOffset.X += _autoScrollVelocity;
                hasChanged = true;
            }

            if (Math.Abs(zapc_rect.Top - zapc_mp.Y) <= _minScrollDist) {
                translateOffset.Y -= _autoScrollVelocity;
                hasChanged = true;
            } else if (Math.Abs(zapc_rect.Bottom - zapc_mp.Y) <= _minScrollDist) {
                translateOffset.Y += _autoScrollVelocity;
                hasChanged = true;
            }

            if (hasChanged) {
                _autoScrollVelocity += _autoScrollAccumulator;
                zoomBorder.Translate(translateOffset.X, translateOffset.Y);
            }
        }
    }

}
