using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileContentControlController : MpControlController {
        public Control TileContentControl { get; set; }
        private Point _offset = Point.Empty;
        public Point Offset {
            get {
                return _offset;
            }
            set {
                if(_offset != value) {
                    _offset = value;
                    Update();
                }
            }
        }
        public MpTileContentControlController(MpCopyItem ci,MpControlController parent) : base(parent) {
            switch (ci.CopyItemType) {
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                case MpCopyItemType.Text:
                    TileContentControl = new RichTextBox() {
                        WordWrap = false,
                        Multiline = true,
                        Text = ((string)ci.GetData()).Replace("''", "'"),
                        ScrollBars = RichTextBoxScrollBars.None,
                        BackColor = Properties.Settings.Default.TileItemBgColor,
                        ForeColor = Color.Black,
                        AutoSize = false,
                        ReadOnly = true,                   
                        BorderStyle = BorderStyle.None
                    };                    
                    break;
                case MpCopyItemType.FileList:
                    TileContentControl = new ScrollableControl() {
                        AutoScroll = false,
                        AutoSize = false,
                    };
                    if (ci.GetData().GetType() == typeof(string[])) {
                        int maxWidth = int.MinValue;
                        foreach (string fileOrPathStr in (string[])ci.GetData()) {
                            MpFileListItemRowPanelController newFlirp = new MpFileListItemRowPanelController(TileContentControl.Controls.Count, fileOrPathStr, this);
                            Point p = newFlirp.GetFileListItemRowPanel().Location;
                            p.Y = newFlirp.GetFileListItemRowPanel().Bottom * TileContentControl.Controls.Count;
                            newFlirp.GetFileListItemRowPanel().Location = p;
                            TileContentControl.Controls.Add(newFlirp.GetFileListItemRowPanel());

                            if (newFlirp.GetFileListItemRowPanel().Width > maxWidth) {
                                maxWidth = newFlirp.GetFileListItemRowPanel().Width;
                            }
                        }
                    }
                    break;
                case MpCopyItemType.Image:
                    TileContentControl = new PictureBox() {
                        Image = ci.GetData().GetType() == typeof(Bitmap) ? (Image)ci.GetData() : MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData()),
                        BorderStyle = BorderStyle.None,
                        AutoSize = false,
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    break;
            }
            TileContentControl.DoubleBuffered(true);
            TileContentControl.Bounds = GetBounds();
            int pad = 0;// (int)(((MpScrollPanelController)Parent).GetBounds().Height * Properties.Settings.Default.TileItemScrollBarThicknessRatio);
            SetPadding(new Padding(pad));
            TileContentControl.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

        }
        public override Rectangle GetBounds() {
            //scroll panel rect
            Rectangle spr = ((MpScrollPanelController)Parent).GetBounds();
            Size contentSize = Size.Empty;

            if (TileContentControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
               // ((RichTextBox)TileContentControl).Text = (string)((MpTilePanelController)Parent).CopyItem.GetData();
                float fontSize = spr.Width * Properties.Settings.Default.TileFontSizeRatio;
                fontSize = fontSize < 1.0f ? 10.0f : fontSize;
                Font f = new Font(Properties.Settings.Default.TileFont, fontSize, GraphicsUnit.Pixel);
                ((TextBoxBase)TileContentControl).Font = f;
                contentSize = TextRenderer.MeasureText(((TextBoxBase)TileContentControl).Text, f);
            }
            else if (TileContentControl.GetType().IsSubclassOf(typeof(PictureBox))) {
                contentSize = ((PictureBox)TileContentControl).Image.Size;
            }
            else {
                // TODO Add sizing for file list
            }
            //update so items minimum is as big as scrollable panel
            contentSize.Width = contentSize.Width > spr.Size.Width ? contentSize.Width : spr.Size.Width;
            contentSize.Height = contentSize.Height > spr.Size.Height ? contentSize.Height : spr.Size.Height;

            //adjust scrollbar offset to proportions of content to scrollpanel
            float xratio = (float)contentSize.Width / (float)((MpScrollPanelController)Parent).GetBounds().Width;
            float yratio = (float)contentSize.Height / (float)((MpScrollPanelController)Parent).GetBounds().Height;
            int x = (int)((float)Offset.X * xratio);
            int y = (int)((float)Offset.Y * yratio);
            return new Rectangle(x,y, contentSize.Width,contentSize.Height);
        }
        public override void Update() {
            TileContentControl.Bounds = GetBounds();
            int pad = 0;// (int)(((MpScrollPanelController)Parent).GetBounds().Height * Properties.Settings.Default.TileItemScrollBarThicknessRatio);
            SetPadding(new Padding(pad));

            TileContentControl.Invalidate();
        }
        private void SetPadding(Padding padding) {
            if(TileContentControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                TextBoxBase textBox = (TextBoxBase)TileContentControl;
                var rect = new Rectangle(padding.Left, padding.Top, textBox.Size.Width - padding.Left - padding.Right, textBox.Size.Height - padding.Top - padding.Bottom);
                RECT rc = new RECT(rect);
                WinApi.SendMessageRefRect(textBox.Handle, WinApi.EM_SETRECT, 0, ref rc);
            }
        }
        public void UpdateItem(MpCopyItem ci) {
            if (TileContentControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                ((TextBoxBase)TileContentControl).Text = (string)ci.GetData();
                Update();
            }
        }
    }
}
