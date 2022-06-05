using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;


namespace MonkeyPaste.Common.Wpf {
    public static class MpWpfExtensions {
        #region Input

        public static string ToString(this Key key) {
            return MpWpfKeyboardInputHelpers.ConvertKeyToString(key);
        }

        public static void KillFocus(this Control control) {
            // Kill logical focus
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(control), null);
            // Kill keyboard focus
            Keyboard.ClearFocus(); 
        }

        #endregion

        #region System

        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject dependencyObject) {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++) {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }

        public static IEnumerable<DependencyObject> EnumerateVisualDescendents(this DependencyObject dependencyObject) {
            yield return dependencyObject;

            foreach (DependencyObject child in dependencyObject.EnumerateVisualChildren()) {
                foreach (DependencyObject descendent in child.EnumerateVisualDescendents()) {
                    yield return descendent;
                }
            }
        }

        public static void ClearBindings(this DependencyObject dependencyObject) {
            foreach (DependencyObject element in dependencyObject.EnumerateVisualDescendents()) {
                BindingOperations.ClearAllBindings(element);
            }
        }

        #endregion

        #region Graphics

        public static Point ToWpfPoint(this MpPoint p) {
            return new Point() { X = p.X, Y = p.Y };
        }

        public static Size ToWpfSize(this MpSize s) {
            return new Size() { Width = s.Width, Height = s.Height };
        }

        public static Rect ToWpfRect(this MpRect rect) {
            return new Rect(rect.Location.ToWpfPoint(), rect.Size.ToWpfSize());
        }
        #endregion

        #region Collections

        public static void Refresh(this CollectionView cv,[CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            cv.Refresh();
            MpConsole.WriteTraceLine("Collection refreshed",null,callerName,callerFilePath,lineNum);
        }

        public static bool IsEmpty<T>(this IList<T> source) {
            return source.Count == 0;
        }       
        
        //public static void Move<T>(this IList<T> collection, int oldIdx, int newIdx) where T : class {
        //    var item = collection[oldIdx];
        //    collection.RemoveAt(oldIdx);
        //    collection.Insert(newIdx, item);
        //}

        public static ObservableCollection<TSource> Sort<TSource, TKey>(
            this ObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector,
            bool desc = false) where TSource : class {
            if (source == null) {
                return null;
            }
            Comparer<TKey> comparer = Comparer<TKey>.Default;

            for (int i = source.Count - 1; i >= 0; i--) {
                for (int j = 1; j <= i; j++) {
                    TSource o1 = source[j - 1];
                    TSource o2 = source[j];
                    int comparison = comparer.Compare(keySelector(o1), keySelector(o2));
                    if (desc && comparison < 0) {
                        source.Move(j, j - 1);
                    } else if (!desc && comparison > 0) {
                        source.Move(j - 1, j);
                    }
                }
            }
            return source;
        }

        #endregion

        #region TreeView/TreeViewItem

        //public static ScrollViewer GetScrollViewer(this TreeView tv) {
        //    ScrollViewer sv = tv.GetVisualDescendent<ScrollViewer>();
        //    return sv;
        //}

        public static TreeViewItem GetTreeViewItem(this TreeView tv, int index) {
            if (tv == null) {
                return null;
            }
            if (index < 0 || index >= tv.Items.Count) {
                return null;
            }
            return tv.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
        }

        public static Rect GetTreeViewItemRect(this TreeView tv, int index) {
            var tvi = tv.GetTreeViewItem(index);
            if (tvi == null || tvi.Visibility != Visibility.Visible) {
                return new Rect();
            }
            var sv = tv.GetVisualDescendent<ScrollViewer>();
            Point origin = tvi.TranslatePoint(new Point(0, 0), sv);
            return new Rect(origin, new Size(tvi.ActualWidth, tvi.ActualHeight));
        }

        public static Rect GetTreeViewRect(this TreeView tv) {
            if (tv == null) {
                return new Rect();
            }
            return new Rect(new Point(0, 0), new Size(tv.ActualWidth, tv.ActualHeight));
        }

        #endregion

        #region Context Menus

        public static bool IsMenuItem(this MenuItem mi, int idx) {
            return mi.ItemContainerGenerator.ContainerFromItem(idx) is MenuItem;
        }

        public static bool IsMenuItem(this ContextMenu cm, int idx) {
            return cm.ItemContainerGenerator.ContainerFromItem(idx) is MenuItem;
        }

        public static bool IsSeparator(this MenuItem mi, int idx) {
            return !mi.IsMenuItem(idx);
        }

        public static bool IsSeparator(this ContextMenu cm, int idx) {
            return !cm.IsMenuItem(idx);
        }

        public static MenuItem GetMenuItem(this MenuItem mi, int idx) {            
            return mi.ItemContainerGenerator.ContainerFromItem(idx) as MenuItem;
        }

        public static MenuItem GetMenuItem(this ContextMenu cm, int idx) {
            return cm.ItemContainerGenerator.ContainerFromItem(idx) as MenuItem;
        }

        public static Separator GetSeparator(this MenuItem mi, int idx) {
            return mi.ItemContainerGenerator.ContainerFromItem(idx) as Separator;
        }

        public static Separator GetSeparator(this ContextMenu cm, int idx) {
            return cm.ItemContainerGenerator.ContainerFromItem(idx) as Separator;
        }

        #endregion

        #region Listbox/ListboxItem

        #region Extended Selection

        public static void UpdateExtendedSelection(this ListBox lb, int index) {
            /*
                1    if the target item is not selected, select it
                2    if Ctrl key is down, add target item to selection 
                3    if Shift key is down
                4    if there is a previously selected item, add all items between target item and most recently selected item to selection, clearing any others
                5    else add item and all previous items
                6    if the target item is selected de-select only if Ctrl key is down         
                7    if neither ctrl nor shift are pressed clear any other selection
                8    if the target item is selected
                9    if Ctrl key is down, remove item from selection
               10    if Shift key is down
               11    if there is a previously selected item, clear selection and then add between target item and first previously selected item
               12    else remove any other item from selection
            */


            //if (lb.DataContext is MpContentItemViewModel civm) {
            //    var ctr_lb = lb.GetVisualAncestor<ListBox>();
            //    ctr_lb.UpdateExtendedSelection(civm.Parent.ItemIdx);
            //}

            bool isMouse_L_Down = Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed;
            bool isMouse_R_Down = Mouse.PrimaryDevice.RightButton == MouseButtonState.Pressed;

            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control); 
            bool isShiftDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift); 

            var lbi = lb.GetListBoxItem(index);
            if (lbi.IsSelected) {
                if (isShiftDown) {
                    //10   if Shift key is down
                    if (lb.SelectedItems.Count > 0) {
                        //11   if there is a previously selected item, remove all items between target item and previous item from selection
                        var firstSelectedItem = lb.GetListBoxItem(lb.Items.IndexOf(lb.SelectedItems[0]));
                        int firstIdx = lb.Items.IndexOf(firstSelectedItem.DataContext);
                        lb.SelectedItems.Clear();
                        if (firstIdx < index) {
                            for (int i = firstIdx; i <= index; i++) {
                                lb.GetListBoxItem(i).IsSelected = true;
                            }
                            return;
                        } else {
                            for (int i = index; i <= firstIdx; i++) {
                                lb.GetListBoxItem(i).IsSelected = true;
                            }
                            return;
                        }
                    }

                } else if (isCtrlDown) {
                    //9    if Ctrl key is down, remove item from selection
                    lbi.IsSelected = false;
                } else {
                    //12   else remove any other item from selection
                    if(!isMouse_L_Down) {
                        lb.SelectedItems.Clear();
                        lbi.IsSelected = true;

                        lb.GetVisualAncestor<ListBox>().SelectedItems.Clear();
                        lbi.GetVisualAncestor<ListBoxItem>().IsSelected = true;
                    }
                    
                }
            } else {
                ListBoxItem lastSelectedItem = null;
                if (lb.SelectedItems.Count > 0) {
                    // NOTE this maybe the wrong item
                    int itemIdx = lb.Items.IndexOf(lb.SelectedItems[lb.SelectedItems.Count - 1]);
                    lastSelectedItem = (ListBoxItem)lb.GetListBoxItem(itemIdx);
                }
                if (isShiftDown) {
                    if (lastSelectedItem == null) {
                        //5 else add item and all previous items
                        for (int i = 0; i <= index; i++) {
                            lb.GetListBoxItem(i).IsSelected = true;
                        }
                        return;
                    } else {
                        //4 if there is a previously selected item, add all items between target
                        //  item and most recently selected item to selection, clearing any others
                        lb.SelectedItems.Clear();

                        int lastIdx = lb.Items.IndexOf(lastSelectedItem.DataContext);
                        if (lastIdx < index) {
                            for (int i = lastIdx; i <= index; i++) {
                                lb.GetListBoxItem(i).IsSelected = true;
                            }
                        } else {
                            for (int i = index; i <= lastIdx; i++) {
                                lb.GetListBoxItem(i).IsSelected = true;
                            }
                        }
                    }
                } else if (isCtrlDown) {
                    //2    if Ctrl key is down, add target item to selection 
                    //6    if the target item is selected de-select only if Ctrl key is down

                    lbi.IsSelected = !lbi.IsSelected;
                } else {
                    //7    if neither ctrl nor shift are pressed clear any other selection
                    if(isMouse_L_Down) {
                        lbi.IsSelected = true;
                    } else {
                        lb.SelectedItems.Clear();
                    }
                }
            } 
        }

        public static void UpdateExtendedSelection(this ListBoxItem lbi) {
            int lbiIdx = lbi.GetListBoxItemIdx();
            if (lbiIdx >= 0) {
                lbi.GetParentListBox().UpdateExtendedSelection(lbiIdx);
            }
        }

        #endregion


        public static ScrollBar GetScrollBar(this ScrollViewer sv, Orientation orientation) {
            if(orientation == Orientation.Vertical) {
                return sv.Template.FindName("PART_VerticalScrollBar", sv) as ScrollBar;
            }
            return sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
        }
        
        public static ListBoxItem GetListBoxItem(this ListBox lb, int index) {
            if (lb == null) {
                return null;
            }
            if (index < 0 || index >= lb.Items.Count) {
                return null;
            }
            return lb.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        public static ListBoxItem GetListBoxItem(this ListBox lb, object dataContext) {
            if (lb == null) {
                return null;
            }
            for (int i = 0; i < lb.Items.Count; i++) {
                var lbi = lb.Items[i];
                if (lbi == dataContext) {
                    return lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                }
            }
            return null;
        }

        public static IEnumerable<ListBoxItem> GetListBoxItems(this ListBox lb) {
            for (int i = 0; i < lb.Items.Count; i++) {
                yield return lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
            }
        }

        public static int GetListBoxItemIdx(this ListBoxItem lbi) {
            var lb = lbi.GetParentListBox();
            return lb.ItemContainerGenerator.IndexFromContainer(lbi);
        }

        public static Rect GetRect(this ListBoxItem lbi, bool relativeToListBox = false) {
            var lbir = lbi.GetParentListBox().GetListBoxItemRect(lbi.GetListBoxItemIdx());
            if(relativeToListBox) {
                return lbir;
            }
            lbir.Location = new Point();
            return lbir;
        }

        public static ListBox GetParentListBox(this ListBoxItem lbi) {
            return lbi.GetVisualAncestor<ListBox>();
        }

        public static Rect GetListBoxItemRect(this ListBox lb, int index, Visual relativeTo = null) {
            var lbi = lb.GetListBoxItem(index);
            if (lbi == null || lbi.Visibility != Visibility.Visible) {
                return new Rect();
            }
            var sv = relativeTo == null ? lb.GetVisualDescendent<ScrollViewer>():relativeTo;
            Point origin = lbi.TranslatePoint(new Point(0, 0), (UIElement)sv);
            //Point origin2 = lbi.TranslatePoint(new Point(0, 0), lb);
            return new Rect(origin, new Size(lbi.ActualWidth, lbi.ActualHeight));
        }

        public static List<Rect> GetListBoxItemRects(this ListBox lb, Visual relativeTo = null) {            
            var rl = new List<Rect>();
            for (int i = 0; i < lb.Items.Count; i++) {
                rl.Add(lb.GetListBoxItemRect(i,relativeTo));
            }
            return rl;
        }

        public static ListBoxItem GetItemAtPoint(this ListBox lb, Point p, Visual relativeTo = null) {
            int idx = lb.GetItemIndexAtPoint(p,relativeTo);
            return idx < 0 ? null : lb.GetListBoxItem(idx);
        }

        public static int GetItemIndexAtPoint(this ListBox lb, Point mp, Visual relativeTo = null) {
            relativeTo = relativeTo == null ? lb : relativeTo;

            Rect lbr = lb.GetListBoxRect();
            //mp.X += sv.HorizontalOffset;
            //mp.Y += sv.VerticalOffset;
            var lbirl = lb.GetListBoxItemRects(relativeTo);
            var lbir = lbirl.Where(x => x.Contains(mp)).FirstOrDefault();

            int idx = lbirl.IndexOf(lbir);
            if (idx < 0 && lb.Items.Count > 0) {
                //point not over any item in listbox
                //but this will still give either 0 or Count + 1 idx
                //Rect lbr = lb.GetListBoxRect();
                if (lbr.Contains(mp)) {
                    //point is still in listbox
                    //get first and last lbi rect's
                    var flbir = lb.GetListBoxItemRect(0,relativeTo);
                    var llbir = lb.GetListBoxItemRect(lb.Items.Count - 1, relativeTo);
                    if (lb.GetOrientation() == Orientation.Horizontal) {
                        if (mp.X >= 0 && mp.X <= flbir.Left) {
                            return 0;
                        }
                        if (mp.X >= llbir.Right && mp.X <= lbr.Right) {
                            return lb.Items.Count;
                        }
                    } else {
                        if (mp.Y >= 0 && mp.Y <= flbir.Top) {
                            return 0;
                        }
                        if (mp.Y >= llbir.Bottom && mp.Y <= lbr.Bottom) {
                            return lb.Items.Count;
                        }
                    }
                }
            }
            return idx;
        }

        public static Orientation GetOrientation(this ListBox lb) {
            var vsp = lb.GetVirtualItemsPanel();
            if (vsp == null) {
                return Orientation.Horizontal;
            }
            return vsp.Orientation;
        }

        public static VirtualizingStackPanel GetVirtualItemsPanel(this ListBox lb) {
            DependencyObject dio = lb.ItemsPanel.LoadContent();
            return dio?.GetVisualDescendent<VirtualizingStackPanel>();
        }

        public static Rect GetListBoxRect(this ListBox lb) {
            if (lb == null) {
                return new Rect();
            }
            return new Rect(new Point(0, 0), new Size(lb.ActualWidth, lb.ActualHeight));
        }

        #endregion

        #region ComboBox

        public static TextBox GetEditableTextBox(this ComboBox cmb) {
            return cmb.Template.FindName("PART_EditableTextBox", cmb) as TextBox;
        }
        #endregion

        #region Visual 

        public static double Distance(this Point from, Point to) {
            return Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        }


        internal static DependencyObject FindVisualTreeRoot(this DependencyObject d) {
            var current = d;
            var result = d;

            while (current != null) {
                result = current;
                if (current is Visual || current is Visual3D) {
                    break;
                } else {
                    // If we're in Logical Land then we must walk 
                    // up the logical tree until we find a 
                    // Visual/Visual3D to get us back to Visual Land.
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            return result;
        }

        public static T GetVisualAncestor<T>(this DependencyObject d) where T : class {
            var item = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());

            while (item != null) {
                var itemAsT = item as T;
                if (itemAsT != null) {
                    return itemAsT;
                }

                item = VisualTreeHelper.GetParent(item);
                if (item == null) {
                    item = VisualTreeHelper.GetParent(d);
                    d = item;
                }
            }

            return null;
        }

        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type itemSearchType, ItemsControl itemsControl, Type itemContainerSearchType) {
            if (itemsControl == null) throw new ArgumentNullException(nameof(itemsControl));
            if (itemContainerSearchType == null) throw new ArgumentNullException(nameof(itemContainerSearchType));

            var visualTreeRoot = d.FindVisualTreeRoot();
            var currentVisual = VisualTreeHelper.GetParent(visualTreeRoot);

            while (currentVisual != null && itemSearchType != null) {
                var currentVisualType = currentVisual.GetType();
                if (currentVisualType == itemSearchType || currentVisualType.IsSubclassOf(itemSearchType)) {
                    if (currentVisual is TreeViewItem || itemsControl.ItemContainerGenerator.IndexFromContainer(currentVisual) != -1) {
                        return currentVisual;
                    }
                }

                if (itemContainerSearchType.IsAssignableFrom(currentVisualType)) {
                    // ok, we found an ItemsControl (maybe an empty)
                    return null;
                }

                currentVisual = VisualTreeHelper.GetParent(currentVisual);
            }

            return null;
        }

        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type itemSearchType, ItemsControl itemsControl) {
            if (itemsControl == null) throw new ArgumentNullException(nameof(itemsControl));

            var visualTreeRoot = d.FindVisualTreeRoot();
            var currentVisual = VisualTreeHelper.GetParent(visualTreeRoot);
            DependencyObject lastFoundItemByType = null;

            while (currentVisual != null && itemSearchType != null) {
                if (currentVisual == itemsControl) {
                    return lastFoundItemByType;
                }

                var currentVisualType = currentVisual.GetType();
                if ((currentVisualType == itemSearchType || currentVisualType.IsSubclassOf(itemSearchType))
                    && (itemsControl.ItemContainerGenerator.IndexFromContainer(currentVisual) != -1)) {
                    lastFoundItemByType = currentVisual;
                }

                currentVisual = VisualTreeHelper.GetParent(currentVisual);
            }

            return lastFoundItemByType;
        }

        public static T GetVisualDescendent<T>(this DependencyObject d) where T : DependencyObject {
            return d.GetVisualDescendents<T>(null).FirstOrDefault();
        }

        public static T GetVisualDescendent<T>(this DependencyObject d, string childName) where T : DependencyObject {
            return d.GetVisualDescendents<T>(childName).FirstOrDefault();
        }

        public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d) where T : DependencyObject {
            return d.GetVisualDescendents<T>(null);
        }

        public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d, string childName) where T : DependencyObject {
            if(d == null) {
                yield break;
            }
            var childCount = VisualTreeHelper.GetChildrenCount(d);

            for (var n = 0; n < childCount; n++) {
                var child = VisualTreeHelper.GetChild(d, n);

                if (child is T descendent) {
                    if (string.IsNullOrEmpty(childName)
                        || descendent is IFrameworkInputElement frameworkInputElement && frameworkInputElement.Name == childName) {
                        yield return descendent;
                    }
                }

                foreach (var match in GetVisualDescendents<T>(child, childName)) {
                    yield return match;
                }
            }

            yield break;
        }

        public static Rect Bounds(this FrameworkElement fe, Visual rv = null) {
            //if(fe == rv || rv == null) {
            //    return fe.TransformToVisual(fe).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            //}
            //if(fe.IsDescendantOf(rv)) {
            //    return fe.TransformToAncestor(rv).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            //}
            //if(fe.IsAncestorOf(rv)) {
            //    return fe.TransformToDescendant(rv).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            //}
            //return fe.TransformToVisual(rv).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            Point origin = new Point();
            if(rv != null) {
                origin = fe.TranslatePoint(origin, (UIElement)rv);
            }
            return new Rect(origin, new Size(fe.ActualWidth, fe.ActualHeight));
        }        

        public static bool IsVisualDescendant(this DependencyObject parent, DependencyObject child) {
            if(parent == null || child == null) {
                return false;
            }
            foreach(var descendant in parent.FindChildren<UIElement>()) {
                if(descendant == child) {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<T> FindChildren<T>(this DependencyObject source)
                                             where T : DependencyObject {
            if (source != null) {
                var childs = GetChildObjects(source);
                foreach (DependencyObject child in childs) {
                    //analyze if children match the requested type
                    if (child != null && child is T) {
                        yield return (T)child;
                    }

                    //recurse tree
                    foreach (T descendant in FindChildren<T>(child)) {
                        yield return descendant;
                    }
                }
            }
        }

        public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent) {
            if (parent == null) yield break;


            if (parent is ContentElement || parent is FrameworkElement) {
                //use the logical tree for content / framework elements
                foreach (object obj in LogicalTreeHelper.GetChildren(parent)) {
                    var depObj = obj as DependencyObject;
                    if (depObj != null) yield return (DependencyObject)obj;
                }
            } else {
                //use the visual tree per default
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++) {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }

        public static T FindParentOfType<T>(this DependencyObject dpo) where T : class {
            if (dpo == null) {
                return default;
            }
            if (dpo.GetType() == typeof(T) || dpo.GetType().IsSubclassOf(typeof(T))) {
                return (dpo as T);
            }
            if (dpo.GetType().IsSubclassOf(typeof(FrameworkContentElement))) {
                if (((FrameworkContentElement)dpo).Parent != null) {
                    return FindParentOfType<T>(((FrameworkContentElement)dpo).Parent);
                }
                if (((FrameworkContentElement)dpo).TemplatedParent != null) {
                    return FindParentOfType<T>(((FrameworkContentElement)dpo).TemplatedParent);
                }

            } else if (dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                if (((FrameworkElement)dpo).Parent != null) {
                    return FindParentOfType<T>(((FrameworkElement)dpo).Parent);
                }
                if (((FrameworkElement)dpo).TemplatedParent != null) {
                    return FindParentOfType<T>(((FrameworkElement)dpo).TemplatedParent);
                }
            }

            return null;
        }
        #endregion

        #region Documents


        #endregion

        #region Images
        //faster version but needs unsafe thing
        //public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset) {
        //    fixed (PixelColor* buffer = &pixels[0, 0])
        //        source.CopyPixels(
        //          new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
        //          (IntPtr)(buffer + offset),
        //          pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
        //          stride);
        //}


        //public static Color ToWinColor(this SKColor skc) {
        //    return Color.FromArgb(skc.Alpha, skc.Red, skc.Green, skc.Blue);
        //}
        #endregion

        #region Mail
        //Extension method for MailMessage to save to a file on disk
        public static void Save(this MailMessage message, string filename, bool addUnsentHeader = true) {
            using (var filestream = File.Open(filename, FileMode.Create)) {
                if (addUnsentHeader) {
                    var binaryWriter = new BinaryWriter(filestream);
                    //Write the Unsent header to the file so the mail client knows this mail must be presented in "New message" mode
                    //binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                    binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                }

                var assembly = typeof(SmtpClient).Assembly;
                var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

                // Get reflection info for MailWriter contructor
                var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);

                // Construct MailWriter object with our FileStream
                var mailWriter = mailWriterContructor.Invoke(new object[] { filestream });

                // Get reflection info for Send() method on MailMessage
                var sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

                sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { mailWriter, true, true }, null);

                // Finally get reflection info for Close() method on our MailWriter
                var closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);
            }
        }
        #endregion

        
    }
}
