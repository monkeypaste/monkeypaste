using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MonkeyPaste_WindowsControlLibrary {
    public partial class MpTilePanel : UserControl {
        public MpTilePanel() {
            Size = new Size(512, 512);
            // Paint += this.MpCopyItemCard_Paint;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);            

            InitializeComponent();
        }

        #region Components
        private Panel _basePanel;
        private MpAdvancedLabel _clipTitleLabel;
        private Label _createdDateTimeLabel;
        private PictureBox _downDotIconPictureBox;
        private PictureBox _sourceAppIconPictureBox;
        private MpPictureFramePanel _sourceAppIconFramePanel;
        private Control _contentControl;
        private Panel _scrollableContainerPanel;
        private Panel _hScrollbarPanel;
        private Panel _vScrollbarPanel;
        #endregion

        #region Designer Properties
        [Browsable(true), Category("Tile Panel")]
        public DateTime CopyItemCreatedDateTime {
            get {
                return _copyItemCreatedDateTime;
            }
            set {
                _copyItemCreatedDateTime = value;
                Invalidate();
            }
        }
        private DateTime _copyItemCreatedDateTime = DateTime.Now;

        [Browsable(true), Category("Tile Panel")]
        public object CopyItemData {
            get {
                return _copyItemData;
            }
            set {
                _copyItemData = value;
                Invalidate();
            }
        }
        private object _copyItemData = (object)"Visual Studio is a dots per inch (DPI) aware application, which means the display scales automatically. If an application states that it's not DPI-aware, the operating system scales the application as a bitmap. This behavior is also called DPI virtualization. The application still thinks that it's running at 100% scaling, or 96 dpi. This article discusses the limitations of Windows Forms Designer on HDPI monitors and how to run Visual Studio as a DPI-unaware process.Windows Forms Designer on HDPI monitors. The Windows Forms Designer in Visual Studio doesn't have scaling support. This causes display issues when you open some forms in the Windows Forms Designer on high dots per inch (HDPI) monitors. For examples, controls can appear to overlap as shown in the following image:";

        [Browsable(true), Category("Tile Panel")]
        public Color TitleColor {
            get {
                return _titleColor;
            }
            set {
                _titleColor = value;
                Invalidate();
            }
        }
        private Color _titleColor = Color.White;

        [Browsable(true), Category("Tile Panel")]
        public Color TitleShadowColor {
            get {
                return _titleShadowColor;
            }
            set {
                _titleShadowColor = value;
                Invalidate();
            }
        }
        private Color _titleShadowColor = Color.DimGray;
        [Browsable(true), Category("Tile Panel")]
        public Size TitleShadowOffset {
            get {
                return _titleShadowOffset;
            }
            set {
                _titleShadowOffset = value;
                Invalidate();
            }
        }
        private Size _titleShadowOffset = new Size(3, 3);

        [Browsable(true), Category("Tile Panel")]
        public Font TitleFont {
            get {
                return _titleFont;
            }
            set {
                _titleFont = value;
                Invalidate();
            }
        }
        private Font _titleFont = new Font("Tahoma", 35.0f, GraphicsUnit.Pixel);

        [Browsable(true), Category("Tile Panel")]
        public Rectangle TitleRect {
            get {
                return _titleRect;
            }
            set {
                _titleRect = value;
                Invalidate();
            }
        }
        private Rectangle _titleRect = new Rectangle(10, 10, 200, 55);

        [Browsable(true), Category("Tile Panel")]
        public string Title {
            get {
                return _title;
            }
            set {
                _title = value;
                Invalidate();
            }
        }
        private string _title = "Test Title";// string.Empty;

        [Browsable(true), Category("Tile Panel")]
        public Rectangle AppIconRect {
            get {
                return _appIconRect;
            }
            set {
                _appIconRect = value;
                Invalidate();
            }
        }
        private Rectangle _appIconRect = new Rectangle(10, 10, 200, 55);

        [Browsable(true), Category("Tile Panel")]
        public Image AppIcon {
            get {
                return _appIcon;
            }
            set {
                _appIcon = value;

                Invalidate();
            }
        }
        private Image _appIcon = Image.FromFile(@"C:\Users\tkefauver\Desktop\Dev\MonkeyPaste\artwork\Icon Projects\star_filled.png", true);
        
        [Browsable(true),Category("Tile Panel")]
        public ShadowMode ShadowStyle {
            get {
                return _shadowStyle;
            }
            set {
                _shadowStyle = value;
                Invalidate();
            }
        }
        private ShadowMode _shadowStyle = ShadowMode.ForwardDiagonal;

        [Browsable(true), Category("Tile Panel")]
        public int ShadowShift {
            get { return _shadowShift; }
            set {
                _shadowShift = value;
                Invalidate();
            }
        }
        private int _shadowShift = 5;

        [Browsable(true), Category("Tile Panel")]
        public Color ShadowColor {
            get { return _shadowColor; }
            set {
                _shadowColor = value;
                Invalidate();
            }
        }
        private Color _shadowColor = Color.DimGray;

        [Browsable(true), Category("Tile Panel")]
        public int EdgeWidth {
            get {
                return _edgeWidth;
            }
            set {
                _edgeWidth = value;
                Invalidate();
            }
        }
        private int _edgeWidth = 2;

        [Browsable(true), Category("Tile Panel")]
        public BevelStyle Style {
            get {
                return _style;
            }
            set {
                _style = value;
                Invalidate();
            }
        }
        private BevelStyle _style = BevelStyle.Flat;

        [Browsable(true), Category("Tile Panel")]
        public Color GradientColor1 {
            get { return _gradientColor1; }
            set {
                _gradientColor1 = value;
                Invalidate();
            }
        }
        private Color _gradientColor1 = Color.FromArgb(232, 238, 249);
        
        [Browsable(true), Category("Tile Panel")]
        public Color GradientColor2 {
            get { return _gradientColor2; }
            set {
                _gradientColor2 = value;
                Invalidate();
            }
        }
        private Color _gradientColor2 = Color.FromArgb(168, 192, 234);

        [Browsable(true), Category("Tile Panel")]
        public PanelGradientMode GradientMode {
            get { return _gradientMode; }
            set {
                _gradientMode = value;
                Invalidate();
            }
        }
        private PanelGradientMode _gradientMode = PanelGradientMode.Vertical;

        [Browsable(true),Category("Tile Panel")]
        public Color FlatBorderColor {
            get { return _borderColor; }
            set {
                _borderColor = value;
                Invalidate();
            }
        }
        private Color _borderColor = Color.FromArgb(102, 102, 102);

        [Browsable(true), Category("Tile Panel")]
        public int CornerRadius {
            get { return _cornerRadius; }
            set {
                _cornerRadius = value;
                Invalidate();
            }
        }
        private int _cornerRadius = 15;

        #endregion

        #region Rendering

        private Color mainColor;
        private const int sh = 10;
        private Color edgeColor1;
        private Color edgeColor2;

        protected virtual void RenderControl(Graphics g, ButtonState buttonState, CheckState checkState) {
            DrawShadow(g);
            var panelRect = new Rectangle();
            if (_shadowShift > 0) {
                DrawShadow(g);
            }
            switch (_shadowStyle) {
                case ShadowMode.ForwardDiagonal:
                    panelRect = new Rectangle(
                        0,
                        0,
                        Width - _shadowShift - 1,
                        Height - _shadowShift - 1);

                    break;
                case ShadowMode.Surrounded:
                    panelRect = new Rectangle(ShadowShift,
                    _shadowShift + _edgeWidth,
                    Width - (2 * ShadowShift) - 1,
                    Height - (2 * ShadowShift) - 1);
                    break;
                case ShadowMode.Dropped:
                    panelRect = new Rectangle(0,
                    0,
                    Width - 1,
                    Height - (2 * ShadowShift) - 1);
                    break;
            }
            DrawRect(g, panelRect);
            DrawDropShadowedText(g, TitleRect, Title, TitleFont,Color.Transparent,TitleColor,TitleShadowColor,TitleShadowOffset);
            DrawAppIcon(g, AppIcon, AppIconRect);            
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            RenderControl(e.Graphics, _buttonState, _checkState);
        }
        #region Rect rendering
        private void DrawRectLowered(Graphics graphics, Rectangle rect) {
            var darknessEnd = _gradientColor2.GetSaturation();
            var darknessBegin = _gradientColor1.GetSaturation();
            mainColor = darknessEnd <= darknessBegin ? _gradientColor2 : _gradientColor1;

            edgeColor1 = ControlPaint.Dark(mainColor);
            edgeColor2 = ControlPaint.Light(mainColor);

            DrawEdges(graphics, ref rect);
            rect.Inflate(-_edgeWidth, -_edgeWidth);
            DrawPanelStyled(graphics, rect);
        }
        private void DrawRect(Graphics graphics, Rectangle rect) {
            // Border rectangle
            using (Brush backgroundGradientBrush = new SolidBrush(_borderColor)) {
                MpRoundedRectangle.DrawFilledRoundedRectangle(graphics, backgroundGradientBrush, rect, _cornerRadius);
            }

            rect.Inflate(-_edgeWidth, -_edgeWidth);

            // Panel main rectangle
            using (Brush backgroundGradientBrush = new LinearGradientBrush(
                rect, _gradientColor1, _gradientColor2, (LinearGradientMode)this.GradientMode)) {
                MpRoundedRectangle.DrawFilledRoundedRectangle(graphics, backgroundGradientBrush, rect, _cornerRadius);
            }
        }
        private void DrawRectRaised(Graphics graphics, Rectangle rect) {
            var darknessEnd = _gradientColor2.GetSaturation();
            var darknessBegin = _gradientColor1.GetSaturation();
            mainColor = darknessEnd >= darknessBegin ? _gradientColor2 : _gradientColor1;

            edgeColor1 = ControlPaint.Light(_gradientColor1);
            edgeColor2 = ControlPaint.Dark(_gradientColor2);

            DrawEdges(graphics, ref rect);
            rect.Inflate(-_edgeWidth, -_edgeWidth);
            DrawPanelStyled(graphics, rect);

        }
        protected virtual void DrawEdges(Graphics g, ref Rectangle edgeRect) {

            Rectangle lgbRect = edgeRect;
            lgbRect.Inflate(1, 1);

            // Blend colors 
            var edgeBlend = new Blend();
            if (CornerRadius >= 150) {
                edgeBlend.Positions = new float[] { 0.0f, .2f, .4f, .6f, .8f, 1.0f };
                edgeBlend.Factors = new float[] { .0f, .0f, .2f, .4f, 1f, 1f };
            }
            else {
                switch (Style) {
                    case BevelStyle.Lowered:
                        edgeBlend.Positions = new float[] { 0.0f, .49f, .52f, 1.0f };
                        edgeBlend.Factors = new float[] { .0f, .6f, .99f, 1f };


                        break;
                    case BevelStyle.Raised:
                        edgeBlend.Positions = new float[] { 0.0f, .45f, .51f, 1.0f };
                        edgeBlend.Factors = new float[] { .0f, .0f, .2f, 1f };
                        break;
                }
            }


            using (var edgeBrush = new LinearGradientBrush(lgbRect,
                                                edgeColor1,
                                                edgeColor2,
                                                LinearGradientMode.ForwardDiagonal)) {
                edgeBrush.Blend = edgeBlend;
                MpRoundedRectangle.DrawFilledRoundedRectangle(g, edgeBrush, edgeRect, _cornerRadius);
            }
        }
        protected virtual void DrawPanelStyled(Graphics g, Rectangle rect) {
            using (Brush pgb = new LinearGradientBrush(rect, _gradientColor1, _gradientColor2,
                (LinearGradientMode)this.GradientMode)) {
                MpRoundedRectangle.DrawFilledRoundedRectangle(g, pgb, rect, _cornerRadius);
            }

        }
        private void DrawShadow(Graphics graphics) {
            GraphicsPath path;
            Rectangle rect = new Rectangle();
            switch (_shadowStyle) {
                case ShadowMode.ForwardDiagonal:
                    rect = new Rectangle(ShadowShift + sh, ShadowShift + sh,
                                    Width - ShadowShift - sh, Height - ShadowShift - sh);
                    break;
                case ShadowMode.Surrounded:
                    rect = new Rectangle(0, 0, Width, Height);
                    break;
                case ShadowMode.Dropped:
                    rect = new Rectangle(_shadowShift, 0, Width - 2 * _shadowShift, Height);
                    break;
            }

            if (_shadowStyle != ShadowMode.Dropped) {
                path = MpRoundedRectangle.DrawRoundedRectanglePath(rect, _cornerRadius);
            }
            else {
                path = MpRoundedRectangle.DrawRoundedRectanglePath(rect, _cornerRadius, true);
            }

            using (PathGradientBrush shadowBrush = new PathGradientBrush(path)) {
                shadowBrush.CenterPoint = new PointF(rect.Width / 2,
                    rect.Height / 2);

                // Set the color along the entire boundary 
                Color[] color = { Color.Transparent };
                shadowBrush.SurroundColors = color;

                // Set the center color 
                shadowBrush.CenterColor = _shadowColor;
                graphics.FillPath(shadowBrush, path);

                shadowBrush.FocusScales = new PointF(0.95f, 0.85f);
                graphics.FillPath(shadowBrush, path);

            }

        }
        #endregion

        #region Label rendering
        private void DrawDropShadowedText(Graphics g,Rectangle t,string text,Font font,Color backColor,Color foreColor,Color shadowColor,Size shadowOffset) {
            //title text origin
            PointF tto = new PointF((float)t.Location.X, (float)t.Location.Y);
            //title shadow origin
            PointF tso = new PointF((float)t.Location.X+(float)shadowOffset.Width, (float)t.Location.Y+(float)shadowOffset.Height);
            //shadow is slightly larg3er than foreground so make a new font for it
            //Font f2 = new Font(font.FontFamily.ToString(), font.Size * 1.5f, GraphicsUnit.Pixel);
            //title shadow origin
            //PointF tso = new PointF(tto.X + (f2.Size - font.Size), tto.Y);
            t.Inflate(shadowOffset);
            g.FillRectangle(new SolidBrush(backColor), t);
            g.DrawString(text, font, new SolidBrush(shadowColor), tso);// new PointF(tso.X+(float)shadowOffset.Width, tso.Y+(float)shadowOffset.Height));
            g.DrawString(text, font, new SolidBrush(foreColor), tto);// tto);
        }
        private void DrawAppIcon(Graphics g, Image img, Rectangle r) {
            g.DrawImage(img, r);
        }
        #endregion
        #endregion

        #region Events
        private ButtonState _buttonState = ButtonState.Flat;
        private CheckState _checkState = CheckState.Unchecked;

        // indicates whether the mouse is hovering over the control protected
        protected bool mouseOver = false;
        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e); _buttonState = ButtonState.Normal; mouseOver = true; Invalidate(true);
        }
        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e); _buttonState = ButtonState.Flat; mouseOver = false; Invalidate(true);
        }
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            this.Focus(); if (!(e.Button == MouseButtons.Left)) return; _buttonState = ButtonState.Pushed; switch (_checkState) {
                case CheckState.Checked: _checkState = CheckState.Unchecked; break;
                case CheckState.Unchecked: _checkState = CheckState.Checked; break;
                case CheckState.Indeterminate:
                    _checkState = CheckState.Unchecked;
                    break;
            }
            Invalidate(true);
        }
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            if (!((e.Button & MouseButtons.Left) == MouseButtons.Left)) return; _buttonState = ButtonState.Normal;
            Invalidate(true);
        }
        #endregion

        #region Designer Autocode
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // MpTilePanel
            // 
            this.Name = "MpTilePanel";
            this.Size = new System.Drawing.Size(994, 556);
            this.ResumeLayout(false);

        }
        #endregion       
    }
    public enum BevelStyle {
        /// <summary>Lowered border.</summary>
        Lowered,
        /// <summary>Raised border.</summary>
        Raised,
        /// <summary>Thin border.</summary>
        Flat
    }
    public enum ShadowMode {
        /// <summary>Specifies a shodow from upper left to lower right.</summary>
        ForwardDiagonal = 0,
        /// <summary>Specifies a surrounded shadow.</summary>
        Surrounded = 1,
        /// <summary>Specifies a dropped shadow.</summary>
        Dropped = 2
    }
    public enum PanelGradientMode {
        /// <summary>Specifies a gradient from upper right to lower left.</summary>
        BackwardDiagonal = 3,
        /// <summary>Specifies a gradient from upper left to lower right.</summary>
        ForwardDiagonal = 2,
        /// <summary>Specifies a gradient from left to right.</summary>
        Horizontal = 0,
        /// <summary>Specifies a gradient from top to bottom.</summary>
        Vertical = 1
    }
}
