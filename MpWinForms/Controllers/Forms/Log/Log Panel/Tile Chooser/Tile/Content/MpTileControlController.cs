using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileControlController : MpController {
        public Control ItemControl { get; set; }
        public Font ItemFont { get; set; }
        public Panel ItemControlPanel { get; set; }
        public Panel ItemPanel { get; set; }
        private Point _lastMouseLoc = Point.Empty;
        private Point _itemControlPanelOrigin = Point.Empty;

        public MpTileControlController(MpCopyItem ci,MpController Parent) : base(Parent) {
            switch(ci.CopyItemType) {
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                case MpCopyItemType.Text:
                    ItemControl = new RichTextBox() {
                        WordWrap = false,
                        Multiline = true,
                        Text = ((string)ci.GetData()).Replace("''","'"),
                        ScrollBars = RichTextBoxScrollBars.Both,
                        BackColor = Properties.Settings.Default.TileItemBgColor,
                        AutoSize = false,                        
                        ReadOnly = true,
                       // Cursor = Cursors.Arrow,                        
                        BorderStyle = BorderStyle.None
                    };
                    MpHelperSingleton.Instance.SetPadding((TextBoxBase)ItemControl,new Padding(Properties.Settings.Default.TileItemPadding));
                    break;
                case MpCopyItemType.FileList:
                    ItemControl = new ScrollableControl() {
                        AutoScroll = true,
                        AutoSize = false,
                    };
                    if(ci.GetData().GetType() == typeof(string[])) {
                        int maxWidth = int.MinValue;
                        foreach(string fileOrPathStr in (string[])ci.GetData()) {
                            MpFileListItemRowPanelController newFlirp = new MpFileListItemRowPanelController(ItemControl.Controls.Count,fileOrPathStr,this);
                            Point p = newFlirp.GetFileListItemRowPanel().Location;
                            p.Y = newFlirp.GetFileListItemRowPanel().Bottom * ItemControl.Controls.Count;
                            newFlirp.GetFileListItemRowPanel().Location = p;
                            ItemControl.Controls.Add(newFlirp.GetFileListItemRowPanel());

                            if(newFlirp.GetFileListItemRowPanel().Width > maxWidth) {
                                maxWidth = newFlirp.GetFileListItemRowPanel().Width;
                            }
                        }
                    } 
                    break;
                case MpCopyItemType.Image:
                    ItemControl = new PictureBox() {                        
                        Image = ci.GetData().GetType() == typeof(Bitmap) ? (Image)ci.GetData():MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData()),   
                        BorderStyle = BorderStyle.None,                       
                        AutoSize = false,                  
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    break;
            }
            ItemControl.DoubleBuffered(true);
            ItemControlPanel = new Panel() {
                BorderStyle = BorderStyle.None,
                BackColor = Properties.Settings.Default.TileItemBgColor,
                //AutoScroll = true,
                AutoSize = false
            };
            ItemControlPanel.DoubleBuffered(true);

            ItemControlPanel.Controls.Add(ItemControl);

            ItemPanel = new Panel() {
                BorderStyle = BorderStyle.None,
                //AutoScroll = true,
                BackColor = Properties.Settings.Default.TileItemBgColor,
                AutoSize = false
            };
            ItemPanel.DoubleBuffered(true);
            ItemPanel.Controls.Add(ItemControlPanel);

            ItemControl.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            //Link(new List<MpIView> { (MpIView)ItemControl});
        }
           public override void Update() {
            //tile panel
            var tp = ((MpTilePanelController)Find("MpTilePanelController")).TilePanel;
            //tile panel controller
            var tpc = ((MpTilePanelController)Parent);

            //get tile rect
            Rectangle tr = tpc.TilePanel.Bounds;
            //itemcontrol padding
            //int tpd = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width) + tp.EdgeWidth;
            int tpd = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            //get tile details rect
            Rectangle tdr = tpc.TileDetailsPanelController.TileDetailsPanel.Bounds;
            //get tile title rect
            Rectangle ttr = tpc.TileTitlePanelController.TileTitlePanel.Bounds;
             //get tile header rect
            Rectangle thr = tpc.TileHeaderPanelController.TileHeaderPanel.Bounds;
            //control padding
            int p = 10;
            //control size
            int w = ttr.Width - (p*2);
            int h = tr.Height - (p*2);
            //ItemPanel.SetBounds(tpd*3,tpd+ttr.Bottom,tr.Width - (tpd*6) - tp.ShadowShift,tr.Height-tdr.Height-ttr.Height-(tpd*4));
            ItemPanel.SetBounds(p, ttr.Y + ttr.Height+ p, w, h);

            if(ItemControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                ((RichTextBox)ItemControl).Text = (string)tpc.CopyItem.GetData();
                float fontSize = ItemPanel.Height * Properties.Settings.Default.TileFontSizeRatio;
                fontSize = fontSize < 1.0f ? 10.0f : fontSize;
                ItemFont = new Font(Properties.Settings.Default.TileFont,fontSize,GraphicsUnit.Pixel);
                ((TextBoxBase)ItemControl).Font = ItemFont;
                ItemControl.Location = Point.Empty;
                Size textSize = TextRenderer.MeasureText(((TextBoxBase)ItemControl).Text,ItemFont);
                if(textSize.Width < ItemPanel.Width) {
                    textSize.Width = ItemPanel.Width;
                }
                if(textSize.Height < ItemPanel.Height) {
                    textSize.Height = ItemPanel.Height;
                }
                ItemControl.Size = textSize;
                ItemControlPanel.Size = ItemPanel.Size;

                MpHelperSingleton.Instance.SetPadding((TextBoxBase)ItemControl,new Padding(Properties.Settings.Default.TileItemPadding));
            } else {
                ItemFont = null;
                ItemControl.Bounds = ItemPanel.Bounds;
                ItemControlPanel.Bounds = ItemPanel.Bounds;
                ItemControl.Location = Point.Empty;
                ItemControlPanel.Location = Point.Empty;
            }
            ItemPanel.SendToBack();

            ItemPanel.Invalidate(); 
            ItemControlPanel.Invalidate();
            ItemControl.Invalidate();  
        }
        public void TraverseItem(Point ml) {
            return;
            //if(!((MpTilePanelController)Parent).IsSelected()) {
            //    return;
            //}
            if(ItemControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                //item control size
                Size ics = MpHelperSingleton.Instance.GetTextSize(((TextBoxBase)ItemControl).Text,ItemFont);
                //item panel size
                Size ips = ItemPanel.Size;
                Point p = ItemControlPanel.Location;
                if(ics.Width > ips.Width) {
                    p.X = ItemControlPanel.Location.X - (int)((((float)ml.X / (float)ips.Width) * (float)(ics.Width-ips.Width)));
                }
                if(ics.Height > ips.Height) {
                    p.Y = ItemControlPanel.Location.Y - (int)((((float)ml.Y / (float)ips.Height) * (float)(ics.Height-ips.Height)));
                }
                //constrain scrolling for left justified text
                p = new Point(Math.Min(-ItemPanel.Width,p.X),Math.Min(-ItemPanel.Height,p.Y));

                ItemControlPanel.Location = p;
                ItemControlPanel.Refresh();
                //Console.WriteLine("Traversing item: "+ControllerName+" P:"+p.ToString()+" Item Dimensions:"+ics.ToString());
            }
            if(_itemControlPanelOrigin == Point.Empty) {
                _itemControlPanelOrigin = ItemControlPanel.Location;
            }
        }       
    }
}
