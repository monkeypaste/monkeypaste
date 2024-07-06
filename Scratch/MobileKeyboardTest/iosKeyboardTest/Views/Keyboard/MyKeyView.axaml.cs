using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using System;
using System.Linq;
using System.Windows.Input;

namespace iosKeyboardTest
{
    public partial class MyKeyView : UserControl
    {
        #region Private Variables
        private bool _isHolding;
        private Point _down_mp;
        #endregion
        
        #region Statics
        public static FlyoutBase OpenFlyout { get; set; }
        #endregion

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
            if(this.GetVisualAncestors().OfType<MyKeyboardView>().FirstOrDefault() is not { } kbv) {
                return;
            }
            e.Pointer.Capture(kbv);
            _down_mp = e.GetPosition(kbv);
            kbv.PointerMoved += Kbv_PointerMoved;
        }

        private void Kbv_PointerMoved(object sender, PointerEventArgs e) {
            if (this.GetVisualAncestors().OfType<MyKeyboardView>().FirstOrDefault() is not { } kbv) {
                return; 
            }
            if(this.DataContext is not KeyViewModel kvm ||
                kvm.Parent is not KeyboardViewModel kbvm ||
                kbvm.HoldingKeyViewModel is not { } hkvm) {
                kbv.PointerMoved -= Kbv_PointerMoved;
                e.Pointer.Capture(null);
                return;
            }
            var mp = e.GetPosition(kbv);
            var over_tup =
                kbvm.Keys
                .Select(x => (x , new Rect(x.X, x.Y, x.Width, x.Height)))
                .FirstOrDefault(x => x.Item2.Contains(mp));
            if(over_tup.x is not { } over_kvm) {
                return;
            }

            int diff = over_kvm.Column - hkvm.Column;
            int hold_idx = Math.Clamp(diff, 0, hkvm.SecondaryCharacters.Count - 1);
            var holds = kbvm.Keys.Where(x => x.IsHoldKey).ToList();
            for (int i = 0; i < holds.Count; i++) {
                holds[i].IsHoldFocusKey = i == hold_idx;
            }
            kbvm.RefreshKeyboardState();
        }

        private void MyKeyView_Holding(object sender, Avalonia.Input.HoldingRoutedEventArgs e)
        {
            HoldCommand?.Execute(HoldCommandParameter);
        }

        private void MyKeyView_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (this.GetVisualAncestors().OfType<MyKeyboardView>().FirstOrDefault() is not { } kbv) {
                return;
            }

            kbv.PointerMoved -= Kbv_PointerMoved;

            this.Classes.Remove("pressed");
            e.Pointer.Capture(null);

            if (this.DataContext is not KeyViewModel kvm ||
                kvm.Parent is not KeyboardViewModel kbvm ||
                kbvm.HoldingFocusKeyViewModel is not { } hfkvm ||
                kvm.IsHoldKey) {
                TapCommand?.Execute(TapCommandParameter);
                return;
            }
            kbvm.KeyTapCommand.Execute(hfkvm);
        }
    }

    public class iosExtAvaloniaViewLoader {
        private static iosExtAvaloniaViewLoader _instance;
        public static iosExtAvaloniaViewLoader Instance => _instance ?? (_instance = new iosExtAvaloniaViewLoader());

        public static object AvViewObj { get; set; }
    }
}