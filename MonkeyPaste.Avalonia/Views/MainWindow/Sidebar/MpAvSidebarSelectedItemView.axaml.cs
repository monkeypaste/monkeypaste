using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia;
[DoNotNotify]
public partial class MpAvSidebarSelectedItemView : MpAvUserControl<MpAvSidebarItemCollectionViewModel>
{
    public MpAvSidebarSelectedItemView()
    {
        InitializeComponent();
    }
}