using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste.Common;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvActionDesignerItemDropBehavior : MpAvDropBehaviorBase<Control> {
        #region Private Variables

        private double _autoScrollAccumulator = 5;
        private double _autoScrollVelocity = 25;

        #endregion

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Action;

        public override Control RelativeToElement => AssociatedObject;

        public override Control AdornedElement => AssociatedObject;

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


        public override List<MpRect> GetDropTargetRects() {
            var designerItemRect = new List<MpRect>() {
                new MpRect(0,0,AssociatedObject.Bounds.Width,AssociatedObject.Bounds.Height)
            };
            return designerItemRect;
        }

        public override int GetDropTargetRectIdx() {
            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = VisualExtensions.PointToClient(AssociatedObject, gmp.ToAvPixelPoint()).ToPortablePoint();
            //var mp = Application.Current.MainWindow.TranslatePoint(gmp, AssociatedObject);
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
            if(AssociatedObject.DataContext is MpAvActionViewModelBase avm && !avm.IsValid) {
                return false;
            }

            if(base.IsDragDataValid(isCopy, dragData)) {
                return true;
            }
            if(dragData is MpAvTagTileViewModel) {
                return true;
            }
            return false;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await base.Drop(isCopy, dragData);

            List<MpCopyItem> dragModels = new List<MpCopyItem>();
            if(dragData is MpPortableDataObject mpdo) {
                if (MpAvCefNetWebView.DraggingRtb != null) {
                    await Drop(isCopy, MpAvCefNetWebView.DraggingRtb.DataContext);
                    return;
                }
                // from external source
                var tempCopyItem = await MpAvCopyItemBuilder.CreateFromDataObject(mpdo, true);
                if (tempCopyItem == null) {
                    return;
                }
                dragModels.Add(tempCopyItem);
            } else if(dragData is MpAvClipTileViewModel ctvm) {
                dragModels.Add(ctvm.CopyItem);
            } else if(dragData is MpAvTagTileViewModel ttvm) {
                dragModels = await MpDataModelProvider.GetCopyItemsForTagAsync(ttvm.TagId);
            }

            var avm = AssociatedObject.DataContext as MpAvActionViewModelBase;

            await Task.WhenAll(dragModels.Select(x => avm.PerformAction(x)));
        }


        public override async Task StartDrop(PointerEventArgs e) {
            await Task.Delay(1);
        }

        public override void AutoScrollByMouse() {
            double _minScrollDist = 5;

            var zoomBorder = AssociatedObject.GetVisualAncestor<MpAvZoomBorder>();

            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var zapc_mp = VisualExtensions.PointToClient(AssociatedObject, gmp.ToAvPixelPoint()).ToPortablePoint();
            MpRect zapc_rect = new MpRect(0, 0, AssociatedObject.Bounds.Width, AssociatedObject.Bounds.Height);
            if (!zapc_rect.Contains(zapc_mp)) {
                return;
            }

            bool hasChanged = false;

            MpPoint translateOffset = new MpPoint();
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
