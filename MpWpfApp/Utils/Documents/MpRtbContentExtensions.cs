using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.Xml;
using System.Windows.Markup;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Windows.Data;

namespace MpWpfApp {
    public static class MpRtbContentExtensions {
        public static InlineUIContainer LoadImage(
            this TextRange tr, 
            string base64Str, 
            out Size unformattedContentSize) {
            if (!base64Str.IsStringBase64()) {
                unformattedContentSize = new Size();
                return null;
            }

            BitmapSource bmpSrc = base64Str.ToBitmapSource();//.Resize(docSize.Value);
            unformattedContentSize = new Size(bmpSrc.PixelWidth, bmpSrc.PixelHeight);
            
            var img = new Image() {
                Source = bmpSrc,
                Width = MpMeasurements.Instance.ClipTileContentDefaultSize.Width,
                Height = MpMeasurements.Instance.ClipTileContentDefaultSize.Height,
                Stretch = System.Windows.Media.Stretch.Uniform
            };

            //tr.Text = string.Empty;

            using (var subStream = new MemoryStream()) {
                var subDoc = new FlowDocument();
                //var sub_tr = new TextRange(subDoc.ContentStart, subDoc.ContentEnd);

                var iuc = new InlineUIContainer(img);
                var p = new Paragraph(iuc);
                subDoc.Blocks.Add(p);
                p.ContentRange().Save(subStream, DataFormats.XamlPackage, true);
                subStream.Seek(0, SeekOrigin.Begin);
                tr.Load(subStream, DataFormats.XamlPackage);
                tr.Start.Parent.FindParentOfType<FlowDocument>().LineStackingStrategy = LineStackingStrategy.MaxHeight;

                return tr.Start.Parent as InlineUIContainer;
            }

            // return new InlineUIContainer(img,tr.Start);
        }

        public static FlowDocument ToImageDocument(this string base64Str, out Size unformattedContentSize) {
            if (!base64Str.IsStringBase64()) {
                unformattedContentSize = new Size();
                return string.Empty.ToFlowDocument();
            }

            //BitmapSource bmpSrc = base64Str.ToBitmapSource();

            //var img = new Image() {
            //    Source = bmpSrc,
            //    Width = bmpSrc.Width,
            //    Height = bmpSrc.Height,
            //    Stretch = System.Windows.Media.Stretch.Uniform
            //};

            var fd = string.Empty.ToFlowDocument();
            
            var p = fd.Blocks.FirstBlock as Paragraph;
            p.ContentRange().LoadImage(base64Str, out Size imgSize);
            unformattedContentSize = imgSize;

            fd.ConfigureLineHeight();
            p.ContentRange().ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Center);

            return fd;
        }

        public static Paragraph LoadFileItem(this TextRange tr, string path, int iconId = 0, double iconSize = 16) {
            string iconBase64 = string.Empty;

            if (iconId > 0 && path.IsFileOrDirectory()) {
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);
                if (ivm == default) {
                    iconBase64 = MpBase64Images.Warning;
                } else {
                    iconBase64 = ivm.IconBase64;
                }
            } else if (path.IsFileOrDirectory()) {
                iconBase64 = MpShellEx.GetBitmapFromPath(path, MpIconSize.SmallIcon16).ToBase64String();
            }
            if (string.IsNullOrEmpty(iconBase64)) {
                iconBase64 = MpBase64Images.Warning;
            }

            BitmapSource bmpSrc = iconBase64.ToBitmapSource();
            var pathIcon = new Image() {
                Source = bmpSrc,
                Width = iconSize,
                Height = iconSize,
                Stretch = System.Windows.Media.Stretch.Fill
            };

            var iuc = new InlineUIContainer(pathIcon) {
                BaselineAlignment = BaselineAlignment.Bottom
            };

            Paragraph p = new Paragraph(iuc) {
               // Margin = new Thickness(iconSize, 0, 0, 0),
               // TextIndent = -iconSize
            };

            ToolTipService.SetInitialShowDelay(p, 300);

            string pathDir = path;
            if (File.Exists(pathDir)) {
                pathDir = Path.GetDirectoryName(pathDir);
            }

            var pathRun = new Run(Path.GetFileName(path));
            var pathLink = new Hyperlink(pathRun) {
                IsEnabled = true,
                NavigateUri = new Uri(pathDir, UriKind.Absolute)
            };

            p.Inlines.Add(new Run(" "));
            p.Inlines.Add(pathLink);

            double fontSize = 16;
            p.FontFamily = new FontFamily(MpPrefViewModel.Instance.DefaultFontFamily);
            p.FontSize = fontSize;


            using (var stream = new MemoryStream()) {
                var subDoc = new FlowDocument();
                subDoc.Blocks.Add(p);
                p.ContentRange().Save(stream, DataFormats.XamlPackage, true);
                stream.Seek(0, SeekOrigin.Begin);
                tr.Load(stream, DataFormats.XamlPackage);

                var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();

                var hl = tr.GetAllTextElements()
                           .FirstOrDefault(x => x is Hyperlink) as Hyperlink;

                var fileItemParagraph = tr.GetAllTextElements().FirstOrDefault(x => x is Paragraph) as Paragraph;

                var ctvm = tr.Start.Parent.FindParentOfType<FlowDocument>().DataContext as MpClipTileViewModel;

                fileItemParagraph.TextAlignment = TextAlignment.Left;
                fileItemParagraph.Padding = new Thickness(3);

                RequestNavigateEventHandler hl_nav_handler = (s, e) => {
                    System.Diagnostics.Process.Start(e.Uri.ToString());
                };
                MouseButtonEventHandler hl_mouseLeftButtonUp_handler = (s, e) => {
                    Process.Start(hl.NavigateUri.ToString());
                };

                MouseEventHandler p_mouseEnter_handler = (s, e) => {
                    if (MpDragDropManager.IsDragAndDrop) {
                        return;
                    }
                    fileItemParagraph.Background = Brushes.Gainsboro;
                    fileItemParagraph.BorderBrush = Brushes.Black;
                    fileItemParagraph.BorderThickness = new Thickness(0.5);

                    ctvm.IsHovering = true;

                    hl.IsEnabled = true;

                    rtb.ToolTip = path;

                    var selectionRange = fileItemParagraph.ContentRange();
                    if (!rtb.Document.ContentRange().IsRangeInSameDocument(selectionRange)) {
                        Debugger.Break();
                        return;
                    }
                    rtb.Selection.Select(selectionRange.Start, selectionRange.End);
                    //MpConsole.WriteLine("Hover ItemData: " + civm.Parent.HoverItem.CopyItemData);
                };
                MouseEventHandler p_mouseLeave_handler = (s, e) => {
                    if (MpDragDropManager.IsDragAndDrop) {
                        return;
                    }
                    fileItemParagraph.Background = Brushes.Transparent;
                    fileItemParagraph.BorderBrush = Brushes.Transparent;
                    fileItemParagraph.BorderThickness = new Thickness(0);

                    ctvm.IsHovering = false;
                    rtb.ToolTip = null;
                    hl.IsEnabled = false;
                    var selectionRange = fileItemParagraph.ContentStart.ToTextRange();
                    if (!rtb.Document.ContentRange().IsRangeInSameDocument(selectionRange)) {
                        Debugger.Break();
                        return;
                    }
                    rtb.Selection.Select(selectionRange.Start, selectionRange.End);
                };

                PropertyChangedEventHandler ctvm_PropertyChanged_handler = (s, e) => {
                    if(e.PropertyName == nameof(ctvm.IsItemDragging)) {
                        if(ctvm.IsItemDragging) {
                            if(rtb.Selection.IsEmpty) {
                                var doc_paragraphs = rtb.Document.GetAllTextElements().Where(x => x is Paragraph).Cast<Paragraph>();
                                foreach (var dp in doc_paragraphs) {
                                    dp.Background = Brushes.Gainsboro;
                                    dp.BorderBrush = Brushes.Black;
                                    dp.BorderThickness = new Thickness(0.5);
                                    dp.ContentRange().GetAllTextElements().Where(x => x is Hyperlink)
                                    .ForEach(x => x.IsEnabled = true);
                                }
                            }
                            
                        } else {
                            if (rtb.Selection.IsEmpty) {
                                var doc_paragraphs = rtb.Document.GetAllTextElements().Where(x => x is Paragraph).Cast<Paragraph>();
                                foreach (var dp in doc_paragraphs) {
                                    dp.Background = Brushes.Transparent;
                                    dp.BorderBrush = Brushes.Transparent;
                                    dp.BorderThickness = new Thickness(0);
                                    dp.ContentRange().GetAllTextElements().Where(x => x is Hyperlink)
                                    .ForEach(x => x.IsEnabled = false);
                                }
                            }
                        }
                    }
                };


                RoutedEventHandler hl_Unload_handler = null;
                hl_Unload_handler = (s, e) => {
                    hl.RequestNavigate -= hl_nav_handler;
                    hl.Unloaded -= hl_Unload_handler;
                    hl.MouseLeftButtonUp -= hl_mouseLeftButtonUp_handler;
                };

                RoutedEventHandler p_Unload_handler = null;
                p_Unload_handler = (s, e) => {
                    fileItemParagraph.MouseEnter -= p_mouseEnter_handler;
                    fileItemParagraph.MouseLeave -= p_mouseLeave_handler;
                    fileItemParagraph.Unloaded -= p_Unload_handler;
                    if(ctvm != null) {
                        ctvm.PropertyChanged -= ctvm_PropertyChanged_handler;
                    }
                };

                hl.RequestNavigate += hl_nav_handler;
                hl.Unloaded += hl_Unload_handler;
                hl.MouseLeftButtonUp += hl_mouseLeftButtonUp_handler;
                fileItemParagraph.MouseEnter += p_mouseEnter_handler;
                fileItemParagraph.MouseLeave += p_mouseLeave_handler;
                fileItemParagraph.Unloaded += p_Unload_handler;
                ctvm.PropertyChanged += ctvm_PropertyChanged_handler;

                return fileItemParagraph;
            }

            //MpContentItemViewModel civm = null;
            //var fce = tr.Start.Parent as FrameworkContentElement;
            //if (fce.DataContext is MpClipTileViewModel ctvm) {
            //    civm = ctvm.Items.FirstOrDefault(x => x.CopyItemData == path);

            //    if (civm == null) {
            //        Debugger.Break();
            //    }
            //} else if (fce.DataContext is MpContentItemViewModel) {
            //    civm = (fce.DataContext as MpContentItemViewModel).Parent.Items.FirstOrDefault(x => x.CopyItemData == path);
            //} else {
            //    Debugger.Break();
            //}
            //var fileItemParagraph = new MpFileItemParagraph();
            //fileItemParagraph.DataContext = civm;



            //var pToReplace = tr.GetAllTextElements().FirstOrDefault(x => x is Paragraph) as Paragraph;
            //var fd = tr.Start.Parent.FindParentOfType<FlowDocument>();
            //if (pToReplace == null) {
            //    fd.Blocks.Add(fileItemParagraph);
            //} 
            //else {
            //    //fd.Blocks.InsertAfter(pToReplace, fileItemParagraph);
            //    //fd.Blocks.Remove(pToReplace);
            //    pToReplace = fileItemParagraph;
            //    pToReplace.DataContext = civm;
            //    return pToReplace;
            //    //using (var subStream = new MemoryStream()) {
            //    //    var subDoc = new FlowDocument();
            //    //    var tfip = new MpFileItemParagraph() { DataContext = civm };
            //    //    subDoc.Blocks.Add(tfip);
            //    //    tfip.ContentRange().Save(subStream, DataFormats.XamlPackage, true);
            //    //    subStream.Seek(0, SeekOrigin.Begin);
            //    //    tr.Load(subStream, DataFormats.XamlPackage);
            //    //}
            //}
            //return fileItemParagraph;
        }

        public static FlowDocument ToFilePathDocument(this string path, int iconId = 0, double iconSize = 16) {
            var fd = string.Empty.ToFlowDocument(); 
            fd.Blocks.FirstBlock.ContentRange().LoadFileItem(path, iconId, iconSize);
            return fd;
        }
        public static void LoadRtf(this TextRange tr, string str, out Size unformattedContentSize) {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                tr.Load(stream, DataFormats.Rtf);
                var fd = tr.Start.Parent.FindParentOfType<FlowDocument>();
                var ds = fd.GetDocumentSize();
                unformattedContentSize = ds;
                var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                    //ignore to tr
                } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                    tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                }                
            }
            //using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
            //    try {
            //        using (var subStream = new MemoryStream()) {
            //            var subDoc = new FlowDocument();
            //            var sub_tr = new TextRange(subDoc.ContentStart, subDoc.ContentEnd);
            //            sub_tr.Load(stream, DataFormats.Rtf);
            //            var ds = new Size(subDoc.PageWidth, subDoc.PageHeight);
            //            ds = subDoc.GetDocumentSize();
            //            unformattedContentSize = ds;

            //            sub_tr.Save(subStream, DataFormats.Rtf);
            //            subStream.Seek(0, SeekOrigin.Begin);
            //            tr.Load(subStream, DataFormats.Rtf);
            //        }
            //        var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
            //        if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
            //            //ignore to tr
            //        } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
            //            tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
            //        }
            //    }
            //    catch (Exception ex) {
            //        MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...", ex);
            //        unformattedContentSize = new Size();
            //        return;
            //    }
            //}
        }


        public static void LoadTextTemplate_FromCode(this TextRange tr, MpTextTemplate cit) {
            // TODO (maybe) when cit is new (and no formatting is stored) need to check font formatting of beginning of tr
            // and clone (and also switch to FLowDocument if a list or something) here
            // because as long as this done on initial creation the formatting will persist (or is loaded from cit)
            var tr_parent_te = tr.Start.Parent as TextElement;
            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();

            if (tr_parent_te == null) {
                tr_parent_te = tr.Start.Paragraph;
            }

            tr.Text = string.Empty;
            //var r = new Run(cit.TemplateName) {
            //    Tag = cit
            //};

            var tb = new TextBlock() {
                Text = cit.TemplateName,
                FontSize = tr_parent_te.FontSize,
                FontFamily = tr_parent_te.FontFamily,
                FontStretch = tr_parent_te.FontStretch,
                FontWeight = tr_parent_te.FontWeight,
                FontStyle = tr_parent_te.FontStyle,
                Background = Brushes.Transparent,
                Tag = cit
            };
            var b = new Border() {
                Child = tb,
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
                Tag = cit
            };

            var iuic = new InlineUIContainer(b, tr.Start) {
                Tag = cit,
                BaselineAlignment = BaselineAlignment.Center
            };

            MpHelpers.RunOnMainThread(async () => {
                while (!b.IsLoaded) {
                    await Task.Delay(100);
                }
                if (iuic.ContentStart.Paragraph != null) {
                    if (iuic.ContentStart.Paragraph.LineHeight < b.ActualHeight) {
                        iuic.ContentStart.Paragraph.LineHeight = b.ActualHeight;
                    }
                }
            });


            //var thl = new Hyperlink(tr.Start,tr.End) {
            //    Tag = cit,
            //    TextDecorations = null
            //}; 
            //thl.Inlines.Clear();
            //thl.Inlines.Add(iuic);

            MpClipTileViewModel ctvm = rtb.DataContext as MpClipTileViewModel;
            if (ctvm == null) {
                Debugger.Break();
            }

            MpTextTemplateViewModelBase tvm = ctvm.TemplateCollection.Items.FirstOrDefault(x => x.TextTemplateId == cit.Id);

            #region Events

            MouseEventHandler thl_mouseEnter_handler = (s, e) => {
                tvm.IsHovering = true;
                if (!ctvm.IsContentReadOnly) {
                    MpPlatformWrapper.Services.Cursor.SetCursor(tvm, MpCursorType.Hand);
                }
            };

            MouseEventHandler thl_mouseLeave_handler = (s, e) => {
                tvm.IsHovering = false;
                if (!ctvm.IsContentReadOnly) {
                    MpPlatformWrapper.Services.Cursor.UnsetCursor(tvm);
                }
            };

            MouseButtonEventHandler thl_previewMouseLeftButtonDown_handler = (s, e) => {
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
                if(ctvm.IsSubSelectionEnabled || !ctvm.IsContentReadOnly) {
                    MpDragDropManager.StartDragCheck(ctvm);
                }
            };

            MouseButtonEventHandler thl_previewMouseRightButtonDown_handler = (s, e) => {
                if (!tvm.Parent.Parent.IsSelected) {
                    tvm.Parent.Parent.IsSelected = true;
                }
                if (!ctvm.IsContentReadOnly) {
                    tvm.Parent.SelectedItem = tvm;
                    if (!ctvm.IsPasting) {
                        var origin = tr.Start.GetCharacterRect(LogicalDirection.Forward).Location;
                        origin = iuic.FindParentOfType<RichTextBox>().TranslatePoint(origin, Application.Current.MainWindow);

                        MpContextMenuView.Instance.DataContext = tvm.ContextMenuItemViewModel;
                        //MpContextMenuView.Instance.PlacementRectangle = new Rect(origin,new Size(200,50));

                        iuic.ContextMenu = MpContextMenuView.Instance;
                        MpContextMenuView.Instance.IsOpen = true;
                    }
                    e.Handled = true;
                }
            };


            RoutedEventHandler thl_unloaded_handler = null;
            thl_unloaded_handler = (s, e) => {
                MpHelpers.RunOnMainThread(async () => {
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

                iuic.Unloaded -= thl_unloaded_handler;
                iuic.MouseEnter -= thl_mouseEnter_handler;
                iuic.MouseLeave -= thl_mouseLeave_handler;
                iuic.PreviewMouseLeftButtonDown -= thl_previewMouseLeftButtonDown_handler;
                iuic.PreviewMouseRightButtonDown -= thl_previewMouseRightButtonDown_handler;
            };

            iuic.Unloaded += thl_unloaded_handler;
            iuic.MouseEnter += thl_mouseEnter_handler;
            iuic.MouseLeave += thl_mouseLeave_handler;
            iuic.PreviewMouseLeftButtonDown += thl_previewMouseLeftButtonDown_handler;
            iuic.PreviewMouseRightButtonDown += thl_previewMouseRightButtonDown_handler;
            #endregion

            #region Bindings

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateDisplayValue)),
                   target: tb,
                   targetProperty: TextBlock.TextProperty);

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateForegroundHexColor)),
                   target: tb,
                   targetProperty: TextBlock.ForegroundProperty,
                   converter: new MpStringHexToBrushConverter());

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateBackgroundHexColor)),
                   target: b,
                   targetProperty: Border.BackgroundProperty,
                   converter: new MpStringHexToBrushConverter());

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateBorderHexColor)),
                   target: b,
                   targetProperty: Border.BorderBrushProperty,
                   converter: new MpStringHexToBrushConverter());

            #endregion
        }

        public static FlowDocument LoadContent(object dc, string str, MpCopyItemType strItemDataType, out Size unformattedContentSize) {
            var fd = new FlowDocument() {
                DataContext = dc,
            };
            var tr = fd.ContentRange();

            unformattedContentSize = new Size();
            if (string.IsNullOrEmpty(str)) {
                tr.Text = str;
                // rtb.IsUndoEnabled = wasUndoEnabled;
                return fd;
            }
            switch (strItemDataType) {
                case MpCopyItemType.Text:
                    tr.LoadRtf(str, out unformattedContentSize);
                    break;
                case MpCopyItemType.Image:
                    //tr.LoadImage(str, out unformattedContentSize);
                    //fd.Blocks.Clear();
                    var ip = new MpImageParagraph() {
                        DataContext = fd.DataContext
                    };
                    fd.Blocks.Add(ip);
                    unformattedContentSize = ip.ContentImage.Source.PixelSize();
                    break;
                case MpCopyItemType.FileList:
                    //fd.Blocks.Clear();
                    var ctvm = fd.DataContext as MpClipTileViewModel;
                    var fpl = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < fpl.Length; i++) {
                        var fivm = new MpFileItemViewModel(ctvm);
                        fivm.Path = fpl[i];
                        ctvm.FileItems.Add(fivm);
                        var fip = new MpFileItemParagraph() {
                            DataContext = fivm
                        };
                        fd.Blocks.Add(fip);

                        double width = ((Path.GetFileName(fivm.Path).Length + 1) * 16) + 6 + 2 + 16;
                        unformattedContentSize.Height += (16 + 6 + 2);
                        unformattedContentSize.Width = Math.Max(unformattedContentSize.Width, width);
                    }
                    break;
            }
            return fd;
        }
        public static void LoadItemData(
            this TextRange tr, 
            string str, 
            MpCopyItemType strItemDataType, 
            out Size unformattedContentSize) {
            // NOTE iconId is only used to convert file path's to rtf w/ icon 

            var fd = tr.Start.Parent.FindParentOfType<FlowDocument>();
            //var rtb = fd.FindParentOfType<RichTextBox>();
            //bool wasUndoEnabled = rtb.IsUndoEnabled;
            //rtb.IsUndoEnabled = false;

            unformattedContentSize = new Size();
            if (string.IsNullOrEmpty(str)) {
                tr.Text = str;
               // rtb.IsUndoEnabled = wasUndoEnabled;
                return;
            }
            switch (strItemDataType) {
                case MpCopyItemType.Text:
                    tr.LoadRtf(str, out unformattedContentSize);
                    break;
                case MpCopyItemType.Image:
                    //tr.LoadImage(str, out unformattedContentSize);
                    //fd.Blocks.Clear();
                    var ip = new MpImageParagraph() {
                        DataContext = fd.DataContext
                    };
                    fd.Blocks.Add(ip);
                    unformattedContentSize = ip.ContentImage.Source.PixelSize();
                    break;
                case MpCopyItemType.FileList:
                    //fd.Blocks.Clear();
                    var ctvm = fd.DataContext as MpClipTileViewModel;
                    var fpl = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < fpl.Length; i++) {
                        var fivm = new MpFileItemViewModel(ctvm);
                        fivm.Path = fpl[i];
                        ctvm.FileItems.Add(fivm);
                        var fip = new MpFileItemParagraph() {
                            DataContext = fivm
                        };
                        fd.Blocks.Add(fip);

                        double width = ((Path.GetFileName(fivm.Path).Length + 1) * 16) + 6 + 2 + 16;
                        unformattedContentSize.Height += (16 + 6 + 2);
                        unformattedContentSize.Width = Math.Max(unformattedContentSize.Width, width);
                    }
                    break;
            }
            //rtb.IsUndoEnabled = wasUndoEnabled;
        }

        public static FlowDocument ToFlowDocument(this string str, int iconId = 0) {
            // NOTE iconId is only used to convert file path's to rtf w/ icon 

            if (string.IsNullOrEmpty(str)) {
                return string.Empty.ToContentRichText().ToFlowDocument();
            }
            if (str.IsStringRichText()) {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                    try {
                        var fd = new FlowDocument();
                        var range = new TextRange(fd.ContentStart, fd.ContentEnd);
                        range.Load(stream, System.Windows.DataFormats.Rtf);

                        var tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                        var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                        if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                            //ignore to r
                        } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                            tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                        }

                        var ps = fd.GetDocumentSize();
                        fd.PageWidth = ps.Width;
                        fd.PageHeight = ps.Height;
                        fd.ConfigureLineHeight();

                        return fd;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                        MpConsole.WriteLine("Exception Details: " + ex);
                        return str.ToPlainText().ToFlowDocument();
                    }
                }
            }
            if (str.IsStringBase64()) {
                var img_fd = str.ToImageDocument(out Size imgSize);
                img_fd.PageWidth = imgSize.Width;
                img_fd.PageHeight = imgSize.Height;
                return img_fd;
            }
            if (str.IsStringWindowsFileOrPathFormat()) {
                return str.ToFilePathDocument();
            }
            return str.ToContentRichText(iconId).ToFlowDocument();
        }


        public static string ToXamlPackage(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return string.Empty.ToContentRichText().ToXamlPackage();
            }
            if (str.IsStringHtmlText()) {
                return str.ToContentRichText().ToXamlPackage();
            }
            if (str.IsStringPlainText()) {
                return str.ToContentRichText().ToXamlPackage();
            }
            if (str.IsStringRichText()) {
                var assembly = Assembly.GetAssembly(typeof(System.Windows.FrameworkElement));
                var xamlRtfConverterType = assembly.GetType("System.Windows.Documents.XamlRtfConverter");
                var xamlRtfConverter = Activator.CreateInstance(xamlRtfConverterType, true);
                var convertRtfToXaml = xamlRtfConverterType.GetMethod("ConvertRtfToXaml", BindingFlags.Instance | BindingFlags.NonPublic);
                var xamlContent = (string)convertRtfToXaml.Invoke(xamlRtfConverter, new object[] { str });
                return xamlContent;
            }
            throw new Exception("ToXaml exception string must be plain or rich text. Its content is: " + str);
        }

        public static string ToXamlPackage(this FlowDocument fd) {

            return XamlWriter.Save(fd);
        }

        public static string ToXamlPackage(this TextRange tr) {
            using (var ms = new MemoryStream()) {
                tr.Save(ms, DataFormats.XamlPackage);
                return XamlWriter.Save(ms);
            }
        }

        public static string ToContentRichText(this string str, int iconId = 0) {
            // NOTE iconId is only used for converting file path's icons to rtf
            if (str == null) {
                str = string.Empty;
            }
            if (str.IsStringRichText() || str.IsStringRichTextTable()) {
                return str;
            }
            if (str.IsStringHtmlText()) {
                //return MpQuillHtmlToRtfConverter.ConvertQuillHtmlToRtf(str);
                string xaml = HtmlToXamlDemo.HtmlToXamlConverter.ConvertHtmlToXaml(str, true);
                return xaml.ToContentRichText(iconId);                
            }
            if (str.IsStringXaml()) {
                using (var stringReader = new StringReader(str)) {
                    var xmlReader = XmlReader.Create(stringReader);
                    //if (!IsStringFlowSection(xaml)) {
                    //    return (FlowDocument)XamlReader.Load(xmlReader);
                    //}
                    var doc = new FlowDocument();
                    var data = XamlReader.Load(xmlReader);
                    if (data.GetType() == typeof(Span)) {
                        Span span = (Span)data;
                        while (span.Inlines.Count > 0) {
                            //doc.Blocks.Add(sec.Blocks.FirstBlock);
                            var inline = span.Inlines.FirstInline;
                            span.Inlines.Remove(inline);
                            doc.Blocks.Add(new Paragraph(inline));
                        }
                    } else if (data.GetType() == typeof(Section)) {
                        Section sec = (Section)data;
                        while (sec.Blocks.Count > 0) {
                            //doc.Blocks.Add(sec.Blocks.FirstBlock);
                            var block = sec.Blocks.FirstBlock;
                            sec.Blocks.Remove(block);
                            doc.Blocks.Add(block);
                        }
                    } else {
                        doc = (FlowDocument)data;
                    }

                    // alternative:
                    /*
                        var richTextBox = new System.Windows.Controls.RichTextBox();
                        if (string.IsNullOrEmpty(xaml)) {
                            return string.Empty;
                        }

                        var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

                        using (var xamlMemoryStream = new MemoryStream()) {
                            using (var xamlStreamWriter = new StreamWriter(xamlMemoryStream)) {
                                xamlStreamWriter.Write(xaml);
                                xamlStreamWriter.Flush();
                                xamlMemoryStream.Seek(0, SeekOrigin.Begin);

                                textRange.Load(xamlMemoryStream, DataFormats.Xaml);
                            }
                        }

                        using (var rtfMemoryStream = new MemoryStream()) {
                            textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                            textRange.Save(rtfMemoryStream, DataFormats.Rtf);
                            rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                            using (var rtfStreamReader = new StreamReader(rtfMemoryStream)) {
                                return rtfStreamReader.ReadToEnd();
                            }
                        }

                    */

                    return doc.ToRichText();
                }
            }
            if (str.IsStringBase64()) {
                return str.ToImageDocument(out Size imgSize).ToRichText();
            }
            if (str.IsStringWindowsFileOrPathFormat()) {
                return str.ToFilePathDocument(iconId).ToRichText();
            }
            if (str.IsStringPlainText()) {
                using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                    rtb.Text = str;
                    rtb.Font = new System.Drawing.Font(MpPrefViewModel.Instance.DefaultFontFamily, (float)MpPrefViewModel.Instance.DefaultFontSize);
                    return rtb.Rtf;
                }
            }

            return string.Empty;
        }


        public static FlowDocument Combine(this FlowDocument fd, FlowDocument ofd, TextPointer insertPointer = null, bool insertNewline = true) {
            return CombineFlowDocuments(ofd, fd, insertPointer, insertNewline);
        }

        public static FlowDocument Combine(this FlowDocument fd, string text, TextPointer insertPointer = null, bool insertNewline = true) {
            return CombineFlowDocuments(text.ToContentRichText().ToFlowDocument(), fd, insertPointer, insertNewline);
        }

        public static FlowDocument CombineFlowDocuments(
            FlowDocument from,
            FlowDocument to,
            TextPointer toInsertPointer = null, bool insertNewLine = false) {
            toInsertPointer = toInsertPointer == null ? to.ContentEnd : toInsertPointer;
            if(!insertNewLine) {
                if(to.Blocks.LastBlock is Paragraph lp) {
                    if(lp.Inlines.LastInline == null) {
                        to.Blocks.Remove(lp);
                        if(to.Blocks.LastBlock is Paragraph) {
                            lp = to.Blocks.LastBlock as Paragraph;
                        }                        
                    }
                    if(lp != null) {
                        toInsertPointer = lp.Inlines.LastInline.ContentEnd;
                    }
                    
                }
            }

            using (MemoryStream stream = new MemoryStream()) {
                var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);

                XamlWriter.Save(rangeFrom, stream);
                rangeFrom.Save(stream, DataFormats.XamlPackage);

                
                //if (insertNewLine) {
                //    var lb = new LineBreak();
                //    var p = (Paragraph)to.Blocks.LastBlock;
                //    p.LineHeight = 1;
                //    p.Inlines.Add(lb);
                //} 

                var rangeTo = new TextRange(toInsertPointer, toInsertPointer);
                rangeTo.Load(stream, DataFormats.XamlPackage);

                var tr = new TextRange(to.ContentStart, to.ContentEnd);
                var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                if (rtbAlignment == null ||
                    rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}" ||
                    (TextAlignment)rtbAlignment == TextAlignment.Justify) {
                    tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                }

                var ps = to.GetDocumentSize();
                to.PageWidth = ps.Width;
                to.PageHeight = ps.Height;

                
                return to;
            }
        }





        //public static FlowDocument TokenizeMatches(this FlowDocument fd, string matchValue, Uri uri, bool isCaseSensitive = false) {
        //    var trl = MpHelpers.FindStringRangesFromPosition(fd.ContentStart, matchValue, isCaseSensitive);

        //}

        //public static Hyperlink ToHyperlink(this TextRange tr, Uri uri) {
        //    var hl = new Hyperlink(tr.Start, tr.End);
        //    hl.NavigateUri = uri;
        //    hl.IsEnabled = true;

        //}

        

        public static string ToEncodedPlainText(this TextRange tr) {
            if(tr.IsEmpty) {
                return string.Empty;
            }
            var templatesToEncode = tr.GetAllTextElements()
                                                .Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate)
                                                .OrderBy(x => tr.Start.GetOffsetToPosition(x.ContentStart))
                                                .ToList();
            if(templatesToEncode.Count() == 0) {
                return tr.Text;
            }

            var sb = new StringBuilder();
            var ctp = tr.Start;
            foreach (var te in templatesToEncode) {
                var ntp = te.ElementStart;
                sb.Append(new TextRange(ctp, ntp).Text);
                sb.Append((te.Tag as MpTextTemplate).EncodedTemplate);
                ctp = te.ElementEnd;
                if (te == templatesToEncode.Last()) {
                    sb.Append(new TextRange(ctp, tr.End).Text);
                }
            }
            return sb.ToString();
        }

        public static string ToEncodedRichText(this TextRange tr) {
            if (tr.IsEmpty) {
                return string.Empty;
            }
            var templatesToEncode = tr.GetAllTextElements()
                                                .Where(x => x is MpTextTemplateInlineUIContainer && x.Tag is MpTextTemplate)
                                                .OrderBy(x => tr.Start.GetOffsetToPosition(x.ContentStart))
                                                .Cast<MpTextTemplateInlineUIContainer>()
                                                .ToList();
            var doc = tr.Start.Parent.FindParentOfType<FlowDocument>();
            var clonedDoc = doc.Clone(tr, out TextRange encodedRange);

            return encodedRange.ToRichText();
        }

        public static string ToRichTextTable(this string csvStr) {
            if (string.IsNullOrEmpty(csvStr) || !csvStr.IsStringCsv()) {
                return csvStr;
            }
            //return new MpCsvReader(csvStr).FlowDocument.ToRichText();
            return MpCsvToRtfTableConverter.GetFlowDocument(csvStr).ToRichText();
        }

        public static string ToQuillText(this string text) {
            if (text.IsStringHtmlText()) {
                return text;
            }
            return MpRtfToHtmlConverter.ConvertRtfToHtml(text.ToContentRichText());
        }

        
    }
}
