using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSearchBoxView : MpAvUserControl<MpAvSearchBoxViewModel> {
        public MpAvSearchBoxView() {
            InitializeComponent();
        }

        protected override async void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            if (BindingContext == null) {
                return;
            }
            await Task.Delay(500);
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsExpanded));
            this?.InvalidateAll();
        }
    }
}
