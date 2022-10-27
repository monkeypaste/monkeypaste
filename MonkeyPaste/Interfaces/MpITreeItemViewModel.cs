using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIHierarchialViewModel<T> : 
        MpIViewModel,
        MpITreeItemViewModel<T>,
        MpIHoverableViewModel,
        MpIFocusableViewModel,
        MpISelectableViewModel
        //MpIEditableViewModel,
        //MpIMenuItemViewModel 
        where T:MpViewModelBase, MpITreeItemViewModel {

        string Label { get; set; }

        double LabelFontSize { get; }
        string LabelForegroundHexColor { get; }
        bool ShowAddButton { get; }
        double ScreenWidth { get; set; }
        double ScreenHeight { get; }
        string BackgroundHexColor { get; }
        string BorderHexColor { get; }
        string IconHexColor { get; }
        string IconTextOrResourceKey { get; }
        string IconLabelHexColor { get; }

        bool IsNew { get; }
        

        ICommand AddChildCommand { get; } 
    }

    public interface MpITreeItemViewModel : MpIViewModel {
        bool IsExpanded { get; set; }

        MpITreeItemViewModel ParentTreeItem { get; }

        IEnumerable<MpITreeItemViewModel> Children { get; }
    }

    public interface MpITreeItemViewModel<T> :MpITreeItemViewModel where T:MpViewModelBase {
        //bool IsExpanded { get; set; }

        new T ParentTreeItem { get; }

        new ObservableCollection<T> Children { get; }
    }

    public static class MpITreeItemViewModelExtensions {
        #region Anon Version


        public static IEnumerable<MpITreeItemViewModel> FindAllChildren(this MpITreeItemViewModel tivm) {
            var activml = new List<MpITreeItemViewModel>();
            foreach (MpITreeItemViewModel c in tivm.Children) {
                activml.Add(c);
                var ccl = c.FindAllChildren();
                foreach (var cc in ccl) {
                    activml.Add(cc);
                }
            }
            return activml;
        }

        public static MpITreeItemViewModel FindRootParent(this MpITreeItemViewModel tivm)  {
            MpITreeItemViewModel rootParent = tivm.ParentTreeItem as MpITreeItemViewModel;
            while (rootParent.ParentTreeItem != null) {
                rootParent = rootParent.ParentTreeItem as MpITreeItemViewModel;
            }
            return rootParent;
        }

        #endregion

        #region Generic Version

        public static IList<T> ToList<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase, MpITreeItemViewModel {
            var activml = new List<T>() { tivm as T };

            activml.AddRange(tivm.FindAllChildren());
            return activml;
        }

        public static IEnumerable<T> FindAllChildren<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase, MpITreeItemViewModel {
            var activml = new List<T>();
            foreach (MpITreeItemViewModel<T> c in tivm.Children) {
                activml.Add(c as T);
                activml.AddRange(c.FindAllChildren());
            }
            return activml;
        }

        public static T FindRootParent<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase, MpITreeItemViewModel {
            MpITreeItemViewModel<T> rootParent = tivm.ParentTreeItem as MpITreeItemViewModel<T>;
            while (rootParent.ParentTreeItem != null) {
                rootParent = rootParent.ParentTreeItem as MpITreeItemViewModel<T>;
            }
            return rootParent as T;
        }

        public static int FindTreeLevel<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase, MpITreeItemViewModel {
            int level = 0;
            MpITreeItemViewModel<T> rootParent = tivm.ParentTreeItem as MpITreeItemViewModel<T>;
            while (rootParent.ParentTreeItem != null) {
                rootParent = rootParent.ParentTreeItem as MpITreeItemViewModel<T>;
                level++;
            }
            return level;
        }

        #endregion
    }
}
