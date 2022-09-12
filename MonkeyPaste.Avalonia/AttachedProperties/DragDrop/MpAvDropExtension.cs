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
using System.Threading.Tasks;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDropExtension {
        static MpAvDropExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            DropAdornedControlProperty.Changed.AddClassHandler<Control>((x, y) => HandleDropAdornedControlChanged(x, y));
        }


        #region Properties

        public static MpAvIDropHost CurrentDropHost { get; private set; }

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
            if (e.NewValue is bool isEnabled &&
                element is Control dropControl) {
                if (isEnabled) {
                    if (dropControl.IsInitialized) {
                        DropControl_AttachedToVisualTree(dropControl, null);
                    } else {
                        dropControl.AttachedToVisualTree += DropControl_AttachedToVisualTree;
                    }
                } else {
                    //DropControls.Remove(dropControl);
                    DropControl_DetachedFromVisualTree(element, null);
                }
            }
        }

        #endregion

        #endregion

        #region Control Event Handlers

        private static void DropControl_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Control dropControl) {
                dropControl.DetachedFromVisualTree += DropControl_DetachedFromVisualTree;

                if (e == null) {
                    dropControl.AttachedToVisualTree += DropControl_AttachedToVisualTree;
                }
                AddOrRemoveDropHandlers(dropControl, true);
            }
        }

        private static void DropControl_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Control dropControl) {
                AddOrRemoveDropHandlers(dropControl, false);

                dropControl.DetachedFromVisualTree -= DropControl_DetachedFromVisualTree;
                dropControl.AttachedToVisualTree -= DropControl_AttachedToVisualTree;
                MpConsole.WriteLine("DropControl falsed and removed. Control Type: " + dropControl.GetType() + " DataContext: " + dropControl.DataContext);
            }
        }

        #endregion

        #region DragDrop

        #region Drop

        private static void AddOrRemoveDropHandlers(Control dropControl, bool isAdd) {
            if (isAdd) {
                DragDrop.SetAllowDrop(dropControl, true);
                dropControl.AddHandler(DragDrop.DragEnterEvent, DragEnter);
                dropControl.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
                dropControl.AddHandler(DragDrop.DragOverEvent, DragOver);
                dropControl.AddHandler(DragDrop.DropEvent, Drop);
            } else {
                dropControl.RemoveHandler(DragDrop.DropEvent, Drop);
                dropControl.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
                dropControl.RemoveHandler(DragDrop.DragOverEvent, DragOver);
                dropControl.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
                DragDrop.SetAllowDrop(dropControl, false);
            }

            MpConsole.WriteLine("DropHandler " + (isAdd ? "ADDED" : "REMOVED") + " for Control Type: " + dropControl.GetType() + " DataContext: " + dropControl.DataContext);
        }


        private static void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine($"[Drag Over] Source: '{e.Source}' Datat: '{e.Data}'");            

            MpAvIDropHost dropHost = GetDropHost(sender); 
            e.DragEffects = GetDropEffects(e);
            MpPoint drop_host_mp = e.GetPosition(dropHost as IVisual).ToPortablePoint();
            bool isDropValid = dropHost.IsDropValid(e.Data, drop_host_mp, e.DragEffects);
            if(isDropValid) {

            } else { 
                // currently pin tray drop is only invlaid on move to same idx
                e.DragEffects = DragDropEffects.None;
            }
            dropHost.DragOver(drop_host_mp, e.Data, e.DragEffects);
        }
        private static void DragEnter(object sender, DragEventArgs e) {            
            MpAvIDropHost dropHost = GetDropHost(sender);            
            e.DragEffects = GetDropEffects(e);
            MpPoint drop_host_mp = e.GetPosition(dropHost as IVisual).ToPortablePoint();
            bool isDropValid = dropHost.IsDropValid(e.Data, drop_host_mp, e.DragEffects);
            if (isDropValid && dropHost.IsDropEnabled) {
                CurrentDropHost = dropHost;
            } 

            DragOver(sender, e);
        }
        private static void DragLeave(object sender, RoutedEventArgs e) {
            //MpConsole.WriteLine("Drag Leave: " + sender);

            MpAvIDropHost dropHost = GetDropHost(sender);
            CurrentDropHost = null;
            //if (dropHost == CurrentDropHost &&
            //    dropHost is Control dropHostControl) {
            //    //since drag leave is not called on enter of a child drop host
            //    // pass current to parent or when mw grid drag will be out of app
            //    var parent_drop_host = dropHostControl.GetVisualAncestors().FirstOrDefault(x => x is MpAvIDropHost) as MpAvIDropHost;
            //    while (parent_drop_host != null) {
            //        //set closest enabled parent to drop host or null if none
            //        if (parent_drop_host.IsDropEnabled) {
            //            CurrentDropHost = parent_drop_host;
            //            break;
            //        }
            //        if (parent_drop_host is Window w && App.Desktop.MainWindow == w) {
            //            parent_drop_host = null;
            //            break;
            //        }
            //        parent_drop_host = dropHostControl.GetVisualAncestors().FirstOrDefault(x => x is MpAvIDropHost) as MpAvIDropHost;
            //    }
            //} 
            //else if (sender is Control control && control.Parent is Window) {
            //    // catch drag leave app to remove drop host
            //    CurrentDropHost = null;
            //}
            dropHost?.DragLeave();
        }

        private static async void Drop(object sender, DragEventArgs e) {
            MpConsole.WriteLine("Drop: " + sender);
            MpAvIDropHost dropHost = GetDropHost(sender);
            e.DragEffects = GetDropEffects( e);
            if (dropHost == null || dropHost != CurrentDropHost) {
                return;
            }
            CurrentDropHost = null;
            MpPoint drop_host_mp = e.GetPosition(dropHost as IVisual).ToPortablePoint();
            
            e.DragEffects = await dropHost.DropDataObjectAsync(e.Data, drop_host_mp, e.DragEffects);
        }

        #endregion

        #region Drop Adorner

        private static void HandleDropAdornedControlChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            // NOTE only removed in detach or disable
            if (element is Control dropAdornedControl) {
                if (dropAdornedControl.IsInitialized) {
                    DropAdornedControl_AttachedToVisualTree(dropAdornedControl, null);
                } else {
                    dropAdornedControl.AttachedToVisualTree += DropAdornedControl_AttachedToVisualTree;

                }
            }
        }

        private static void DropAdornedControl_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Control dropAdornedControl) {
                dropAdornedControl.DetachedFromVisualTree += DropAdornedControl_DetachedFromVisualTree;

                AddOrRemoveDropAdorner(dropAdornedControl, true);
                if (e == null) {
                    dropAdornedControl.AttachedToVisualTree += DropControl_AttachedToVisualTree;
                }
            }
        }

        private static void DropAdornedControl_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Control dropAdornedControl) {
                AddOrRemoveDropAdorner(dropAdornedControl, false);
                dropAdornedControl.DetachedFromVisualTree -= DropAdornedControl_DetachedFromVisualTree;
                dropAdornedControl.AttachedToVisualTree -= DropControl_AttachedToVisualTree;
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
                    while (adornerLayer == null) {
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
                AdornerLayer.SetAdornedElement(dropAdorner, dropAdornedControl);
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

        #region Drop Helper Methods

        private static MpAvIDropHost GetDropHost(object sender) {
            if (sender is Control control) {
                //if (control.Name == "PinTrayListBox" &&
                //    control.GetVisualAncestor<MpAvPinTrayView>() is MpAvPinTrayView ptrv) {
                //    return ptrv;
                //}
                if(control is MpAvIDropHost) {
                    return (MpAvIDropHost)control;
                }
                var dropHost = control.GetVisualAncestors<IVisual>().FirstOrDefault(x => x is MpAvIDropHost);
                return (MpAvIDropHost)dropHost;
            }
            return null;
        }

        private static DragDropEffects GetDropEffects(DragEventArgs e) {
            if (MpAvDragExtension.CurrentDragHost == null) {
                // this should only happen from external drag

                //test data object conversion to validate
                var mpdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToSupportedPortableFormats(e.Data);
                if (mpdo.DataFormatLookup.Count == 0) {
                    // external drop data has no supported formats so invalidate
                    return DragDropEffects.None;
                }
            }
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                return DragDropEffects.Copy;
            }
            return DragDropEffects.Move;
        }

        #endregion

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
