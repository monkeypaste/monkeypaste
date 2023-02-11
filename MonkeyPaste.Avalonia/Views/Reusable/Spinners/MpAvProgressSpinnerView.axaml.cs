using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using PropertyChanged;
using System.Diagnostics;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using Avalonia.Input;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvProgressSpinnerView : MpAvUserControl<MpIProgressIndicatorViewModel> {
        #region Private Variables

        #endregion
        public MpAvProgressSpinnerView() {
            InitializeComponent();
            //this.AttachedToVisualTree += MpAvProgressSpinnerView_AttachedToVisualTree;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void MpAvProgressSpinnerView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            Test();
        }

        private void Test() {
            Dispatcher.UIThread.Post(async () => {
                var ps = this.GetVisualDescendant<MpAvProgressSpinner>();
                while(true) {
                    if(ps == null) {
                        ps = this.GetVisualDescendant<MpAvProgressSpinner>();
                        if(ps != null) {
                            ps.Percent = 0;
                        } else {

                            await Task.Delay(100);
                            continue;
                        }
                        
                    }
                    await Task.Delay(100);
                    ps.Percent += 0.01;
                    if(ps.Percent > 1) {
                        ps.Percent = 0;
                    }
                    ps.InvalidateVisual();
                }
            });
        }
    }
}
