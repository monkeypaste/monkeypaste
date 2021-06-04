using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(ClipId), "ClipId")]
    public partial class MpClipTagAssociationPageView : ContentPage {
        public int ClipId {
            set {
                LoadClip(value);
            }
        }
        public MpClipTagAssociationPageView()  {
            InitializeComponent();
        }

        //public MpClipTagAssociationPageView(MpClipTagAssociationPageViewModel ctapvm) : base() {
        //    InitializeComponent();
        //    //BindingContext = ctapvm;
        //}

        private async void LoadClip(int ciid) {
            try {
                var ci = await MpClip.GetClipById(ciid);
                BindingContext = new MpClipTagAssociationPageViewModel(ci);
            }
            catch (Exception) {
                MpConsole.WriteLine($"Failed to load copy item {ciid}.");
            }
        }
    }
}