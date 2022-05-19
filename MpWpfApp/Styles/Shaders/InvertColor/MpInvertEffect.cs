using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MpWpfApp {
    // from https://stackoverflow.com/a/45096471/105028
    class MpInvertEffect : ShaderEffect {
        private static readonly PixelShader _shader =
            new PixelShader { 
                UriSource = new Uri("pack://application:,,,/Styles/Shaders/InvertColor/MpInvertColorShader.ps") };

        public MpInvertEffect() {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }

        public Brush Input {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty(
                nameof(Input), 
                typeof(MpInvertEffect), 0);

        #region IsBright

        public static bool GetIsBright(DependencyObject obj) {
            return (bool)obj.GetValue(IsBrightProperty);
        }
        public static void SetIsBright(DependencyObject obj, bool value) {
            obj.SetValue(IsBrightProperty, value);
        }
        public static readonly DependencyProperty IsBrightProperty =
          DependencyProperty.RegisterAttached(
            "IsBright",
            typeof(bool),
            typeof(MpInvertEffect),
            new FrameworkPropertyMetadata() {
                DefaultValue = false,
                PropertyChangedCallback = (s,e) => {
                    if(e.NewValue == null) {
                        return;
                    }
                    bool isBright = (bool)e.NewValue;

                }
            });

        #endregion

    }
}
