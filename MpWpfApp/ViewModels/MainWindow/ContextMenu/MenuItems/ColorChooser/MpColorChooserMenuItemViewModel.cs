using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
	public class MpColorChooserMenuItemViewModel : MpViewModelBase<MpContextMenuViewModel> {

		#region Properties

		public ObservableCollection<ObservableCollection<Brush>> ColorColumns2 { get; set; } = new ObservableCollection<ObservableCollection<Brush>>();

		public ObservableCollection<ObservableCollection<MpColorCellViewModel>> ColorColumns { get; set; } = new ObservableCollection<ObservableCollection<MpColorCellViewModel>>();

		public MpColorCellViewModel SelectedItem {
            get {
				foreach (var cc in ColorColumns) {
					foreach (var ccvm in cc) {
						if (ccvm.IsSelected) {
							return ccvm;
						}
					}
				}
				return null;
			}
		}

		public Brush SelectedItem2 { get; set; }

		#endregion

		#region Public Methods
		public MpColorChooserMenuItemViewModel() : base(null) { }

		public MpColorChooserMenuItemViewModel(MpContextMenuViewModel parent) : base(parent) {

			var colors = MpThemeColors.Instance.ContentColors;
			for (int i = 0; i < colors.Count; i++) {
				ColorColumns.Add(new ObservableCollection<MpColorCellViewModel>());
				ColorColumns2.Add(new ObservableCollection<Brush>());
				for (int j = 0; j < colors[i].Count; j++) {
					var ccvm = new MpColorCellViewModel(this, colors[i][j]);
					ColorColumns[i].Add(ccvm);
					ColorColumns2[i].Add(colors[i][j]);
				}
			}
		}
		#endregion
	}
}
