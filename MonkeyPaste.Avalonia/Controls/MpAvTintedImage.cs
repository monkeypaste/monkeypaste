using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvTintedImage : UserControl {
        #region Properties

        #region Tint

        public static readonly AttachedProperty<IBrush> TintProperty =
            AvaloniaProperty.RegisterAttached<MpAvTintedImage, Control, IBrush>(
                nameof(Tint),
                defaultValue: Brushes.Black);

        public IBrush Tint {
            get => GetValue(TintProperty);
            set => SetValue(TintProperty, value);
        }

        #endregion
        
        #region Source

        public static readonly AttachedProperty<IImage> SourceProperty =
            AvaloniaProperty.RegisterAttached<MpAvTintedImage, Control, IImage>(
                nameof(Source));

        public IImage Source {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        #endregion

        #endregion
        public MpAvTintedImage() {
            this.GetObservable(SourceProperty).Subscribe(value => Init()).AddDisposable(this);
            this.GetObservable(TintProperty).Subscribe(value => Init()).AddDisposable(this);
        }

        private void Init() {
            if(this.OpacityMask is not ImageBrush ib) {
                ib = new ImageBrush() {
                    TileMode = TileMode.None
                };
                this.OpacityMask = ib;
            }
            ib.Source = Source as IImageBrushSource;
            this.Background = Tint;
            this.InvalidateVisual();
        }
    }
}
