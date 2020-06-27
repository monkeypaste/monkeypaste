using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpSearchHightlightTextBlock : TextBlock {
        public MpSearchHightlightTextBlock() : base() { }

        public String SearchText {
            get { return (String)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        private static void OnDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                new Action(() => OnDataChangedHelper((TextBlock)source,(string)e.NewValue))
            );

        }
        private static void OnDataChangedHelper(TextBlock tb, string updatedSearchText) {
            if (tb.Text.Length == 0)
                return;

            string textUpper = tb.Text.ToUpper();
            String toFind = updatedSearchText.ToUpper();
            int firstIndex = textUpper.IndexOf(toFind);
            if (firstIndex < 0) {
                return;
            }
            String firstStr = tb.Text.Substring(0, firstIndex);
            String foundStr = tb.Text.Substring(firstIndex, toFind.Length);
            String endStr = tb.Text.Substring(firstIndex + toFind.Length,
                                             tb.Text.Length - (firstIndex + toFind.Length));

            tb.Inlines.Clear();
            var run = new Run();
            run.Text = firstStr;
            tb.Inlines.Add(run);
            run = new Run();
            run.Background = Brushes.Yellow;
            run.Text = foundStr;
            tb.Inlines.Add(run);
            run = new Run();
            run.Text = endStr;

            tb.Inlines.Add(run);
        }
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText",
                                        typeof(String),
                                        typeof(MpSearchHightlightTextBlock),
                                        new FrameworkPropertyMetadata(null,OnDataChanged));
    }
}
