﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace MpWpfApp {
    public class MpAnalyticItemGuidToTitleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var aivm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyzerPluginGuid == value.ToString());
            if(aivm == null) {
                return null;
            }
            return aivm.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}