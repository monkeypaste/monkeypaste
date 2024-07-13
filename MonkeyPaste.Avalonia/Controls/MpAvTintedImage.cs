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
        
        #region Stretch

        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<MpAvTintedImage, Stretch>(
                nameof(Stretch),
                Stretch.Uniform);

        public Stretch Stretch {
            get => GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        #endregion
        
        #region StretchDirection

        public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
            AvaloniaProperty.Register<MpAvTintedImage, StretchDirection>(
                nameof(StretchDirection),
                StretchDirection.Both);

        public StretchDirection StretchDirection {
            get => GetValue(StretchDirectionProperty);
            set => SetValue(StretchDirectionProperty, value);
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
            AffectsRender<MpAvTintedImage>(SourceProperty, TintProperty);
            AffectsMeasure<MpAvTintedImage>(SourceProperty, TintProperty);
            SourceProperty.Changed.AddClassHandler<MpAvTintedImage>((x, y) => Init(x));
            TintProperty.Changed.AddClassHandler<MpAvTintedImage>((x, y) => Init(x));
        }

        private static void Init(MpAvTintedImage ti) {
            var ib = GetImageBrush(ti.Source as IImageBrushSource);
            if (ti.Tint == null) {
                // treat Tint == null as special case to not tint image
                ti.OpacityMask = null;
                ti.Background = ib;
            } else {
                ti.OpacityMask = ib;
                ti.Background = ti.Tint;
            }
            ti.InvalidateVisual();
            ti.InvalidateMeasure();
        }

        private static ImageBrush GetImageBrush(IImageBrushSource source) {
            return new ImageBrush() {
                TileMode = TileMode.None,
                Source = source
            };
        }
        public MpAvTintedImage() { }
    }
}
