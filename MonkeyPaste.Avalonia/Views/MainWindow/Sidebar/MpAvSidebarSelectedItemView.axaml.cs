using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia;

public partial class MpAvSidebarSelectedItemView : MpAvUserControl<MpISidebarItemViewModel>
{
    public MpAvSidebarSelectedItemView()
    {
        InitializeComponent();
    }
}