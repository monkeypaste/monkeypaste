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


        #region Constructors

        static MpAvDragExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        private static MpAvIDragHost _currentDragHost;
        public static MpAvIDragHost CurrentDragHost {
            get => _currentDragHost;
            set {
                if(_currentDragHost != value) {
                    var oldVal = _currentDragHost;
                    _currentDragHost = value;

                    bool is_drag_end = value == null;

                    if(is_drag_end) {
                        bool was_drag_source_internal = oldVal is Control dragHost && dragHost.DataContext is MpViewModelBase;
                        if (was_drag_source_internal) {
                            // when drag is complete and drag host was from external source
                            MpMessenger.SendGlobal(MpMessageType.ItemDragEnd);
                        } else {
                            // TODO if necessary send ExternalDragEnd here
                        }
                    } else {
                        // drag begin
                        bool is_drag_source_internal = _currentDragHost is Control dragHost && dragHost.DataContext is MpViewModelBase;
                        if(is_drag_source_internal) {
                            MpMessenger.SendGlobal(MpMessageType.ItemDragBegin);
                        } else {
                            // TODO if necessary send External DragBegin here
                        }
                    }
                }
            }
        }

        #region DragDataHost AvaloniaProperty
        public static MpAvIDragHost GetDragDataHost(AvaloniaObject obj) {
            return obj.GetValue(DragDataHostProperty);
        }

        public static void SetDragDataHost(AvaloniaObject obj, MpAvIDragHost value) {
            obj.SetValue(DragDataHostProperty, value);
        }

        public static readonly AttachedProperty<MpAvIDragHost> DragDataHostProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpAvIDragHost>(
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
            if(CurrentDragHost != null) {
                // this shouldn't happen
                Debugger.Break();
                _currentDragHost = null;
                return;
            }

            if (sender is Control dragControl) {
                if(dragControl.DataContext is MpIResizableViewModel rvm &&
                    rvm.CanResize) {
                    return;
                }
                DragCheckAndStart(dragControl, e);
            }
        }

        private static void DragCheckAndStart(Control dragControl, PointerPressedEventArgs e) {
            MpPoint dc_down_pos = e.GetClientMousePoint(dragControl);
            var dragHost = GetDragDataHost(dragControl);
            if (dragHost == null || !dragHost.IsDragValid(dc_down_pos)) {
                return;
            }
            bool was_drag_started = false;

            EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
            EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;

            // Drag Control PointerMoved Handler
            dragControl_PointerMoved_Handler = (s, e) => {
                MpPoint dc_move_pos = e.GetClientMousePoint(dragControl);

                var drag_dist = dc_down_pos.Distance(dc_move_pos);
                was_drag_started = drag_dist >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST;
                if (was_drag_started) {

                    // DRAG START

                    dragControl.PointerMoved -= dragControl_PointerMoved_Handler;

                    Dispatcher.UIThread.Post(async () => {
                        DragDropEffects dragEffects = GetDragEffects(dragHost, dc_move_pos, e.KeyModifiers);
                        if (dragEffects != DragDropEffects.None) {

                            _currentDragHost = dragHost;
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
                
                // DRAG END

                dragControl.PointerMoved -= dragControl_PointerMoved_Handler;
                dragControl.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("DragCheck pointer released (was not drag)");

                _currentDragHost = null;
                dragHost.DragEnd();

            };

            dragControl.PointerReleased += dragControl_PointerReleased_Handler;
            dragControl.PointerMoved += dragControl_PointerMoved_Handler;
        }

        private static DragDropEffects GetDragEffects(MpAvIDragHost dragDataHost, MpPoint drag_host_mp, KeyModifiers keyModifiers) {
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
