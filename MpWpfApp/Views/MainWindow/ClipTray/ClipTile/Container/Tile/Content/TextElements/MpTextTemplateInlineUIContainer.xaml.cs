using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpFileItemParagraph.xaml
    /// </summary>
    public partial class MpTextTemplateInlineUIContainer : InlineUIContainer {
        public static MpTextTemplateInlineUIContainer Create(TextRange tr, MpTextTemplateViewModelBase tvm) {
            tr.Text = string.Empty;
            var ttiuic = new MpTextTemplateInlineUIContainer(tr, tvm);
            return ttiuic;
        }
        public MpTextTemplateInlineUIContainer() : base() {
            InitializeComponent();
        }
        private MpTextTemplateInlineUIContainer(TextRange tr, MpTextTemplateViewModelBase tvm) : base(null, tr.Start) {
            DataContext = tvm;
            InitializeComponent();
            //Child = TextTemplateBorder;
        }

        private void Border_Loaded(object sender, RoutedEventArgs e) {
            if(ContentStart.Paragraph != null &&
                ContentStart.Paragraph.LineHeight < TextTemplateBorder.ActualHeight) {
                ContentStart.Paragraph.LineHeight = TextTemplateBorder.ActualHeight;
            } 
        }

        private void InlineUIContainer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var tvm = DataContext as MpTextTemplateViewModelBase;
            if(tvm == null) {
                Debugger.Break();
                return;
            }
            var ctvm = tvm.Parent.Parent as MpClipTileViewModel;
            if(ctvm == null) {
                Debugger.Break();
                return;
            }
            if (!tvm.Parent.Parent.IsSelected) {
                tvm.Parent.Parent.IsSelected = true;
            }
            if (!ctvm.IsContentReadOnly) {
                tvm.Parent.SelectedItem = tvm;
                if (!ctvm.IsPasting) {
                    tvm.EditTemplateCommand.Execute(null);
                    e.Handled = true;
                }
            }
            if (ctvm.IsSubSelectionEnabled || !ctvm.IsContentReadOnly) {
                MpDragDropManager.StartDragCheck(ctvm);
            }
        }

        private void InlineUIContainer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            var tvm = DataContext as MpTextTemplateViewModelBase;
            if (tvm == null) {
                Debugger.Break();
                return;
            }
            var ctvm = tvm.Parent.Parent as MpClipTileViewModel;
            if (ctvm == null) {
                Debugger.Break();
                return;
            }
            if (!tvm.Parent.Parent.IsSelected) {
                tvm.Parent.Parent.IsSelected = true;
            }
            if (!ctvm.IsContentReadOnly) {
                tvm.Parent.SelectedItem = tvm;
                if (!ctvm.IsPasting) {
                    var origin = ContentStart.GetCharacterRect(LogicalDirection.Forward).Location;
                    origin = ContentStart.Parent.FindParentOfType<RichTextBox>().TranslatePoint(origin, Application.Current.MainWindow);

                    MpContextMenuView.Instance.DataContext = tvm.MenuItemViewModel;
                    //MpContextMenuView.Instance.PlacementRectangle = new Rect(origin,new Size(200,50));

                    this.ContextMenu = MpContextMenuView.Instance;
                    MpContextMenuView.Instance.IsOpen = true;
                }
                e.Handled = true;
            }
        }

        private void TextTemplateBorder_Unloaded(object sender, RoutedEventArgs e) {
            MpHelpers.RunOnMainThread(async () => {
                var rtb = ContentStart.Parent.FindParentOfType<RichTextBox>();
                if(rtb == null) {
                    return;
                }
                var ctvm = rtb.DataContext as MpClipTileViewModel;
                if(ctvm == null) {
                    return;
                }
                var cit = Tag as MpTextTemplate;

                if (rtb != null &&
                    !rtb.IsReadOnly &&
                    !ctvm.IsPastingTemplate &&
                    Mouse.LeftButton == MouseButtonState.Released &&
                    !MpClipTrayViewModel.Instance.HasScrollVelocity &&
                    !MpClipTrayViewModel.Instance.IsRequery) {
                    //while editing if template is removed check if its the only one if so remove from db and tcvm
                    var iuicl = rtb.Document.GetAllTextElements().Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate);
                    if (iuicl != null && iuicl.All(x => (x.Tag as MpTextTemplate).Id != cit.Id)) {
                        var tcvm = (rtb.DataContext as MpClipTileViewModel).TemplateCollection;
                        var toRemove_tvml = tcvm.Items.Where(x => x.TextTemplateGuid == cit.Guid).ToList();
                        foreach (var toRemove_tvm in toRemove_tvml) {
                            MpConsole.WriteTraceLine($"Template {toRemove_tvm} unloaded in delete state, so its gone now.");
                            tcvm.Items.Remove(toRemove_tvm);
                        }
                        await Task.WhenAll(toRemove_tvml.Select(x => x.TextTemplate.DeleteFromDatabaseAsync()));

                        tcvm.OnPropertyChanged(nameof(tcvm.Items));
                    }
                }
            });
        }

        private void TextTemplateTextBlock_Loaded(object sender, RoutedEventArgs e) {
            var tvm = DataContext as MpTextTemplateViewModelBase;

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.RichTextFormat)),
                   target: new MpRichTextFormatExtension(),
                   targetProperty: MpRichTextFormatExtension.RichTextFormatProperty);

            MpRichTextFormatExtension.SetIsEnabled(TextTemplateTextBlock, true);
        }

        public override string ToString() {
            if(Tag is MpTextTemplate cit) {
                return cit.ToString();
            }
            return base.ToString();
        }
    }
}
