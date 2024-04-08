using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutView : MpAvUserControl<MpAvIKeyGestureViewModel> {
        #region Privates

        #endregion
        #region Properties

        #region EmptyText Property

        private string _EmptyText = UiStrings.CommonNoneLabel;

        public static readonly DirectProperty<MpAvShortcutView, string> EmptyTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutView, string>
            (
                nameof(EmptyText),
                o => o.EmptyText,
                (o, v) => o.EmptyText = v
            );

        public string EmptyText {
            get => _EmptyText;
            set {
                SetAndRaise(EmptyTextProperty, ref _EmptyText, value);
            }
        }

        #endregion

        #region RecordCommandParameter Property

        private object _RecordCommandParameter = null;

        public static readonly DirectProperty<MpAvShortcutView, object> RecordCommandParameterProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutView, object>
            (
                nameof(RecordCommandParameter),
                o => o.RecordCommandParameter,
                (o, v) => o.RecordCommandParameter = v
            );

        public object RecordCommandParameter {
            get => _RecordCommandParameter;
            set {
                SetAndRaise(RecordCommandParameterProperty, ref _RecordCommandParameter, value);
            }
        }

        #endregion 

        #region RecordCommand Property

        private ICommand _RecordCommand = new MpCommand(() => { }, () => false);

        public static readonly DirectProperty<MpAvShortcutView, ICommand> RecordCommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutView, ICommand>
            (
                nameof(RecordCommand),
                o => o.RecordCommand,
                (o, v) => o.RecordCommand = v
            );

        public ICommand RecordCommand {
            get => _RecordCommand;
            set {
                SetAndRaise(RecordCommandProperty, ref _RecordCommand, value);
            }
        }

        #endregion

        Control rb =>
            this.FindControl<Control>("RecordButton");
        Control sclb =>
            this.FindControl<Control>("ShortcutLabel");
        public bool CanRecord =>
            RecordCommand != null &&
            RecordCommand.CanExecute(RecordCommandParameter);
        #endregion

        public MpAvShortcutView() {
            InitializeComponent();
            ContainerGrid.Classes.CollectionChanged += Classes_CollectionChanged;
            this.GetObservable(RecordCommandProperty).Subscribe(value => OnRecordChanged()).AddDisposable(this);
            this.GetObservable(RecordCommandParameterProperty).Subscribe(value => OnRecordChanged()).AddDisposable(this);
        }

        private void OnRecordChanged() {
            if (this.Content is not Control c) {
                return;
            }
            if (RecordCommand != null && RecordCommand.CanExecute(RecordCommandParameter)) {
                c.Classes.Add("recordable");
            } else {
                c.Classes.Remove("recordable");
            }
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            RefreshState();
        }
        protected override void OnUnloaded(RoutedEventArgs e) {
            base.OnUnloaded(e);
            ContainerGrid.Classes.CollectionChanged -= Classes_CollectionChanged;
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);
            RefreshState();
        }
        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);
            RefreshState();
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);
            RefreshState();
        }
        private void Classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            RefreshState();
        }
        private void RefreshState() {
            bool hovering = this.GetSelfAndVisualDescendants().OfType<Control>().Any(x => x.IsPointerOver);
            bool recordable = ContainerGrid.Classes.Contains("recordable");
            bool has_shortcut = ContainerGrid.Classes.Contains("hasShortcut");
            bool is_param_part = this.GetVisualAncestor<MpAvParameterCollectionView>() != null;

            if(recordable) {
                if(hovering) {
                    EmptyShortcutTextBlock.IsVisible = false;
                    ShortcutLabel.IsVisible = false;
                    RecordButton.IsVisible = true;
                    return;
                }
                if(has_shortcut) {
                    EmptyShortcutTextBlock.IsVisible = false;
                    ShortcutLabel.IsVisible = true;
                    RecordButton.IsVisible = false;
                    return;
                }
                if(is_param_part) {
                    EmptyShortcutTextBlock.IsVisible = false;
                    ShortcutLabel.IsVisible = false;
                    RecordButton.IsVisible = true;
                    return;
                }
                EmptyShortcutTextBlock.IsVisible = true;
                ShortcutLabel.IsVisible = false;
                RecordButton.IsVisible = false;
                return;
            }
            if (has_shortcut) {
                EmptyShortcutTextBlock.IsVisible = false;
                ShortcutLabel.IsVisible = true;
                RecordButton.IsVisible = false;
                return;
            }
            EmptyShortcutTextBlock.IsVisible = true;
            ShortcutLabel.IsVisible = false;
            RecordButton.IsVisible = false;
            return;
        }
    }
}
