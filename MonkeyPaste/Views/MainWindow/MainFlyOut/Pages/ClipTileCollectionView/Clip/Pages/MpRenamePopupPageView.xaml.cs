using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpRenamePopupPageView : PopupPage {
        public bool WasCanceled = false;

        public event EventHandler<string> OnComplete;

        public MpRenamePopupPageView(string orgTitle) {            
            InitializeComponent();
            TitleEntry.Text = orgTitle;
        }
        private void OnClose(object sender, EventArgs e) {
            WasCanceled = true;
            OnComplete?.Invoke(this, TitleEntry.Text);
        }

        private void OnOk(object sender, EventArgs e) {
            OnComplete?.Invoke(this, TitleEntry.Text);
        }

        //protected override Task OnAppearingAnimationEndAsync() {
        //    return Content.FadeTo(0.5);
        //}

        //protected override Task OnDisappearingAnimationBeginAsync() {
        //    return Content.FadeTo(1);
        //}
    }
}