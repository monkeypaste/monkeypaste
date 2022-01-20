using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpColorChooserContextMenuItemView : ContentView {
        public MpColorChooserContextMenuItemView() {
            InitializeComponent();
            
            for (int r = 0;r < 5;r++) {
                ColorPalleteGrid.RowDefinitions.Add(new RowDefinition());
                for(int c = 0;c < 14;c++) {
                    if(c == 0) {
                        ColorPalleteGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                    var f = new Frame();
                    f.Margin = 0;
                    f.WidthRequest = 12;
                    f.HeightRequest = 12;
                    f.BackgroundColor = MpHelpers.GetRandomColor();
                    f.BorderColor = Color.Black;
                    var tr = new TapGestureRecognizer() {
                        NumberOfTapsRequired = 1
                    };
                    tr.Tapped += Tr_Tapped;
                    f.GestureRecognizers.Add(tr);
                    ColorPalleteGrid.Children.Add(f, c, r);
                } 
            }
        }

        private void Tr_Tapped(object sender, EventArgs e) {
            var r = Grid.GetRow(sender as BindableObject);
            var c = Grid.GetColumn(sender as BindableObject);
            MpConsole.WriteLine($"Tapped Item {r} {c}");
            (sender as Frame).BorderColor = Color.Red;
        }
    }
}