using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Data;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutLabel : MpAvUserControl<object> {

        private CompositeDisposable _disposables = new CompositeDisposable();

        #region KeyGesture Property

        private object _KeyGesture = null;

        public static readonly DirectProperty<MpAvShortcutLabel, object> KeyGestureProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutLabel, object>
            (
                nameof(KeyGesture),
                o => o.KeyGesture,
                (o, v) => o.KeyGesture = v
            );

        public object KeyGesture {
            get => _KeyGesture;
            set {
                SetAndRaise(KeyGestureProperty, ref _KeyGesture, value);
            }
        }

        #endregion
        public MpAvShortcutLabel() {
            InitializeComponent();
            this.GetObservable(MpAvShortcutLabel.KeyGestureProperty)
                .Subscribe(value => {
                    Init();
                })
                .DisposeWith(_disposables);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            Init();

        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            _disposables.Dispose();
        }

        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);

        }
        private void Init() {
            var sclb = this.FindControl<ItemsControl>("ShortcutListBox");
            if (KeyGesture is ObservableCollection<MpAvShortcutKeyGroupViewModel> keyGroups) {
                //sclb.Bind(
                //    ItemsControl.ItemsSourceProperty,
                //    new Binding() {
                //        Source = DataContext,
                //        Path = 
                //    })
                sclb.ItemsSource = keyGroups;
            } else if (KeyGesture is KeyGesture kg) {
                sclb.ItemsSource = kg.ToKeyLiteral().ToKeyItems();
            }

            //if (sclb.ItemCount > 0) {
            //    this.MinWidth = 50;
            //    this.MinHeight = 20;
            //} else {
            //    this.MinWidth = 0;
            //    this.MinHeight = 0;
            //}
        }
    }
}
