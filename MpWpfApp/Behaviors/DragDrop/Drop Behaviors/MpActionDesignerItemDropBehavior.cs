﻿using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpActionDesignerItemDropBehavior : MpDropBehaviorBase<FrameworkElement> {
        #region Private Variables

        private double _autoScrollAccumulator = 5;
        private double _autoScrollVelocity = 25;

        #endregion

        public override bool IsEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Content;

        public override UIElement RelativeToElement => AssociatedObject;

        public override FrameworkElement AdornedElement => AssociatedObject;

        public override Orientation AdornerOrientation => Orientation.Vertical;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        public override void OnLoaded() {
            base.OnLoaded();

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
                case MpMessageType.ContentListItemsChanged:
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
            var mp = Mouse.GetPosition(AssociatedObject);
            if (GetDropTargetRects()[0].Contains(mp)) {
                return 0;
            }
            return -1;
        }

        public override bool IsDragDataValid(bool isCopy,object dragData) {
            var cil = dragData as List<MpCopyItem>;
            return cil != null && cil.Count == 1;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await base.Drop(isCopy, dragData);

            List<MpCopyItem> dragModels = dragData as List<MpCopyItem>;

            var avm = AssociatedObject.DataContext as MpActionViewModelBase;

            await avm.PerformAction(dragModels[0]);
        }


        public override async Task StartDrop() {
            await Task.Delay(1);
        }

        public override void AutoScrollByMouse() {
            double _minScrollDist = 5;

            var zapc = AssociatedObject.GetVisualAncestor<ZoomAndPan.ZoomAndPanControl>();

            var zapc_mp = Mouse.GetPosition(zapc);
            Rect zapc_rect = new Rect(0, 0, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            if (!zapc_rect.Contains(zapc_mp)) {
                return;
            }

            bool hasChanged = false;

            if (Math.Abs(zapc_rect.Right - zapc_mp.X) <= _minScrollDist) {
                zapc.ContentOffsetX -= _autoScrollVelocity;
                hasChanged = true;
            } else if (Math.Abs(zapc_rect.Left - zapc_mp.X) <= _minScrollDist) {
                zapc.ContentOffsetX += _autoScrollVelocity;
                hasChanged = true;
            }

            if (Math.Abs(zapc_rect.Top - zapc_mp.Y) <= _minScrollDist) {
                zapc.ContentOffsetY -= _autoScrollVelocity;
                hasChanged = true;
            } else if (Math.Abs(zapc_rect.Bottom - zapc_mp.Y) <= _minScrollDist) {
                zapc.ContentOffsetY += _autoScrollVelocity;
                hasChanged = true;
            }

            if (hasChanged) {
                _autoScrollVelocity += _autoScrollAccumulator;
            }
        }
    }

}