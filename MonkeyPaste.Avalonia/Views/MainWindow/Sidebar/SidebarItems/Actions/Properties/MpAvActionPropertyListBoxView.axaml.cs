using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAvActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpAvActionPropertyListBoxView : MpAvUserControl<MpAvActionCollectionViewModel> {
        public MpAvActionPropertyListBoxView() {
            InitializeComponent();
            //var aplb = this.FindControl<ListBox>("ActionPropertyListBox");
            //aplb.SelectionChanged += Aplb_SelectionChanged;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void Aplb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lb = sender as ListBox;
            //if(lb != null && e.AddedItems != null && e.AddedItems.Count > 0) {
            //    lb.ScrollIntoView(e.AddedItems[e.AddedItems.Count - 1]);
            //}
        }
    }
}
