﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpSliderValueViewSelector : DataTemplateSelector {

    }
    public class MpParameterViewSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var aipvm = item as MpAnalyticItemParameterViewModel;
            if(aipvm == null) {
                return null;
            }
            string keyStr = aipvm.ControlType.ToString() + "Parameter";

            var g = (container as FrameworkElement).GetVisualAncestor<Grid>();
            if(g == null || !g.Resources.Contains(keyStr)) {
                return null;
            }
            var result = g.Resources[keyStr] as DataTemplate;
            return result;
        }
    }
}