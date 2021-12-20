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
                if(item is MpAnalyticItemResultViewModel) {
                    return ResultTemplate;
                }
                if(item is MpAnalyticItemExecuteButtonViewModel) {
                    return ExecuteTemplate;
                }
                throw new Exception("Unknown item type: " + item.GetType().ToString());
            }

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
