using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemView : ContentView
    {
        public MpCopyItemView() : this(new MpCopyItemViewModel()) { }

        public MpCopyItemView(MpCopyItemViewModel viewModel) : base()
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}