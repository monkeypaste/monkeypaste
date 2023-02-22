using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Platform;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIHasModifiableContent {
        void SetContent(object content);
    }
    [DoNotNotify]
    public class MpAvNativeWebViewContainer : NativeControlHost {
        public static MpAvINativeControlBuilder Implementation { get; set; }

        //private readonly Window _hostedWindow = CreateHostedWindow();
        private IPlatformHandle _handle;

        public static readonly StyledProperty<Control> ChildProperty =
            AvaloniaProperty.Register<MpAvNativeWebViewContainer, Control>(nameof(Child));

        [Content]
        public Control Child {
            get => GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        static MpAvNativeWebViewContainer() {
            ChildProperty.Changed.AddClassHandler<MpAvNativeWebViewContainer>((host, e) => host.ChildChanged(e));
        }

        public MpAvNativeWebViewContainer() : base() {
            //_handle =
            //        Implementation.Build(
            //            parent,
            //            () => base.CreateNativeControlCore(parent),
            //            MpAvClipTrayViewModel.EditorUri);
            //InitWindow(_hostedWindow);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            //if (_hostedWindow.PlatformImpl.Handle is IMacOSTopLevelPlatformHandle macOSTopLevelPlatformHandle) {
            //    return new PlatformHandle(macOSTopLevelPlatformHandle.NSView, nameof(macOSTopLevelPlatformHandle.NSView));
            //}

            //return _hostedWindow.PlatformImpl.Handle;
            if (Implementation == null) {
                _handle = base.CreateNativeControlCore(parent);
            } else {
                _handle =
                    Implementation.Build(
                        parent,
                        () => base.CreateNativeControlCore(parent),
                        MpAvClipTrayViewModel.EditorUri);

            }

            return _handle;

        }

        private void ChildChanged(AvaloniaPropertyChangedEventArgs e) {
            if (e.OldValue is Control oldChild &&
                _handle is MpIHasModifiableContent hmc) {
                ((ISetLogicalParent)oldChild).SetParent(null);
                LogicalChildren.Clear();
                //_hostedWindow.Content = null;
                hmc.SetContent(null);
            }

            if (e.NewValue is Control newChild &&
                _handle is MpIHasModifiableContent hmc2) {
                ((ISetLogicalParent)newChild).SetParent(this);
                //_hostedWindow.Content = newChild;
                hmc2.SetContent(newChild);
                LogicalChildren.Add(newChild);
            }
        }

        //private void InitWindow(Window window) {
        //    _hostedWindow[!DataContextProperty] = this[!DataContextProperty];

        //    window.RaiseEvent(new RoutedEventArgs(Window.WindowOpenedEvent));
        //    window.BeginInit();
        //    window.EndInit();
        //    window.IsVisible = true;
        //    window.LayoutManager.ExecuteInitialLayoutPass();
        //    window.Renderer.Start();
        //}

        //private static Window CreateHostedWindow() {
        //    return new Window() {
        //        SystemDecorations = SystemDecorations.None,
        //        // Not setting this property to 'true' causes one more transparent window (this window)
        //        // to appear in window switcher when you press Alt + Tab in Windows.
        //        ShowInTaskbar = true,
        //        // Do not activate window automatically, it will be activated manually on click or on focus.
        //        ShowActivated = false,
        //        // Window shouldn't be resizable it should automatically fill given area.
        //        CanResize = false
        //    };
        //}
    }
}
