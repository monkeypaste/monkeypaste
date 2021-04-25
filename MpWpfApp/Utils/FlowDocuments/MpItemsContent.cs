using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpItemsContent : Section {
        private static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MpItemsContent), new PropertyMetadata(OnItemsSourceChanged));
        private static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(MpItemsContent), new PropertyMetadata(OnItemTemplateChanged));
        private static readonly DependencyProperty ItemsPanelProperty = DependencyProperty.Register("ItemsPanel", typeof(DataTemplate), typeof(MpItemsContent), new PropertyMetadata(OnItemsPanelChanged));

        public MpItemsContent() {
            MpFlowDocumentHelpers.FixupDataContext(this);
            Loaded += ItemsContent_Loaded;
        }

        private void ItemsContent_Loaded(object sender, RoutedEventArgs e) {
            GenerateContent(ItemsPanel, ItemTemplate, ItemsSource);
        }

        public IEnumerable ItemsSource {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public DataTemplate ItemsPanel {
            get { return (DataTemplate)GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        private void GenerateContent(DataTemplate itemsPanel, DataTemplate itemTemplate, IEnumerable itemsSource) {
            Blocks.Clear();
            if (itemTemplate != null && itemsSource != null) {
                FrameworkContentElement panel = null;

                foreach (object data in itemsSource) {
                    if (panel == null) {
                        if (itemsPanel == null)
                            panel = this;
                        else {
                            FrameworkContentElement p = MpFlowDocumentHelpers.LoadDataTemplate(itemsPanel);
                            if (!(p is Block))
                                throw new Exception("ItemsPanel must be a block element");
                            Blocks.Add((Block)p);
                            panel = MpAttached.GetItemsHost(p);
                            if (panel == null)
                                throw new Exception("ItemsHost not found. Did you forget to specify Attached.IsItemsHost?");
                        }
                    }
                    FrameworkContentElement element = MpFlowDocumentHelpers.LoadDataTemplate(itemTemplate);
                    element.DataContext = data;
                    MpFlowDocumentHelpers.UnFixupDataContext(element);
                    if (panel is Section)
                        ((Section)panel).Blocks.Add(MpFlowDocumentHelpers.ConvertToBlock(data, element));
                    else if (panel is TableRowGroup)
                        ((TableRowGroup)panel).Rows.Add((TableRow)element);
                    else
                        throw new Exception(String.Format("Don't know how to add an instance of {0} to an instance of {1}", element.GetType(), panel.GetType()));
                }
            }
        }

        private void GenerateContent() {
            GenerateContent(ItemsPanel, ItemTemplate, ItemsSource);
        }

        private void OnItemsSourceChanged(IEnumerable newValue) {
            if (IsLoaded)
                GenerateContent(ItemsPanel, ItemTemplate, newValue);
        }

        private void OnItemTemplateChanged(DataTemplate newValue) {
            if (IsLoaded)
                GenerateContent(ItemsPanel, newValue, ItemsSource);
        }

        private void OnItemsPanelChanged(DataTemplate newValue) {
            if (IsLoaded)
                GenerateContent(newValue, ItemTemplate, ItemsSource);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((MpItemsContent)d).OnItemsSourceChanged((IEnumerable)e.NewValue);
        }

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((MpItemsContent)d).OnItemTemplateChanged((DataTemplate)e.NewValue);
        }

        private static void OnItemsPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((MpItemsContent)d).OnItemsPanelChanged((DataTemplate)e.NewValue);
        }
    }
}
