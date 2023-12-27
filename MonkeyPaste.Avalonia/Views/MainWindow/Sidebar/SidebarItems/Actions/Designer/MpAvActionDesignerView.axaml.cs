using Avalonia;
using Avalonia.Threading;
using System.Threading.Tasks;


namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpActionDesignerView.xaml
    /// </summary>
    public partial class MpAvActionDesignerView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvActionDesignerView() {
            InitializeComponent();

        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if (BindingContext == null || BindingContext.HasShown) {
                return;
            }
            BindingContext.HasShown = true;
            Dispatcher.UIThread.Post(async () => {
                await Task.Delay(300);
                BindingContext.ResetDesignerViewCommand.Execute(null);
            });
        }
    }
}
