using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.Win32.TaskScheduler;
using MonkeyPaste;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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

namespace MpWpfApp {
    public static class MpExtensions {
        #region Collections

        public static void Refresh(this CollectionView cv,[CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            cv.Refresh();
            MpConsole.WriteTraceLine("Collection refreshed",null,callerName,callerFilePath,lineNum);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (T item in source)
                action(item);
        }

        public static bool IsEmpty<T>(this IList<T> source) {
            return source.Count == 0;
        }

        public static Point[] GetAdornerPoints(this ListBox lb, int index, bool isListBoxHorizontal) {
            var points = new Point[2];
            var itemRect = index >= lb.Items.Count ? lb.GetListBoxItemRect(lb.Items.Count - 1) :lb.GetListBoxItemRect(index);
            if (!isListBoxHorizontal) {
                itemRect.Height = MpMeasurements.Instance.ClipTileContentItemMinHeight;
            }
            if (isListBoxHorizontal) {
                if (index < lb.Items.Count) {
                    points[0] = itemRect.TopLeft;
                    points[1] = itemRect.BottomLeft;
                } else {
                    points[0] = itemRect.TopRight;
                    points[1] = itemRect.BottomRight;
                }
            } else {
                if (index < lb.Items.Count) {
                    points[0] = itemRect.TopLeft;
                    points[1] = itemRect.TopRight;
                } else {
                    points[0] = itemRect.BottomLeft;
                    points[1] = itemRect.BottomRight;
                }
            }
            var sv = lb.GetScrollViewer();
            if (sv != null &&
                (sv.HorizontalOffset > 0 || sv.VerticalOffset > 0)) {
                points[0].X += sv.Margin.Right;
                //points[0].Y += ScrollViewer.VerticalOffset;
                points[1].X += sv.Margin.Right;
                //points[1].Y += ScrollViewer.VerticalOffset;
            }
            return points;
        }


        public static ScrollViewer GetScrollViewer(this ListBox lb) {
            return lb.GetVisualDescendent<ScrollViewer>();
        }

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
            10   if Shift key is down
            11   if there is a previously selected item, clear selection and then add between target item and first previously selected item
            12   else remove any other item from selection
            */
            //if (ListBox.DataContext is MpClipTileRichTextBoxViewModelCollection) {
            //    var hctvm = (ListBox.DataContext as MpClipTileRichTextBoxViewModelCollection).HostClipTileViewModel;
            //    MpClipTrayViewModel.Instance.UpdateExtendedSelection(MpClipTrayViewModel.Instance.IndexOf(hctvm));
            //}
            bool isCtrlDown = MpHelpers.Instance.GetModKeyDownList().Contains(Key.LeftCtrl);
            bool isShiftDown = MpHelpers.Instance.GetModKeyDownList().Contains(Key.LeftShift);
            var lbi = lb.GetListBoxItem(index);
            if (!lbi.IsSelected) {
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
                    // MpClipTrayViewModel.Instance.ClearClipSelection(false);
                    //HostClipTileViewModel.IsSelected = true;
                    lb.SelectedItems.Clear();
                    lbi.IsSelected = true;
                }
            } else if (lbi.IsSelected) {
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

                    //MpClipTrayViewModel.Instance.ClearClipSelection(false);
                    //HostClipTileViewModel.IsSelected = true;
                    lb.SelectedItems.Clear();
                    lbi.IsSelected = true;
                }
            }
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

        /// <summary>
        /// Gets the next ancestor element which is a drop target.
        /// </summary>
        /// <param name="element">The start element.</param>
        /// <returns>The first element which is a drop target.</returns>
        public static UIElement TryGetNextAncestorDropTargetElement(this UIElement element) {
            if (element == null) {
                return null;
            }

            var ancestor = element.GetVisualAncestor<UIElement>();
            while (ancestor != null) {
                if (ancestor.IsDropTarget()) {
                    return ancestor;
                }

                ancestor = ancestor.GetVisualAncestor<UIElement>();
            }

            return null;
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

        public static T GetVisualAncestor<T>(this DependencyObject d)
            where T : class {
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

        /// <summary>
        /// find the visual ancestor item by type
        /// </summary>
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

        /// <summary>
        /// find the visual ancestor by type and go through the visual tree until the given itemsControl will be found
        /// </summary>
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

        public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d, string childName)
            where T : DependencyObject {
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

        #endregion

        #region Visual Tree

        public static Rect RelativeBounds(this FrameworkElement fe, Visual rv = null) {
            if(fe == rv || rv == null) {
                return fe.TransformToVisual(fe).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            }
            if(fe.IsDescendantOf(rv)) {
                return fe.TransformToAncestor(rv).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            }
            if(fe.IsAncestorOf(rv)) {
                return fe.TransformToDescendant(rv).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
            }
            return fe.TransformToVisual(rv).TransformBounds(LayoutInformation.GetLayoutSlot(fe));
        }

        public static bool IsListBoxItemVisible(this ListBox lb, int index) {
            var lbi = lb.GetListBoxItem(index);
            if (lbi != null && lbi.Visibility == Visibility.Visible) {
                var lbir = lb.GetListBoxItemRect(index);
                if (lbir.Left < lb.GetScrollViewer().HorizontalOffset) {
                    return false;
                }
                if (lbir.Right > lb.GetListBoxRect().Right + lb.GetScrollViewer().HorizontalOffset) {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static ListBoxItem GetListBoxItem(this ListBox lb, int index) {
            if (lb == null) {
                return null;
            }
            //if (lb.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) {
            //    return null;
            //}
            if (index < 0 || index >= lb.Items.Count) {
                return null;
            }
            return lb.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        public static ListBoxItem GetListBoxItem(this ListBox lb, object dataContext) {
            if (lb == null) {
                return null;
            }
            //if (lb.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) {
            //    return null;
            //}

            for (int i = 0; i < lb.Items.Count; i++) {
                var lbi = lb.Items[i];
                if(lbi == dataContext) {
                    return lb.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                }
            }
            return null;
        }


        public static Rect GetListBoxItemRect(this ListBox lb, int index, bool ignoreScrollViewer = false) {            
            var lbi = lb.GetListBoxItem(index);
            if (lbi == null || lbi.Visibility != Visibility.Visible) {
                return new Rect();
            }
            Point origin = new Point(); 
            if (ignoreScrollViewer && (lb.GetScrollViewer().HorizontalOffset > 0 || lb.GetScrollViewer().VerticalOffset > 0)) {
                origin = lbi.TranslatePoint(new Point(0, 0), lb.GetScrollViewer());
            } else {
                origin = lbi.TranslatePoint(new Point(0, 0), lb);
            }
            return new Rect(origin, new Size(lbi.ActualWidth, lbi.ActualHeight));
        }

        public static List<Rect> GetListBoxItemRects(this ListBox lb, Visual relativeTo = null) {
            relativeTo = relativeTo == null ? lb : relativeTo;
            var rl = new List<Rect>();
            for(int i = 0;i < lb.Items.Count;i++) {
                rl.Add(lb.GetListBoxItemRect(i)); 
            }
            return rl;
        }

        public static ListBoxItem GetItemAtPoint(this ListBox lb, Point p) {
            int idx = lb.GetItemIndexAtPoint(p);
            return idx < 0 ? null : lb.GetListBoxItem(idx);
        }

        public static int GetItemIndexAtPoint(this ListBox lb, Point rp) {
            var lbirl = lb.GetListBoxItemRects();
            var lbir = lbirl.Where(x => x.Contains(rp)).FirstOrDefault();

            int idx = lbirl.IndexOf(lbir);
            if(idx < 0 && lb.Items.Count > 0) {
                //point not over any item in listbox
                //but this will still give either 0 or Count + 1 idx
                Rect lbr = lb.GetListBoxRect();
                if(lbr.Contains(rp)) {
                    //point is still in listbox
                    //get first and last lbi rect's
                    var flbir = lb.GetListBoxItemRect(0); 
                    var llbir = lb.GetListBoxItemRect(lb.Items.Count - 1);
                    if (lb.GetOrientation() == Orientation.Horizontal) {
                        if(rp.X >= 0 && rp.X <= flbir.Left) {
                            return 0;
                        }                        
                        if(rp.X >= llbir.Right && rp.X <= lbr.Right) {
                            return lb.Items.Count;
                        }
                    } else {
                        if (rp.Y >= 0 && rp.Y <= flbir.Top) {
                            return 0;
                        }
                        if (rp.Y >= llbir.Bottom && rp.Y <= lbr.Bottom) {
                            return lb.Items.Count;
                        }
                    }
                }                
            }
            return idx;
        }

        public static Orientation GetOrientation(this ListBox lb) {
            var vsp = lb.GetVirtualItemsPanel();
            if(vsp == null) {
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

        public static IEnumerable<DependencyObject> GetChildObjects(
                                                    this DependencyObject parent) {
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

        //public static T FindParentOfType<T>(this DependencyObject dpo) where T : class {
        //    if (dpo == null) {
        //        return default;
        //    }
        //    if (dpo is T t) {
        //        return t;
        //    }
        //    if (dpo is FrameworkContentElement fce) {
        //        if(fce.Parent != null) {
        //            return FindParentOfType<T>(fce.Parent);
        //        } 
        //        if(fce.TemplatedParent != null) {
        //            return FindParentOfType<T>(fce.TemplatedParent);
        //        }

        //    } else if (dpo is FrameworkElement fe) {
        //        if (fe.Parent != null) {
        //            return FindParentOfType<T>(fe.Parent);
        //        } 
        //        if (fe.TemplatedParent != null) {
        //            return FindParentOfType<T>(fe.TemplatedParent);
        //        }
        //    }
        //    return null;
        //}
        public static T FindParentOfType<T>(this DependencyObject dpo) where T : class {
            if (dpo == null) {
                return default;
            }
            if (dpo.GetType() == typeof(T)) {
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

        #region Colors
        public static string ToHex(this Brush b) {
            if(b is SolidColorBrush scb) {
                return scb.Color.ToHex();
            }
            throw new Exception("Brush must be solid color brush but is "+b.GetType().ToString());
        }

        public static string ToHex(this Color c) {
            return MpHelpers.Instance.ConvertColorToHex(c);
        }

        public static Brush ToSolidColorBrush(this string hex) {
            return (Brush)new SolidColorBrush(hex.ToWinMediaColor());
        }

        public static Color ToWinMediaColor(this string hex) {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        #endregion

        #region Documents

        public static void UpdateLayout(this UIElement rtb) {
            rtb.UpdateLayout();
        }

        public static void FitDocToRtb(this RichTextBox rtb) {
            rtb.Document.PageWidth = Math.Max(0,rtb.ActualWidth - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
            rtb.Document.PageHeight = Math.Max(0,rtb.ActualHeight - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            rtb.UpdateLayout();
        }


        public static BitmapSource ToBitmapSource(this FlowDocument fd, Brush bgBrush = null) {
            return MpHelpers.Instance.ConvertFlowDocumentToBitmap(
                                fd.Clone(),
                                fd.GetDocumentSize(),
                                bgBrush);
        }

        public static BitmapSource ToBitmapSource(this string str) {
            return MpHelpers.Instance.ConvertStringToBitmapSource(str);
        }

        public static string ToBase64String(this BitmapSource bmpSrc) {
            return MpHelpers.Instance.ConvertBitmapSourceToBase64String(bmpSrc);
        }

        public static bool Equals(this TextRange tra, TextRange trb) {
            if(!tra.Start.IsInSameDocument(trb.Start)) {
                return false;
            }
            if (tra.Start.CompareTo(trb.Start) == 0 && tra.End.CompareTo(trb.End) == 0) {
                return true;
            }
            return false;
        }

        public static MpEventEnabledFlowDocument Clone(this FlowDocument doc) {
            using (MemoryStream stream = new MemoryStream()) {
                var clonedDoc = new MpEventEnabledFlowDocument();
                TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
                System.Windows.Markup.XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);
                TextRange range2 = new TextRange(clonedDoc.ContentEnd, clonedDoc.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);
                return clonedDoc;
            }
        }

        public static StringCollection ToStringCollection(this IEnumerable<string> strings) {
            var stringCollection = new StringCollection();
            foreach (string s in strings) {
                stringCollection.Add(s);
            }
            return stringCollection;
        }

        public static TextRange Clone(this TextSelection ts) {
            return new TextRange(ts.Start, ts.End);
        }

        public static void SetRtf(this System.Windows.Controls.RichTextBox rtb, string document) {
            //var rtbSelection = rtb.Selection;
            var documentBytes = UTF8Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.SelectAll();
                //rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.CaretPosition = rtb.Document.ContentStart;
                //if (rtbSelection != null) {
                //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                //}
            }
        }

        public static void SetRtf(this FlowDocument fd, string document) {
            //var rtbSelection = rtb.Selection;
            var documentBytes = UTF8Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                new TextRange(fd.ContentStart, fd.ContentEnd).Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.SelectAll();
                //rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.CaretPosition = rtb.Document.ContentStart;
                //if (rtbSelection != null) {
                //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                //}
            }
        }

        public static string GetRtf(this RichTextBox rtb) {
            return MpHelpers.Instance.ConvertFlowDocumentToRichText(rtb.Document);
        }

        public static void SetXaml(this System.Windows.Controls.RichTextBox rtb, string document) {
            var documentBytes = Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Xaml);
            }
        }

        public static IEnumerable<TextElement> GetRunsAndParagraphs(this FlowDocument doc) {
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        yield return run;
                    } else {
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            yield return para;
                        }
                    }
                }
            }
        }

        public static bool IsBase64String(this string str) {
            if (str.IsStringRichText()) {
                return false;
            }
            try {
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(str);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                return (str.Replace(" ", "").Length % 4 == 0);
            }
            catch {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }

        public static bool IsStringCsv(this string text) {
            if (string.IsNullOrEmpty(text) || IsStringRichText(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public static bool IsStringRichText(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public static bool IsStringXaml(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public static bool IsStringSpan(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Span xmlns=");
        }

        public static bool IsStringSection(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=");
        }

        public static bool IsStringQuillText(this string text) {
            if(IsStringPlainText(text)) {
                foreach(var qt in MpQuillFormatProperties.Instance.QuillOpenTags) {
                    if(text.Contains(qt)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsStringPlainText(this string text) {
            //returns true for csv
            if (text == null) {
                return false;
            }
            if (text == string.Empty) {
                return true;
            }
            if (IsStringRichText(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
                return false;
            }
            return true;
        }

        public static string ToQuillText(this string text) {
            if(text.IsStringQuillText()) {
                return text;
            }
            return MpRtfToHtmlConverter.Instance.ConvertRtfToHtml(text.ToRichText());
        }

        public static string ToRichText(this string str) {
            if(str == null) {
                str = string.Empty;
            }
            if(str.IsStringRichText()) {
                return str;
            }
            if(str.IsStringQuillText()) {
                return MpHtmlToRtfConverter.Instance.ConvertHtmlToRtf(str);
            }
            if(str.IsStringXaml()) {
                return MpHelpers.Instance.ConvertXamlToRichText(str);
            }
            return MpHelpers.Instance.ConvertPlainTextToRichText(str);
        }

        public static string ToPlainText(this string str) {
            if (str == null) {
                return string.Empty;
            }
            if (MpHelpers.Instance.IsStringPlainText(str)) {
                return str;
            }
            return MpHelpers.Instance.ConvertRichTextToPlainText(str);
        }

        public static string ToRichText(this FlowDocument doc) {
            return MpHelpers.Instance.ConvertFlowDocumentToRichText(doc);
        }

        public static string ToPlainText(this FlowDocument doc) {
            return doc.ToRichText().ToPlainText();
        }

        public static MpEventEnabledFlowDocument ToFlowDocument(this string str) {
            if(string.IsNullOrEmpty(str)) {
                return MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty));
            }
            if(str.IsStringQuillText()) {
                return str.ToRichText().ToFlowDocument();
            }
            if(MpHelpers.Instance.IsStringPlainText(str)) {
                return MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(str));
            }
            if(str.IsStringXaml()) {
                return MpHelpers.Instance.ConvertXamlToFlowDocument(str); 
            }
            if(MpHelpers.Instance.IsStringRichText(str)) {
                return MpHelpers.Instance.ConvertRichTextToFlowDocument(str);
            }
            throw new Exception("ToFlowDocument exception string must be plain or rich text. Its content is: " + str);
        }

        public static string ToXaml(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return string.Empty.ToRichText().ToXaml();
            }
            if (str.IsStringQuillText()) {
                return str.ToRichText().ToXaml();
            }
            if (MpHelpers.Instance.IsStringPlainText(str)) {
                return str.ToRichText().ToXaml();
            }
            if (MpHelpers.Instance.IsStringRichText(str)) {
                return MpHelpers.Instance.ConvertRichTextToXaml(str);
            }
            throw new Exception("ToXaml exception string must be plain or rich text. Its content is: " + str);
        }

        public static string ToXaml(this FlowDocument fd) {
            return MpHelpers.Instance.ConvertFlowDocumentToXaml((MpEventEnabledFlowDocument)fd);
        }

        public static Size GetDocumentSize(this FlowDocument doc) {
            var ft = doc.GetFormattedText();
            return new Size(ft.Width, ft.Height);
        }

        public static List<KeyValuePair<TextRange, Brush>> FindNonTransparentRangeList(this RichTextBox rtb) {
            var matchRangeList = new List<KeyValuePair<TextRange, Brush>>();
            TextSelection rtbSelection = rtb.Selection;
            var doc = rtb.Document;
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        if (run.Background != null && run.Background != Brushes.Transparent) {
                            matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(run.ContentStart, run.ContentEnd), run.Background));
                        }
                    } else {
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            if (para.Background != null && para.Background != Brushes.Transparent) {
                                matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(para.ContentStart, para.ContentEnd), para.Background));
                            }
                        } else {
                            var span = position.Parent as Span;
                            if(span != null) {
                                if (span.Background != null && span.Background != Brushes.Transparent) {
                                    matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(span.ContentStart, span.ContentEnd), span.Background));
                                }
                            }
                        }
                    }
                }
            }
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return matchRangeList;
        }

        public static FormattedText GetFormattedText(this FlowDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            var output = new FormattedText(
              GetText(doc),
              CultureInfo.CurrentCulture,
              doc.FlowDirection,
              new Typeface(doc.FontFamily, doc.FontStyle, doc.FontWeight, doc.FontStretch),
              doc.FontSize,
              doc.Foreground,
              new NumberSubstitution(),
              Properties.Settings.Default.ThisAppDip);

            int offset = 0;
            var runsAndParagraphsList = doc.GetRunsAndParagraphs().ToList();
            for (int i = 0; i < runsAndParagraphsList.Count; i++) {
                TextElement el = runsAndParagraphsList[i];
                Run run = el as Run;

                if (run != null) {
                    int count = run.Text.Length;
                    
                    output.SetFontFamily(run.FontFamily, offset, count);
                    output.SetFontStyle(run.FontStyle, offset, count);
                    output.SetFontWeight(run.FontWeight, offset, count);
                    output.SetFontSize(run.FontSize, offset, count);
                    output.SetForegroundBrush(run.Foreground, offset, count);
                    output.SetFontStretch(run.FontStretch, offset, count);
                    output.SetTextDecorations(run.TextDecorations, offset, count);

                    offset += count;
                } else {                    
                    offset += Environment.NewLine.Length;
                }
            }

            return output;
        }

        public static string InlineInnerText(this HtmlAgilityPack.HtmlNode htmlNode) {
            return htmlNode.InnerText.Replace(Environment.NewLine, string.Empty);
        }

        public static string GetText(FlowDocument doc) {
            StringBuilder sb = new StringBuilder();

            foreach (TextElement el in GetRunsAndParagraphs(doc)) {
                Run run = el as Run;
                sb.Append(run == null ? Environment.NewLine : run.Text);
            }
            return sb.ToString();
        }

        public static bool ContainsByCaseSetting(this string str, string compareStr) {
            return str.ContainsByCase(compareStr, Properties.Settings.Default.SearchByIsCaseSensitive);
        }

        public static bool ContainsByCase(this string str, string compareStr, bool isCaseSensitive) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(compareStr)) {
                return false;
            }
            if (isCaseSensitive) {
                return str.Contains(compareStr);
            }
            return str.ToLower().Contains(compareStr.ToLower());
        }

        public static List<int> IndexListOfAll(this string str, string compareStr) {
            return MpHelpers.Instance.IndexListOfAll(str, compareStr);
        }
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
        public static bool IsEqual(this BitmapSource image1, BitmapSource image2) {
            if (image1 == null || image2 == null) {
                return false;
            }
            return image1.ToByteArray().SequenceEqual(image2.ToByteArray());
        }

        public static byte[] ToByteArray(this BitmapSource source) {
            return MpHelpers.Instance.ConvertBitmapSourceToByteArray(source);
        }
        public static BitmapSource ToBitmapSource(this byte[] byteArray) {
            return MpHelpers.Instance.ConvertByteArrayToBitmapSource(byteArray);
        }
        public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset, bool dummy) {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    pixels[x + x0, y + y0] = new PixelColor {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
                }
            }
        }

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

        #region Reflection


        #endregion
    }
}
