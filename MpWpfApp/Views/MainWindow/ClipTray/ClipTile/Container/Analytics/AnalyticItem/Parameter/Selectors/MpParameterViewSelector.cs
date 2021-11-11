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

        private DataTemplate _executeTemplate;
        public DataTemplate ExecuteTemplate {
            get { return _executeTemplate; }
            set { _executeTemplate = value; }
        }

        private DataTemplate _resultTemplate;
        public DataTemplate ResultTemplate {
            get { return _resultTemplate; }
            set { _resultTemplate = value; }
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var aipvm = (item as MpAnalyticItemParameterViewModel);
            
            if(aipvm == null) {
                return ExecuteTemplate;
            }

            switch(aipvm.Parameter.ParameterType) {
                case MpAnalyticParameterType.ComboBox:
                    return ComboBoxTemplate;
                case MpAnalyticParameterType.Text:
                    if(aipvm.Parameter.IsResult) {
                        return ResultTemplate;
                    }
                    return TextBoxTemplate;
                case MpAnalyticParameterType.CheckBox:
                    return CheckBoxTemplate;
                case MpAnalyticParameterType.Slider:
                    return SliderTemplate;
                case MpAnalyticParameterType.Button:
                    if(aipvm.Parameter.IsExecute) {
                        return ExecuteTemplate;
                    }
                    throw new Exception("Unsupportted parameter type");
                default:
                    throw new Exception("Unsupportted parameter type");
            }

            throw new Exception("Uknown Item Type");
        }
    }
}
