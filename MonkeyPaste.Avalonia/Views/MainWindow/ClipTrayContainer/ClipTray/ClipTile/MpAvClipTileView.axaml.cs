using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            AvaloniaXamlLoader.Load(this);
            DetachedFromLogicalTree += MpAvClipTileView_DetachedFromLogicalTree;
            DataContextChanged += MpAvClipTileView_DataContextChanged;
            PointerMoved += MpAvClipTileView_PointerMoved;

            InitPinPlaceholder();
        }

        #region Pin Placeholder
        private void InitPinPlaceholder() {

            this.FindControl<Control>("PinPlaceholderOverlay")
                .GetObservable(IsVisibleProperty)
                .Subscribe(value => OnPinPlaceholderOverlayIsVisibleChanged(value));
        }
        private void OnPinPlaceholderOverlayIsVisibleChanged(bool isVisible) {
            if (MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening ||
                !isVisible) {
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                ListBoxItem pin_placeholder_lbi = await this.GetVisualAncestorAsync<ListBoxItem>();
                if (pin_placeholder_lbi == null) {
                    return;
                }
                BindingContext.OnPropertyChanged(nameof(BindingContext.PinPlaceholderLabel));
                if (string.IsNullOrEmpty(BindingContext.PinPlaceholderLabel)) {

                }
                if (isVisible) {
                    pin_placeholder_lbi.PointerPressed += Pin_placeholder_lbi_PointerPressed;
                    pin_placeholder_lbi.PointerReleased += Pin_placeholder_lbi_PointerReleased;
                } else {
                    pin_placeholder_lbi.PointerPressed -= Pin_placeholder_lbi_PointerPressed;
                    pin_placeholder_lbi.PointerReleased -= Pin_placeholder_lbi_PointerReleased;
                }
            });
        }

        private void Pin_placeholder_lbi_PointerPressed(object sender, PointerPressedEventArgs e) {
            e.Handled = true;
            if (e.ClickCount == 2) {
                // attemp to unpin pin placeholder tile using click location
                MpAvClipTrayViewModel.Instance.UnpinTileCommand.Execute(BindingContext);
            }
        }
        private void Pin_placeholder_lbi_PointerReleased(object sender, PointerReleasedEventArgs e) {
            e.Handled = true;
        }

        #endregion

        private void MpAvClipTileView_DetachedFromLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (BindingContext != null) {
                BindingContext.PropertyChanged -= BindingContext_PropertyChanged;
            }
            DetachedFromLogicalTree -= MpAvClipTileView_DetachedFromLogicalTree;
            DataContextChanged -= MpAvClipTileView_DataContextChanged;
            PointerMoved -= MpAvClipTileView_PointerMoved;
        }

        private void MpAvClipTileView_PointerMoved(object sender, PointerEventArgs e) {
            if (!Mp.Services.PlatformInfo.IsDesktop) {
                return;
            }
            if (!TestDcMismatchByHover()) {
                // dc mismatch
                MpDebug.Break($"DC mismatch. DC: '{this.DataContext}' BC: {BindingContext}");
                Fix();
            }
        }

        private void MpAvClipTileView_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            // BUG workaround for not being able to bind to row definition w/o getting binding null warning
            //switch (e.PropertyName) {
            //    case nameof(BindingContext.IsHovering):
            //        if (!TestDcMismatchByHover()) {
            //            if (BindingContext.IsChildWindowOpen ||
            //                BindingContext.WasCloseAppendWindowConfirmed) {
            //                break;
            //            }
            //            // dc mismatch
            //            MpDebug.Break();
            //            Fix();
            //        }
            //        break;
            //    case nameof(BindingContext.IsHeaderAndFooterVisible):
            //    case nameof(BindingContext.IsTitleVisible):
            //    case nameof(BindingContext.IsDetailVisible):
            //        if (BindingContext.IsChildWindowOpen) {
            //            //return;
            //        }
            //        var tg = this.FindControl<Grid>("TileGrid");
            //        if (tg == null) {
            //            return;
            //        }
            //        string rd = "0.25*,*,20";
            //        if (BindingContext.IsTitleVisible && BindingContext.IsDetailVisible) {

            //        } else if (BindingContext.IsTitleVisible) {
            //            rd = "0.25*,*,0";
            //        } else if (BindingContext.IsDetailVisible) {
            //            rd = "0,*,20";
            //        } else {
            //            rd = "0,*,0";
            //        }
            //        tg.RowDefinitions = new RowDefinitions(rd);
            //        tg.RowDefinitions[0].MaxHeight = 40.0d;
            //        break;
            //}
        }

        private bool TestDcMismatchByHover() {
            //if (this.GetVisualAncestor<MpAvAppendNotificationWindow>() != null) {
            //    return true;
            //}
            return this.IsPointerOver == BindingContext.IsHovering;
        }
        private void Fix() {

            if (this.IsPointerOver != BindingContext.IsHovering) {


                Control target = this;
                if (this.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi) {
                    target = lbi;
                } else if (this.GetVisualAncestor<Window>() is Window w) {
                    target = w;
                }
                if (target.DataContext != BindingContext) {
                    target.DataContext = BindingContext;

                }
            }
        }
    }
}
