using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMenuView : ContextMenu {
        const bool DEFAULT_SHOW_BY_POINTER =
#if MULTI_WINDOW
            true;
#else
            false;
#endif
        static bool _IsDevToolsOpen = false;
        static ContextMenu _cm;

        protected override Type StyleKeyOverride => typeof(ContextMenu);
        public MpAvIMenuItemViewModel BindingContext {
            get => DataContext as MpAvIMenuItemViewModel;
            set => DataContext = value;
        }

        public static void Init() {
            // first context menu open is a lil slow, i 'think' doing this makes it a lil faster :/opy 
            _cm = new MpAvMenuView();
        }
        public static void CloseMenu() {
            if (_cm != null) {
                _cm.Close();
            }
            _cm = null;
        }

        public static MpAvMenuView ShowMenu(
            Control target,
            MpAvIMenuItemViewModel dc,
            bool showByPointer = DEFAULT_SHOW_BY_POINTER,
            PlacementMode placementMode = PlacementMode.Pointer,
            PopupAnchor popupAnchor = PopupAnchor.TopLeft,
            MpPoint offset = null) {
            if (target == null) {
                return null;
            }
            if(!target.IsAttachedToVisualTree()) {
                while(target != null) {
                    target = target.Parent as Control;
                    if(target != null && target.IsAttachedToVisualTree()) {
                        break;
                    }
                }
                if(target == null) {
                    return null;
                }
            }

            if (showByPointer) {
                placementMode = PlacementMode.TopEdgeAlignedLeft;
                popupAnchor = PopupAnchor.TopLeft;
                offset = MpPoint.Zero;

                if (!MpAvShortcutCollectionViewModel.Instance.IsGlobalHooksPaused &&
                    MpAvWindowManager.GetTopLevel(target) is TopLevel tl) {
                    var tlp = tl.PointToClient(MpAvShortcutCollectionViewModel.Instance.GlobalScaledMouseLocation.ToAvPixelPoint(tl.VisualPixelDensity()));
                    offset = tl.TranslatePoint(tlp, target).Value.ToPortablePoint();
                }
            }

            target.ContextMenu = new MpAvMenuView() {
                DataContext = dc,
                Placement = placementMode,
                PlacementAnchor = popupAnchor,
                HorizontalOffset = offset == null ? 0 : offset.X,
                VerticalOffset = offset == null ? 0 : offset.Y,
            };

            void OnContextMenuOpened(object sender, EventArgs e) {
                if(sender is not ContextMenu cm) {
                    return;
                }
                _cm = cm;
            }
            void OnContextMenuClosed(object sender, EventArgs e) {
                if(sender is not ContextMenu cm) {
                    return;
                }
                cm.Opened -= OnContextMenuOpened;
                cm.Closed -= OnContextMenuClosed;
                if(cm == _cm) {
                    _cm = null;
                }                
            }
            target.ContextMenu.Opened += OnContextMenuOpened;
            target.ContextMenu.Closed += OnContextMenuClosed;

            target.ContextMenu.Open(target);

            return target.ContextMenu as MpAvMenuView;
        }

        public MpAvMenuView() : base() {
            InitializeComponent();

            this.GetObservable(IsOpenProperty).Subscribe(value => OnIsOpenChanged());
            this.Closed += MpAvMenuView_Closed;
            this.Closing += MpAvMenuView_Closing;
        }


        private void MpAvMenuView_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = _IsDevToolsOpen;
        }

        private void MpAvMenuView_Closed(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.PlacementTarget is Control target &&
                target.ContextMenu == this) {
                target.ContextMenu = null;
            }
        }

        private void OnIsOpenChanged() {
            if (BindingContext == null) {
                return;
            }
            BindingContext.IsSubMenuOpen = IsOpen;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            if (e.IsMiddleRelease(this)) {
                _IsDevToolsOpen = !_IsDevToolsOpen;
                if (_IsDevToolsOpen) {
                    bool success = this.Focus();
                    MpConsole.WriteLine($"Context menu focus success: {success}");
                    Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("F12");
                }
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
#if DEBUG
            if (MpAvWindowManager.GetTopLevel(this) is TopLevel tl) {
                tl.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
            }
#endif
        }
    }
}
