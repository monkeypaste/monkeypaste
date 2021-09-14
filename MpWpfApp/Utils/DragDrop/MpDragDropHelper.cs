using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpDragDropHelper {
        #region Singleton Definition
        private static readonly Lazy<MpDragDropHelper> _Lazy = new Lazy<MpDragDropHelper>(() => new MpDragDropHelper());
        public static MpDragDropHelper Instance { get { return _Lazy.Value; } }

        public void Init() { }

        private MpDragDropHelper() { }
        #endregion

        #region Private Variables

        private Point _mouseDownPosition;

        private double _minDragDist = 10;
        #endregion

        #region Properties

        public IDataObject DragDataObject { get; set; }

        public ObservableCollection<MpCopyItem> DragModels { get; private set; } = new ObservableCollection<MpCopyItem>();

        public bool IsDragging {
            get {
                return DragModels.Count > 0;
            }
        }
        #endregion

        #region Public Methods


        public void RegisterDragItem(FrameworkElement fe) {
            
        }

        public void Reset() {
            _mouseDownPosition = new Point();
            DragDataObject = null;
            DragModels.Clear();
        }
        #endregion
    }
}
