using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSliderValueViewSelector : DataTemplateSelector {

    }
    public class MpParameterViewSelector : DataTemplateSelector {
        private DataTemplate _comboBoxTemplate;
        public DataTemplate ComboBoxTemplate {
            get { return _comboBoxTemplate; }
            set { _comboBoxTemplate = value; }
        }

        private DataTemplate _textBoxTemplate;
        public DataTemplate TextBoxTemplate {
            get { return _textBoxTemplate; }
            set { _textBoxTemplate = value; }
        }

        private DataTemplate _checkBoxTemplate;
        public DataTemplate CheckBoxTemplate {
            get { return _checkBoxTemplate; }
            set { _checkBoxTemplate = value; }
        }

        private DataTemplate _sliderTemplate;
        public DataTemplate SliderTemplate {
            get { return _sliderTemplate; }
            set { _sliderTemplate = value; }
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var aipvm = (item as MpAnalyticItemParameterViewModel);
            
            switch(aipvm.Parameter.ParameterType) {
                case MpAnalyticItemParameterType.ComboBox:
                    return ComboBoxTemplate;
                case MpAnalyticItemParameterType.Text:
                    return TextBoxTemplate;
                case MpAnalyticItemParameterType.CheckBox:
                    return CheckBoxTemplate;
                case MpAnalyticItemParameterType.Slider:
                    return SliderTemplate;
                default:
                    throw new Exception("Unsupportted parameter type");
            }

            throw new Exception("Uknown Item Type");
        }
    }
}
