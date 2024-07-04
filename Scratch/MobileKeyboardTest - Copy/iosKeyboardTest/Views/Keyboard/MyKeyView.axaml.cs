using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;


using System.Linq;
using System.Windows.Input;

namespace iosKeyboardTest
{
    public partial class MyKeyView : UserControl
    {
        #region Properties

        #region TapCommand AvaloniaProperty
        public ICommand TapCommand
        {
            get { return (ICommand)GetValue(TapCommandProperty); }
            set { SetValue(TapCommandProperty, value); }
        }

        public static readonly StyledProperty<ICommand> TapCommandProperty =
            AvaloniaProperty.Register<MyKeyView, ICommand>(
                nameof(TapCommand),
                null);

        #endregion
        
        #region TapCommandParameter AvaloniaProperty
        public object TapCommandParameter
        {
            get { return (object)GetValue(TapCommandParameterProperty); }
            set { SetValue(TapCommandParameterProperty, value); }
        }

        public static readonly StyledProperty<object> TapCommandParameterProperty =
            AvaloniaProperty.Register<MyKeyView, object>(
                nameof(TapCommandParameter),
                null);

        #endregion

        #region HoldCommand AvaloniaProperty
        public ICommand HoldCommand
        {
            get { return (ICommand)GetValue(HoldCommandProperty); }
            set { SetValue(HoldCommandProperty, value); }
        }

        public static readonly StyledProperty<ICommand> HoldCommandProperty =
            AvaloniaProperty.Register<MyKeyView, ICommand>(
                nameof(HoldCommand),
                null);

        #endregion

        #region HoldCommandParameter AvaloniaProperty
        public object HoldCommandParameter
        {
            get { return (object)GetValue(HoldCommandParameterProperty); }
            set { SetValue(HoldCommandParameterProperty, value); }
        }

        public static readonly StyledProperty<object> HoldCommandParameterProperty =
            AvaloniaProperty.Register<MyKeyView, object>(
                nameof(HoldCommandParameter),
                null);

        #endregion

        #endregion

        public MyKeyView()
        {
            InitializeComponent();
            this.PointerPressed += MyKeyView_PointerPressed;
            this.PointerReleased += MyKeyView_PointerReleased;
            this.Holding += MyKeyView_Holding;
        }

        private void MyKeyView_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            this.Classes.Add("pressed");
        }

        private void MyKeyView_Holding(object sender, Avalonia.Input.HoldingRoutedEventArgs e)
        {
            if(HoldCommand == null)
            {
                return;
            }
            HoldCommand.Execute(HoldCommandParameter);
        }

        private void MyKeyView_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            this.Classes.Remove("pressed");
            if(TapCommand == null)
            {
                return;
            }
            TapCommand.Execute(TapCommandParameter);
        }
    }
}