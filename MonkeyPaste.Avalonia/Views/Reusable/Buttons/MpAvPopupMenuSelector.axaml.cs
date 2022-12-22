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
using Avalonia.Styling;
using System.Windows.Input;
using Avalonia.Data;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvPopupMenuSelector : MpAvUserControl<MpIPopupMenuViewModel> {
        #region Overrides

        #endregion

        #region Properties

        #region EmptyText

        private string _emptyText;

        public static readonly DirectProperty<MpAvPopupMenuSelector, string> EmptyTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, string>
            (
                nameof(EmptyText),
                o => o.EmptyText,
                (o, v) => o.EmptyText = v,
                "Select..."
            );

        public string EmptyText {
            get => _emptyText;
            set {
                SetAndRaise(EmptyTextProperty, ref _emptyText, value);
            }
        }

        #endregion 

        #region EmptyIconResourceObj

        private object _emptyIconResourceObj = default;

        public static readonly DirectProperty<MpAvPopupMenuSelector, object> EmptyIconResourceObjProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, object>
            (
                nameof(EmptyIconResourceObj),
                o => o.EmptyIconResourceObj,
                (o, v) => o.EmptyIconResourceObj = v
            );

        public object EmptyIconResourceObj {
            get => _emptyIconResourceObj;
            set {
                SetAndRaise(EmptyIconResourceObjProperty, ref _emptyIconResourceObj, value);
            }
        }

        #endregion 

        #region SelectedMenuItem

        private MpMenuItemViewModel _selectedMenuItem = default;

        public static readonly DirectProperty<MpAvPopupMenuSelector, MpMenuItemViewModel> SelectedMenuItemProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, MpMenuItemViewModel>
            (
                nameof(SelectedMenuItem),
                o => o.SelectedMenuItem,
                (o, v) => o.SelectedMenuItem = v
            );

        public MpMenuItemViewModel SelectedMenuItem {
            get => _selectedMenuItem;
            set {
                SetAndRaise(SelectedMenuItemProperty, ref _selectedMenuItem, value);
            }
        }

        #endregion 

        #region PopupMenu

        private MpMenuItemViewModel _popupMenu = default;

        public static readonly DirectProperty<MpAvPopupMenuSelector, MpMenuItemViewModel> PopupMenuProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, MpMenuItemViewModel>
            (
                nameof(PopupMenu),
                o => o.PopupMenu,
                (o, v) => o.PopupMenu = v
            );

        public MpMenuItemViewModel PopupMenu {
            get => _popupMenu;
            set {
                SetAndRaise(SelectedMenuItemProperty, ref _popupMenu, value);
            }
        }

        #endregion 

        #region IsBusy

        private bool _isBusy = default;

        public static readonly DirectProperty<MpAvPopupMenuSelector, bool> IsBusyProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, bool>
            (
                nameof(IsBusy),
                o => o.IsBusy,
                (o, v) => o.IsBusy = v
            );

        public bool IsBusy {
            get => _isBusy;
            set {
                SetAndRaise(IsBusyProperty, ref _isBusy, value);
            }
        }

        #endregion


        #region ClearCommands

        private bool _clearCommands = true;

        public static readonly DirectProperty<MpAvPopupMenuSelector, bool> ClearCommandsProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, bool>
            (
                nameof(ClearCommands),
                o => o.ClearCommands,
                (o, v) => o.ClearCommands = v
            );

        public bool ClearCommands {
            get => _clearCommands;
            set {
                SetAndRaise(ClearCommandsProperty, ref _clearCommands, value);
            }
        }

        #endregion 

        #region IsPopupOpen

        private bool _isPopupOpen = default;

        public static readonly DirectProperty<MpAvPopupMenuSelector, bool> IsPopupOpenProperty =
            AvaloniaProperty.RegisterDirect<MpAvPopupMenuSelector, bool>
            (
                nameof(IsPopupOpen),
                o => o.IsPopupOpen,
                (o, v) => o.IsPopupOpen = v,
                false,
                BindingMode.TwoWay
            );

        public bool IsPopupOpen {
            get => _isPopupOpen;
            set {
                SetAndRaise(IsPopupOpenProperty, ref _isPopupOpen, value);
            }
        }

        #endregion 


        #endregion
        public MpAvPopupMenuSelector() {
            InitializeComponent();
            var pub = this.FindControl<Button>("PopupButton");
            pub.Click += Pub_Click;
        }

        private void Pub_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (PopupMenu == null) {
                return;
            }
            var caret_button = sender as Control;
            var pmvm = PopupMenu;
            if(ClearCommands) {
                pmvm.ClearCommands();
            }
            MpAvMenuExtension.ShowMenu(caret_button, pmvm);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
