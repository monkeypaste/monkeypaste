using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpComboBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemParameterValueViewModel> Values { get; set; } = new ObservableCollection<MpAnalyticItemParameterValueViewModel>();

        #endregion

        #region State

        #endregion

        #region Model

        public override bool IsValid {
            get {
                if(!IsRequired) {
                    return true;
                }
                return SelectedValue != null && !string.IsNullOrEmpty(SelectedValue.Value);
            }
        }
        
        #endregion

        #endregion

        #region Constructors

        public MpComboBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;

            await Task.Delay(3);

            Parameter = aip;

            Values.Clear();
            var valueParts = Parameter.ValueCsv.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < valueParts.Length; i++) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(i, valueParts[i]);
                Values.Add(naipvvm);
            }

            if (!string.IsNullOrEmpty(Parameter.DefaultValue)) {
                var defValVm = Values.Where(x => x.Value == Parameter.DefaultValue).FirstOrDefault();
                if (defValVm != null) {
                    SelectedValue = defValVm;
                }
            }

            OnPropertyChanged(nameof(Values));

            IsBusy = false;
        }

        #endregion
    }
}
