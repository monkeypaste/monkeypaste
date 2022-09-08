using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDragExtension {
        #region Private Variables

        #endregion

        #region Constants

        public const double MIN_DRAG_DIST = 5;

        #endregion

        static MpAvDragExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region DragDataHost AvaloniaProperty
        public static MpAvIDragDataHost GetDragDataHost(AvaloniaObject obj) {
            return obj.GetValue(DragDataHostProperty);
        }

        public static void SetDragDataHost(AvaloniaObject obj, MpAvIDragDataHost value) {
            obj.SetValue(DragDataHostProperty, value);
        }

        public static readonly AttachedProperty<MpAvIDragDataHost> DragDataHostProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvIDragDataHost>(
                "DragDataHost",
                null,
                false);

        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    if (control.IsInitialized) {
                        EnabledControl_AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += EnabledControl_AttachedToVisualHandler;
                    }
                }
            } else {
                DisabledControl_DetachedToVisualHandler(element, null);
            }
        }

        #endregion

        #region Control Event Handlers

        private static void EnabledControl_AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control dragControl) {
                dragControl.DetachedFromVisualTree += DisabledControl_DetachedToVisualHandler;
                dragControl.PointerPressed += DragControl_PointerPressed;
                if (e == null) {
                    dragControl.AttachedToVisualTree += EnabledControl_AttachedToVisualHandler;
                }
            }
        }


        private static void DisabledControl_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= EnabledControl_AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DisabledControl_DetachedToVisualHandler;

                control.PointerPressed -= DragControl_PointerPressed;
            }
        }

        #endregion

        #region DragDrop 

        #region Drag 
        private static void DragControl_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is Control dragControl) {
                if(dragControl.DataContext is MpIResizableViewModel rvm &&
                    rvm.CanResize) {
                    return;
                }
                DragCheckAndStart(dragControl, e);
            }
        }

        private static void DragCheckAndStart(Control dragControl, PointerPressedEventArgs e) {
            var dragHost = GetDragDataHost(dragControl);
            if (dragHost == null) {
                return;
            }
            MpPoint dc_down_pos = e.GetClientMousePoint(dragControl);
            bool was_drag_started = false;
            // 
            EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
            EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;

            // Drag Control PointerMoved Handler
            dragControl_PointerMoved_Handler = (s, e) => {
                MpPoint dc_move_pos = e.GetClientMousePoint(dragControl);

                var drag_dist = dc_down_pos.Distance(dc_move_pos);
                was_drag_started = drag_dist >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST;
                if (was_drag_started) {
                    // drag start
                    //dc_down_pos = null;
                    dragControl.PointerMoved -= dragControl_PointerMoved_Handler;

                    Dispatcher.UIThread.Post(async () => {
                        DragDropEffects dragEffects = GetDragEffects(dragHost, dc_move_pos, e.KeyModifiers);
                        if (dragEffects != DragDropEffects.None) {

                            dragHost.DragBegin();

                            var avdo = await dragHost.GetDragDataObjectAsync();
                            MpConsole.WriteLine("Drag Start. DragEffects: " + dragEffects);
                            var result = await DragDrop.DoDragDrop(e, avdo, dragEffects);

                            dragHost.DragEnd();
                            dragControl.PointerReleased -= dragControl_PointerReleased_Handler;
                            MpConsole.WriteLine("Drag End. Result effect: " + result);
                        }
                    });
                }
            };


            // Drag Control PointerReleased Handler
            dragControl_PointerReleased_Handler = (s, e) => {
                if (was_drag_started) {
                    // this should not happen, or release is called before drop (if its called at all during drop
                    // release should be removed after drop
                    Debugger.Break();
                }
                dragControl.PointerMoved -= dragControl_PointerMoved_Handler;
                dragControl.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("DragCheck pointer released (was not drag)");

                //dc_down_pos = null;
                dragHost.DragEnd();

            };

            dragControl.PointerReleased += dragControl_PointerReleased_Handler;
            dragControl.PointerMoved += dragControl_PointerMoved_Handler;
        }

        private static DragDropEffects GetDragEffects(MpAvIDragDataHost dragDataHost, MpPoint drag_host_mp, KeyModifiers keyModifiers) {
            if (dragDataHost == null) {
                return DragDropEffects.None;
            }
            if (!dragDataHost.IsDragValid(drag_host_mp)) {
                return DragDropEffects.None;
            }

            if (keyModifiers.HasFlag(KeyModifiers.Control)) {
                return DragDropEffects.Copy;
            }
            return DragDropEffects.Move;
        }

        #endregion

        #endregion
    }

}
