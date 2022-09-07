using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;
using System.Linq;
using System;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using Avalonia.Controls.Primitives;
using static SQLite.SQLite3;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMousePressCommand {
        #region Private Variables

        #endregion

        #region Constants

        public const double MIN_DRAG_DIST = 5;

        #endregion

        #region Properties

        public static ObservableCollection<Control> DropControls { get; private set; } = new ObservableCollection<Control>();
        #endregion

        static MpAvMousePressCommand() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsDropControlProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsDropControlChanged(x, y));
            DropAdornedControlProperty.Changed.AddClassHandler<Control>((x, y) => HandleDropAdornedControlChanged(x, y));

            DropControls.CollectionChanged += DropControls_CollectionChanged;
        }


        #region LeftPressCommand AvaloniaProperty
        public static ICommand GetLeftPressCommand(AvaloniaObject obj) {
            return obj.GetValue(LeftPressCommandProperty);
        }

        public static void SetLeftPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LeftPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LeftPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "LeftPressCommand",
                null,
                false);

        #endregion

        #region RightPressCommand AvaloniaProperty
        public static ICommand GetRightPressCommand(AvaloniaObject obj) {
            return obj.GetValue(RightPressCommandProperty);
        }

        public static void SetRightPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(RightPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> RightPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "RightPressCommand",
                null,
                false);

        #endregion

        #region DoubleLeftPressCommand AvaloniaProperty
        public static ICommand GetDoubleLeftPressCommand(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftPressCommandProperty);
        }

        public static void SetDoubleLeftPressCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DoubleLeftPressCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DoubleLeftPressCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DoubleLeftPressCommand",
                null,
                false);

        #endregion

        #region LeftPressCommandParameter AvaloniaProperty
        public static object GetLeftPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(LeftPressCommandParameterProperty);
        }

        public static void SetLeftPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(LeftPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> LeftPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "LeftPressCommandParameter",
                null,
                false);

        #endregion

        #region RightPressCommandParameter AvaloniaProperty
        public static object GetRightPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(RightPressCommandParameterProperty);
        }

        public static void SetRightPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(RightPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> RightPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "RightPressCommandParameter",
                null,
                false);

        #endregion

        #region DoubleLeftPressCommandParameter AvaloniaProperty
        public static object GetDoubleLeftPressCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DoubleLeftPressCommandParameterProperty);
        }

        public static void SetDoubleLeftPressCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DoubleLeftPressCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DoubleLeftPressCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DoubleLeftPressCommandParameter",
                null,
                false);

        #endregion

        #region LeftDragCommand AvaloniaProperty
        public static ICommand GetLeftDragBeginCommand(AvaloniaObject obj) {
            return obj.GetValue(LeftDragBeginCommandProperty);
        }

        public static void SetLeftDragBeginCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LeftDragBeginCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LeftDragBeginCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "LeftDragBeginCommand",
                null,
                false);

        #endregion

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

        #region DropAdornedControl AvaloniaProperty
        public static Control GetDropAdornedControl(AvaloniaObject obj) {
            return obj.GetValue(DropAdornedControlProperty);
        }

        public static void SetDropAdornedControl(AvaloniaObject obj, Control value) {
            obj.SetValue(DropAdornedControlProperty, value);
        }

        public static readonly AttachedProperty<Control> DropAdornedControlProperty =
            AvaloniaProperty.RegisterAttached<object, Control, Control>(
                "DropAdornedControl",
                null,
                false);

        #endregion

        #region IsDropControl AvaloniaProperty
        public static bool GetIsDropControl(AvaloniaObject obj) {
            return obj.GetValue(IsDropControlProperty);
        }

        public static void SetIsDropControl(AvaloniaObject obj, bool value) {
            obj.SetValue(IsDropControlProperty, value);
        }

        public static readonly AttachedProperty<bool> IsDropControlProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsDropControl",
                false,
                false);

        private static void HandleIsDropControlChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isDropControl && 
                element is Control control) {
                if(isDropControl) {
                    if(!DropControls.Contains(control)) {
                        DropControls.Add(control);
                        MpConsole.WriteLine("IsDropControl trued and added. Control Type: " + control.GetType() + " DataContext: "+control.DataContext);
                    }
                } else {
                    DropControls.Remove(control);
                    MpConsole.WriteLine("IsDropControl falsed and removed. Control Type: " + control.GetType() + " DataContext: " + control.DataContext);
                }
            }
        }

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
            if(e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                        
                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }
        }

        #endregion

        #region Control Event Handlers
        
        private static void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.DetachedFromVisualTree += DetachedToVisualHandler;
                control.PointerPressed += Control_PointerPressed;
                
                if(GetDragDataHost(control) != null) {
                    control.PointerPressed += DragControl_PointerPressed;
                }
                if (e == null) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                }
            }
        }


        private static void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                // control
                control.AttachedToVisualTree -= AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DetachedToVisualHandler;
                control.PointerPressed -= Control_PointerPressed;

                // drag 
                control.PointerPressed -= DragControl_PointerPressed;

                // drop
                if (DropControls.Contains(control)) {
                    DropControls.Remove(control);
                    MpConsole.WriteLine("DropContol DETACHED FROM VIEW and removed . Control Type: " + control.GetType() + " DataContext: " + control.DataContext);                    
                }
            }
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if(sender is Control control) {
                ICommand cmd = null;
                object param = null;
                if(e.IsLeftPress(control)) {
                    if(e.ClickCount == 2) {
                        cmd = GetDoubleLeftPressCommand(control);
                        param = GetDoubleLeftPressCommandParameter(control);
                    } else {
                        cmd = GetLeftPressCommand(control);
                        param = GetLeftPressCommandParameter(control);
                    }
                } else if(e.IsRightPress(control)) {
                    cmd = GetRightPressCommand(control);
                    param = GetRightPressCommandParameter(control);
                }
                if (cmd != null) {
                   cmd.Execute(param);
                }
            }
        }

        #endregion

        #region DragDrop 

        #region Drag 
        private static void DragControl_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is Control dragControl) {
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
                        if(dragEffects != DragDropEffects.None) {

                            MpConsole.WriteLine("Drag Start. DragEffects: " + dragEffects);
                            dragHost.DragBegin();

                            var avdo = await dragHost.GetDragDataObjectAsync();
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
                if(was_drag_started) {
                    // this should not happen, or release is called before drop (if its called at all during drop
                    // release should be removed after drop
                    Debugger.Break();
                }
                dragControl.PointerMoved -= dragControl_PointerMoved_Handler;
                dragControl.PointerReleased -= dragControl_PointerReleased_Handler;
                MpConsole.WriteLine("Drag End (pointer released)");

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

        #region Drop

        #region Drop Adorner

        private static void HandleDropAdornedControlChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            // NOTE only removed in detach or disable
            if (element is Control dropControl) {
                AddOrRemoveDropAdorner(dropControl, true);
            }
        }


        private static void AddOrRemoveDropAdorner(Control dropControl, bool isAdd) {
            var dropAdornedControl = GetDropAdornedControl(dropControl);
            if (dropAdornedControl == null) {
                return;
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer(dropAdornedControl);

            if (adornerLayer == null) {
                Dispatcher.UIThread.Post(async () => {
                    adornerLayer = AdornerLayer.GetAdornerLayer(dropAdornedControl);
                    while(adornerLayer == null) {
                        await Task.Delay(100);
                    }
                    AddOrRemoveDropAdorner(dropControl, isAdd);
                });
                return;
            }

            MpAvDropHostAdorner dropAdorner = GetDropAdorner(dropAdornedControl);
            if (isAdd) {
                if (dropAdorner != null) {
                    MpConsole.WriteLine("Warning! dropcontrol already has adorner not adding");
                    return;
                }
                dropAdorner = new MpAvDropHostAdorner(dropAdornedControl);
                adornerLayer.Children.Add(dropAdorner);
                AdornerLayer.SetAdornedElement((Visual)dropAdorner, dropAdornedControl);
                MpConsole.WriteLine("Adorner added to control: " + dropAdornedControl);
                return;
            }

            if (dropAdorner == null) {
                MpConsole.WriteLine("Warning! dropcontrol doesn't have adorner can't remove");
                return;
            }
            adornerLayer.Children.Remove(dropAdorner);
            MpConsole.WriteLine("Adorner removed for control: " + dropAdornedControl);
        }

        #endregion

        private static void DropControls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) {
                        return;
                    }
                    foreach (var ni in e.NewItems) {
                        if (ni is Control control) {
                            AddOrRemoveDropHandlers(control, true);
                            //AddOrRemoveDropAdorner(control, true);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.NewItems == null) {
                        return;
                    }
                    foreach (var ni in e.NewItems) {
                        if (ni is Control control) {
                            AddOrRemoveDropHandlers(control, false);
                            // AddOrRemoveDropAdorner(control, false);
                        }
                    }
                    break;
            }
        }

        private static void AddOrRemoveDropHandlers(Control dropControl, bool isAdd) {
            if(isAdd) {
                dropControl.AddHandler(DragDrop.DropEvent, Drop);
                dropControl.AddHandler(DragDrop.DragOverEvent, DragOver);
                dropControl.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
                DragDrop.SetAllowDrop(dropControl, true);
            } else {
                dropControl.RemoveHandler(DragDrop.DropEvent, Drop);
                dropControl.RemoveHandler(DragDrop.DragOverEvent, DragOver);
                dropControl.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
                DragDrop.SetAllowDrop(dropControl, false);
            }

            MpConsole.WriteLine("DropHandler " + (isAdd ? "ADDED":"REMOVED")+" for Control Type: " + dropControl.GetType() + " DataContext: " + dropControl.DataContext);
        }


        private static void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("Drag Over: " + sender);

            MpAvIDropHost dropHost = GetDropHost(sender);
            e.DragEffects = GetDropEffects(dropHost, e);

            if(dropHost == null) {
                return;
            }

            MpPoint drag_mp = e.GetPosition((IVisual)sender).ToPortablePoint();
            dropHost.DragOver(drag_mp, e.Data, e.DragEffects);
        }

        private static void DragLeave(object sender, DragEventArgs e) {
            MpConsole.WriteLine("Drag Leave: " + sender);

            MpAvIDropHost dropHost = GetDropHost(sender);
            e.DragEffects = GetDropEffects(dropHost, e);

            dropHost.DragLeave();
        }

        private static async void Drop(object sender, DragEventArgs e) {
            MpConsole.WriteLine("Drop: " + sender);
            MpAvIDropHost dropHost = GetDropHost(sender);
            e.DragEffects = GetDropEffects(dropHost, e);
            if(dropHost == null) {
                return;
            }
            e.DragEffects = await dropHost.DropDataObjectAsync(e.Data, e.DragEffects);
        }

        #endregion

        private static MpAvIDropHost GetDropHost(object sender) {
            if (sender is Control control) {
                if (control.Name == "PinTrayListBox" &&
                    control.GetVisualAncestor<MpAvPinTrayView>() is MpAvPinTrayView ptrv) {
                    return ptrv;
                }
            }
            return null;
        }

        private static DragDropEffects GetDropEffects(MpAvIDropHost dropHost, DragEventArgs e) {
            if (dropHost == null) {
                return DragDropEffects.None;
            }
            if (!dropHost.IsDropValid(e.Data)) {
                return DragDropEffects.None;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                return DragDropEffects.Copy;
            } 
            return DragDropEffects.Move;
        }

        #endregion

        #region Public Methods

        public static MpAvDropHostAdorner GetDropAdorner(Control adornedControl) {
            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedControl);
            var drop_adorner = adornerLayer
                    .Children
                    .FirstOrDefault(x => x is MpAvDropHostAdorner ca && ca.AdornedControl == adornedControl);
            return drop_adorner as MpAvDropHostAdorner;
        }
        #endregion
    }

}
