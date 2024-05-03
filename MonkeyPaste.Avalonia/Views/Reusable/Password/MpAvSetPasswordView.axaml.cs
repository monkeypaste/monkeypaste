using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Reactive.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvSetPasswordView : MpAvUserControl<object> {
        #region Properties

        #region ConfirmedPassword AvaloniaProperty

        public string ConfirmedPassword {
            get { return GetValue(ConfirmedPasswordProperty); }
            set { SetValue(ConfirmedPasswordProperty, value); }
        }

        public static readonly StyledProperty<string> ConfirmedPasswordProperty =
            AvaloniaProperty.Register<MpAvSetPasswordView, string>(
                name: nameof(ConfirmedPassword),
                defaultValue: string.Empty,
                defaultBindingMode: BindingMode.TwoWay);

        #endregion

        #region RememberTooltip AvaloniaProperty

        public string RememberTooltip {
            get { return GetValue(RememberTooltipProperty); }
            set { SetValue(RememberTooltipProperty, value); }
        }

        public static readonly StyledProperty<string> RememberTooltipProperty =
            AvaloniaProperty.Register<MpAvSetPasswordView, string>(
                name: nameof(RememberTooltip),
                defaultValue: string.Empty);

        #endregion

        #region AutoFilledPassword AvaloniaProperty

        public string AutoFilledPassword {
            get { return GetValue(AutoFilledPasswordProperty); }
            set { SetValue(AutoFilledPasswordProperty, value); }
        }

        public static readonly StyledProperty<string> AutoFilledPasswordProperty =
            AvaloniaProperty.Register<MpAvSetPasswordView, string>(
                name: nameof(AutoFilledPassword),
                defaultValue: string.Empty);

        #endregion

        #region IsPasswordValid AvaloniaProperty

        public bool IsPasswordValid {
            get { return GetValue(IsPasswordValidProperty); }
            set { SetValue(IsPasswordValidProperty, value); }
        }

        public static readonly StyledProperty<bool> IsPasswordValidProperty =
            AvaloniaProperty.Register<MpAvSetPasswordView, bool>(
                name: nameof(IsPasswordValid),
                defaultValue: true,
                defaultBindingMode: BindingMode.TwoWay);

        #endregion

        #region ShowDialogButtons AvaloniaProperty

        public bool ShowDialogButtons {
            get { return GetValue(ShowDialogButtonsProperty); }
            set { SetValue(ShowDialogButtonsProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowDialogButtonsProperty =
            AvaloniaProperty.Register<MpAvSetPasswordView, bool>(
                name: nameof(ShowDialogButtons),
                defaultValue: false);

        #endregion

        #region RememberPassword AvaloniaProperty

        public bool RememberPassword {
            get { return GetValue(RememberPasswordProperty); }
            set { SetValue(RememberPasswordProperty, value); }
        }

        public static readonly StyledProperty<bool> RememberPasswordProperty =
            AvaloniaProperty.Register<MpAvSetPasswordView, bool>(
                name: nameof(RememberPassword),
                defaultValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        #endregion

        #endregion

        public MpAvSetPasswordView() : base() {
            InitializeComponent();
            var pwtb1 = this.FindControl<TextBox>("PasswordBox1");
            var pwtb2 = this.FindControl<TextBox>("PasswordBox2");
            pwtb1.TextChanged += Password_TextChanged;
            pwtb2.TextChanged += Password_TextChanged;

            var cancelButton = this.FindControl<Button>("CancelButton");
            cancelButton.Click += CancelButton_Click;
            var doneButton = this.FindControl<Button>("DoneButton");
            doneButton.Click += DoneButton_Click;

            this.GetObservable(AutoFilledPasswordProperty)
                .Subscribe(value => OnAutoFillValueChanged());
            //this.DataContextChanged += MpAvSetPasswordView_DataContextChanged;
            //if (DataContext != null) {
            //    MpAvSetPasswordView_DataContextChanged(this, null);
            //}
        }

        private void OnAutoFillValueChanged() {
            if (string.IsNullOrEmpty(AutoFilledPassword)) {
                return;
            }

            var pwtb1 = this.FindControl<TextBox>("PasswordBox1");
            var pwtb2 = this.FindControl<TextBox>("PasswordBox2");
            pwtb1.Text = AutoFilledPassword;
            pwtb2.Text = AutoFilledPassword;
        }

        private void Password_TextChanged(object sender, TextChangedEventArgs e) {
            IsPasswordValid = Validate();
            ConfirmedPassword = IsPasswordValid ?
                this.FindControl<TextBox>("PasswordBox1").Text : null;
        }

        private bool Validate() {
            var pwtb1 = this.FindControl<TextBox>("PasswordBox1");
            var pwtb2 = this.FindControl<TextBox>("PasswordBox2");
            return pwtb1.Text.ToStringOrEmpty() == pwtb2.Text.ToStringOrEmpty();
        }
        private void CancelButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (MpAvWindowManager.GetTopLevel(this) is not MpAvWindow w) {
                return;
            }
            w.DialogResult = null;
            w.Close();
        }

        private void DoneButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (MpAvWindowManager.GetTopLevel(this) is not MpAvWindow w) {
                return;
            }

            w.DialogResult = this.FindControl<TextBox>("PasswordBox1").Text;
            if (w.DialogResult == null) {
                // ensure null is only returned on cancel
                w.DialogResult = string.Empty;
            }
            w.Close();
        }

    }
}
