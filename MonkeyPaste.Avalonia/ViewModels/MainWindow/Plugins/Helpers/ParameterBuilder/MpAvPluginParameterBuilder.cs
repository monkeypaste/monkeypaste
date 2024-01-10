using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginParameterBuilder {
        public static async Task CleanupMissingParamsAsync(IEnumerable<MpAvIParameterCollectionViewModel> phcvml) {
            // let locator create missing params then delete and remove after loaded

            var presets_w_missing_params =
                phcvml
                .Where(x => x.Items.Any(x => x is MpAvMissingParameterViewModel));
            foreach (var pvm in presets_w_missing_params) {
                var new_items = pvm.Items.ToList();
                var missing_params = pvm.Items.OfType<MpAvMissingParameterViewModel>().ToList();
                for (int i = 0; i < missing_params.Count; i++) {
                    // delete missing parameter
                    await missing_params[i].ParameterValue.DeleteFromDatabaseAsync();
                    new_items.Remove(missing_params[i]);
                }
                if (pvm.Items is ObservableCollection<MpAvParameterViewModelBase> pvmil) {
                    // reset preset parameters to set w/o missing params
                    pvmil.Clear();
                    new_items.ForEach(x => pvmil.Add(x));
                }
            }
        }

        public static async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync(
            MpParameterValue aipv,
            MpIParameterHostViewModel host) {
            MpParameterControlType controlType = MpParameterControlType.None;
            var param = host.ComponentFormat.parameters.FirstOrDefault(x => x.paramId == aipv.ParamId);
            if (param == null) {
                // must be a bug still with reset preset, check 
                //MpDebug.Break($"Reset preset error for host {host}");
                return new MpAvMissingParameterViewModel(host as MpAvViewModelBase);
            }
            controlType = param.controlType;
            var result = await CreateParameterViewModelAsync_internal(controlType, aipv, host as MpAvViewModelBase);
            return result;
        }

        private static async Task<MpAvParameterViewModelBase> CreateParameterViewModelAsync_internal(
            MpParameterControlType controlType,
            MpParameterValue aipv,
            MpAvViewModelBase parent) {
            MpAvParameterViewModelBase naipvm;


            switch (controlType) {
                case MpParameterControlType.MultiSelectList:
                    naipvm = new MpAvMultiEnumerableParameterViewModel(parent);
                    break;
                case MpParameterControlType.EditableList:
                    naipvm = new MpAvEditableEnumerableParameterViewModel(parent);
                    break;
                case MpParameterControlType.List:
                case MpParameterControlType.ComboBox:
                    naipvm = new MpAvSingleEnumerableParameterViewModel(parent);
                    break;
                case MpParameterControlType.PasswordBox:
                case MpParameterControlType.TextBox:
                    naipvm = new MpAvTextBoxParameterViewModel(parent);
                    break;
                case MpParameterControlType.DateTimePicker:
                    naipvm = new MpAvDateTimeParameterViewModel(parent);
                    break;
                case MpParameterControlType.CheckBox:
                    naipvm = new MpAvCheckBoxParameterViewModel(parent);
                    break;
                case MpParameterControlType.Button:
                case MpParameterControlType.Hyperlink:
                    naipvm = new MpAvButtonParameterViewModel(parent);
                    break;
                case MpParameterControlType.Slider:
                    naipvm = new MpAvSliderParameterViewModel(parent);
                    break;
                case MpParameterControlType.DirectoryChooser:
                case MpParameterControlType.FileChooser:
                    naipvm = new MpAvFileChooserParameterViewModel(parent);
                    break;
                case MpParameterControlType.ComponentPicker:
                    naipvm = new MpAvComponentPickerParameterViewModel(parent);
                    break;
                case MpParameterControlType.ShortcutRecorder:
                    naipvm = new MpAvShortcutRecorderParameterViewModel(parent);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpParameterControlType), controlType));
            }

            await naipvm.InitializeAsync(aipv);

            return naipvm;
        }
    }
}
