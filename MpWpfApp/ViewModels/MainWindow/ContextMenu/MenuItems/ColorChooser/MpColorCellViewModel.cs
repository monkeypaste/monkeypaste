using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpColorCellViewModel : MpViewModelBase<MpColorChooserMenuItemViewModel>  {

        #region Properties
        public bool IsCustomColor { get; set; } = false;

        public bool IsSelected { get; set; }

        public Brush CellBrush { get; set; }
        #endregion

        #region Public Methods
        public MpColorCellViewModel() : base(null) { }

        public MpColorCellViewModel(MpColorChooserMenuItemViewModel parent, Brush brush, bool isCustomColor = false) : base(parent) {
            CellBrush = brush;
            IsCustomColor = isCustomColor;
        }
        #endregion

        #region Commands

        public ICommand SelectColorCommand => new RelayCommand(
            () => {
                IsSelected = true;
            });
        #endregion
    }
}
