using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;
using System.Collections.Generic;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia;

[DoNotNotify]
public partial class MpAvMobileHeaderView : UserControl
{
    #region Properties


    #region Title Property
    public string Title {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<MpAvMobileHeaderView, string>(
            name: nameof(Title),
            defaultValue: default);

    #endregion
    #region MenuItems

    public static readonly AttachedProperty<IEnumerable<MpAvIMenuItemViewModel>> MenuItemsProperty =
        AvaloniaProperty.RegisterAttached<MpAvMobileHeaderView, Control, IEnumerable<MpAvIMenuItemViewModel>>(
            nameof(MenuItems), default, false);
    public IEnumerable<MpAvIMenuItemViewModel> MenuItems {
        get => GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }
    #endregion


    #region BackCommand

    public static readonly AttachedProperty<ICommand> BackCommandProperty =
        AvaloniaProperty.RegisterAttached<MpAvMobileHeaderView, Control, ICommand>(
            nameof(BackCommand),
            new MpCommand<object>(
                (args) => {
#if MOBILE_OR_WINDOWED
                    if (args is not MpAvChildWindow cw) {
                        cw = MpAvWindowManager.ActiveWindow;
                        if (cw == null) {
                            return;
                        }
                    }
                    cw.Close();
#else
                        return;
#endif
                }));

    public ICommand BackCommand {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    #endregion

    #region BackCommandParameter

    public static readonly AttachedProperty<object> BackCommandParameterProperty =
        AvaloniaProperty.RegisterAttached<MpAvMobileHeaderView, Control, object>(
            nameof(BackCommandParameter));

    public object BackCommandParameter {
        get => GetValue(BackCommandParameterProperty) ?? this;
        set => SetValue(BackCommandParameterProperty, value);
    }

    #endregion
    #endregion

    public MpAvMobileHeaderView()
    {
        InitializeComponent();
    }
}