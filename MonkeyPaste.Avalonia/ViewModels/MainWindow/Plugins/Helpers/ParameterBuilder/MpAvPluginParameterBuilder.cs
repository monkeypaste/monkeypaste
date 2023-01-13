using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginParameterBuilder {
        public static async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync(
            MpPluginPresetParameterValue aipv, 
            MpIParameterHostViewModel pluginHost) {
            var param = pluginHost.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == aipv.ParamId);
            if(param == null) {
                return null;
            }
            MpParameterControlType controlType = param.controlType;

            MpAvParameterViewModelBase naipvm = null;

            switch (controlType) {
                case MpParameterControlType.List:
                case MpParameterControlType.MultiSelectList:
                case MpParameterControlType.EditableList:
                case MpParameterControlType.ComboBox:
                    naipvm = new MpAvEnumerableParameterViewModel(pluginHost);
                    break;
                case MpParameterControlType.PasswordBox:
                case MpParameterControlType.TextBox:
                    naipvm = new MpAvTextBoxParameterViewModel(pluginHost);
                    break;
                case MpParameterControlType.CheckBox:
                    naipvm = new MpAvCheckBoxParameterViewModel(pluginHost);
                    break;
                case MpParameterControlType.Slider:
                    naipvm = new MpAvSliderParameterViewModel(pluginHost);
                    break;
                case MpParameterControlType.DirectoryChooser:
                case MpParameterControlType.FileChooser:
                    naipvm = new MpAvFileChooserParameterViewModel(pluginHost);
                    break;
                case MpParameterControlType.ComponentPicker:
                    naipvm = new MpAvComponentPickerParameterViewModel(pluginHost);
                    break;
                case MpParameterControlType.ShortcutRecorder:
                    naipvm = new MpAvShortcutRecorderParameterViewModel(pluginHost);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpParameterControlType), controlType));
            }

            await naipvm.InitializeAsync(aipv);

            return naipvm;
        }
    }
}
