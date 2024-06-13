using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel>, MpAvISidebarContentView, MpAvIFocusHeaderMenuView {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
        }


        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if(MpAvSidebarItemTemplateSelector.ContentScrollViewer == null) {
                return;
            }
            MpAvSidebarItemTemplateSelector.ContentScrollViewer.ScrollChanged += ContentScrollViewer_ScrollChanged;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            if (MpAvSidebarItemTemplateSelector.ContentScrollViewer == null) {
                return;
            }
            MpAvSidebarItemTemplateSelector.ContentScrollViewer.ScrollChanged -= ContentScrollViewer_ScrollChanged;
        }

        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if(!MpAvThemeViewModel.Instance.IsMobileOrWindowed ||
                sender is not ScrollViewer sv ||
                this.GetVisualDescendant<MpAvZoomBorder>() is not { } zb ||
                !zb.IsKeyboardFocusWithin) {
                return;
            }
            // 
            sv.Offset -= e.OffsetDelta;
        }
    }
}
