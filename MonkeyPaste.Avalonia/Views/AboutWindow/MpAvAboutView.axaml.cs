using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAboutView : MpAvUserControl<MpAvAboutViewModel> {
        private DispatcherTimer _dispatcherTimer;

        public MpAvAboutView() : base() {
            AvaloniaXamlLoader.Load(this);

        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e) {
            var ctb = this.FindControl<TextBlock>("CreditsTextBlock");
            if (ctb.IsPointerOver ||
                BindingContext.IsOverCredits ||
                ctb.Parent is not ScrollViewer sv) {
                return;
            }
            sv.ScrollByPointDelta(new MpPoint(0, 0.3));
        }

    }
}
