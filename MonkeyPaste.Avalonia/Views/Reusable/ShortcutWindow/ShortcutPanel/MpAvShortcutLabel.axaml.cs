using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

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

        private void Init() {
            var sclb = this.FindControl<ItemsControl>("ShortcutListBox");
            string keys = null;
            if (KeyGesture is ObservableCollection<MpAvShortcutKeyGroupViewModel> keyGroups) {
                sclb.ItemsSource = keyGroups;
            } else if (KeyGesture is KeyGesture kg) {
                keys = kg.ToKeyLiteral();
                sclb.ItemsSource = keys.ToKeyItems();
            } else if (KeyGesture is string keystr) {
                keys = keystr;
                sclb.ItemsSource = keystr.ToKeyItems();
            } else if (KeyGesture is IEnumerable<string> key_arr) {
                // used for mod keys in assign warning
                sclb.ItemsSource = key_arr.ToKeyItems(out keys);
            }
            if (!string.IsNullOrEmpty(keys) &&
                MpAvKeyStringToIsGlobalBoolConverterConverter.Instance.Convert(keys, null, null, null) is bool isGlobal &&
                isGlobal) {
                this.Classes.Add("global");
            } else {
                this.Classes.Remove("global");
            }
        }
    }
}
