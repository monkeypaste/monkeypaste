using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAboutView : MpAvUserControl<MpAvAboutViewModel> {
        private DispatcherTimer _dispatcherTimer;

        public MpAvAboutView() : base() {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e) {
            var ctb = this.FindControl<Control>("CreditsTextBlock");
            if (ctb.IsPointerOver ||
                BindingContext.IsOverCredits ||
                ctb.Parent is not ScrollViewer sv) {
                return;
            }
            sv.ScrollByPointDelta(new MpPoint(0, 1));
        }

    }
}
