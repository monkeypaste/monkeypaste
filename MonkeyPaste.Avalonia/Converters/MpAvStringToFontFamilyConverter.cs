﻿using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringToFontFamilyConverter : IValueConverter {
        public static readonly MpAvStringToFontFamilyConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is DynamicResourceExtension dre) {
                value = Mp.Services.PlatformResource.GetResource(dre.ResourceKey.ToString()) as string;
            }
            if (value is string valueStr && !string.IsNullOrEmpty(valueStr)) {
                if (valueStr != "Segoe UI") {

                }
                try {
                    return new FontFamily(valueStr);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting str '{valueStr}' to font family", ex);
                }
            }
            return FontFamily.Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt.ToString();
            }
            if (value is DateTimeOffset dto) {
                return dto.DateTime.ToString();
            }
            return null;
        }
    }
}