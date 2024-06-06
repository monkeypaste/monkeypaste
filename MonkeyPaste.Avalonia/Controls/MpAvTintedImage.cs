using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using MonkeyPaste.Common.Avalonia;
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

        public static readonly StyledProperty<IBrush> TintProperty =
            AvaloniaProperty.Register<MpAvTintedImage,IBrush>(
                nameof(Tint),
                defaultValue: Brushes.Black);

        public IBrush Tint {
            get => GetValue(TintProperty);
            set => SetValue(TintProperty, value);
        }

        #endregion
        
        #region Source

        public static readonly StyledProperty<IImage> SourceProperty =
            AvaloniaProperty.Register<MpAvTintedImage, IImage>(
                nameof(Source));

        public IImage Source {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        #endregion

        #endregion
        static MpAvTintedImage() {
            AffectsRender<Image>(SourceProperty, TintProperty);
            SourceProperty.Changed.AddClassHandler<MpAvTintedImage>((x, y) => Init(x));
            TintProperty.Changed.AddClassHandler<MpAvTintedImage>((x, y) => Init(x));
        }

        private static void Init(MpAvTintedImage ti) {
            if (ti.OpacityMask is not ImageBrush ib) {
                ib = new ImageBrush() {
                    TileMode = TileMode.None
                };
                ti.OpacityMask = ib;
            }
            ib.Source = ti.Source as IImageBrushSource;
            ti.Background = ti.Tint;
            ti.InvalidateVisual();
        }
        public MpAvTintedImage() { }
    }
}
