using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvConditionalExtension : MarkupExtension {
        public MpAvConditionalExtension() { }

        public string Path { get; set; }

        public Type Type { get; set; }

        public object True { get; set; }

        public object False { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var cultureInfo = new CultureInfo(MpPrefViewModel.Instance.DefaultCultureInfoName);

            var binding = new ReflectionBindingExtension(Path) {
                Mode = BindingMode.OneWay,
                Converter = new FuncValueConverter<bool, object>(
                    e => e ?
                    Convert.ChangeType(True, Type, cultureInfo.NumberFormat) :
                    Convert.ChangeType(False, Type, cultureInfo.NumberFormat))
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
