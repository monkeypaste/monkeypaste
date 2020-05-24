using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpFileListItemRowPanelController : MpController {
        public Panel FileListItemRowPanel { get; set; }

        public PictureBox IconPictureBox { get; set; }

        public Label PathTextBox { get; set; }

        private int _rowId = -1;

        public MpFileListItemRowPanelController(int rowId, string path,MpController Parent) : base(Parent) {
            _rowId = rowId;

            FileListItemRowPanel = new Panel() {
                //FlowDirection = FlowDirection.LeftToRight,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top,
                Tag = _rowId
            };

            IconPictureBox = new PictureBox() {
                Anchor = AnchorStyles.Left,
                Location = Point.Empty,
                Tag = _rowId
            };
            FileAttributes attr = File.GetAttributes(path);
            if((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                IconPictureBox.Image = IconReader.GetFolderIcon(IconReader.IconSize.Large,IconReader.FolderType.Closed).ToBitmap();
            }
            else {
                IconPictureBox.Image = IconReader.GetFileIcon(path,IconReader.IconSize.Large,false).ToBitmap();
            }
            IconPictureBox.SetBounds(0,0,IconPictureBox.Image.Width,IconPictureBox.Image.Height);

            FileListItemRowPanel.Controls.Add(IconPictureBox);
            float fontSize = Properties.Settings.Default.LogPanelTileFontSize;
            float w = fontSize * Path.GetFileName(path).Length;
            FileListItemRowPanel.Bounds = new Rectangle(0,0,(int)w,IconPictureBox.Height);

            PathTextBox = new Label() {
                Font = new Font(Properties.Settings.Default.LogFont,fontSize),
                //ReadOnly = true,
                Text = Path.GetFileName(path),
                BackColor = (_rowId + 1) % 2 == 0 ? Color.AliceBlue : Color.Bisque,
                Bounds = new Rectangle(IconPictureBox.Width,0,FileListItemRowPanel.Width,IconPictureBox.Height),
                //Dock = DockStyle.Fill,                
                //AutoSize = true
                //BorderStyle = BorderStyle.None,
                //WordWrap = true                
            };
            FileListItemRowPanel.Controls.Add(PathTextBox);

            //_fileListItemRowPanel.SetBounds(0,0,_fileListItemRowPanel.Width,_iconPictureBox.Height);
            //Link(new List<MpIView>() { FileListItemRowPanel,IconPictureBox,PathTextBox });
        }
        
        public Panel GetFileListItemRowPanel() {
            return FileListItemRowPanel;
        }
        public Label GetPathTextBox() {
            return PathTextBox;
        }
        public PictureBox GetIconPictureBox() {
            return IconPictureBox;
        }

           public override void Update() {
            throw new NotImplementedException();
        }
    }
}
