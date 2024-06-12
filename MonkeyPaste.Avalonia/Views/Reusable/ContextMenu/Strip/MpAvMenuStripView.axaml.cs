
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia;

[DoNotNotify]
public partial class MpAvMenuStripView : UserControl
{
    public MpAvMenuStripView()
    {
        InitializeComponent();
    }

    private void StripButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
        //if(sender is not Control c || c.DataContext is not MpAvIMenuItemViewModel mivm) {
        //    return;
        //}
        //object arg = mivm.CommandParameter ?? c;
        //bool was_frozen = MpAvMainWindowTitleMenuViewModel.Instance.IsFocusHeaderFrozen;
        //if (c.GetVisualAncestor<MpAvMainWindowTitleMenuView>() != null) {
        //    MpAvMainWindowTitleMenuViewModel.Instance.IsFocusHeaderFrozen = true;
        //}
        //mivm.Command.Execute(mivm.CommandParameter ?? c);
        //MpAvMainWindowTitleMenuViewModel.Instance.IsFocusHeaderFrozen = was_frozen;
    }
}