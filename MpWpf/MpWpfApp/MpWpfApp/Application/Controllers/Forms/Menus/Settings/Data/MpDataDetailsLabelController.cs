﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWpfApp {
    public class MpDataDetailsLabelController:MpController {
        public Label DataDetailsLabel { get; set; }

        private int _currentDetailId = 0;

        public MpDataDetailsLabelController(MpController parentController) : base(parentController) {
            DataDetailsLabel = new Label()
            {
                //BackColor = Properties.Settings.Default.TileItemBgColor,
                //ForeColor = Properties.Settings.Default.TileIte MpHelperSingleton.Instance.IsBright((((MpTileDetailsPanelController)Parent).TileDetailsPanel).BackColor) ? Color.Black : Color.White,
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _currentDetailId = 0;
            DataDetailsLabel.MouseEnter += DataDetailsLabel_MouseEnter;
            //Link(new List<MpIView>() { DataDetailsLabel });
            Update();
        }

           public override void Update() {
            //tile details panel  rect
            //Rectangle tdpr = ((MpTileDetailsPanelController)Parent).TileDetailsPanel.Bounds;
            //DataDetailsLabel.SetBounds(0,0,tdpr.Width,tdpr.Height);

            //float fontSize = Properties.Settings.Default.TileDetailFontSizeRatio * (float)tdpr.Height;
            //fontSize = fontSize < 1.0f ? 10.0f : fontSize;
            //DataDetailsLabel.Font = new Font(Properties.Settings.Default.TileDetailFont,fontSize,GraphicsUnit.Pixel);
            //DataDetailsLabel.Location = new Point();
            DataDetailsLabel.Text = GetCurrentDetail(_currentDetailId);

            DataDetailsLabel.Invalidate();
        }
        public void DataDetailsLabel_MouseEnter(object sender,EventArgs e) {
            if(++_currentDetailId > 2) {
                _currentDetailId = 0;
            }
            Update();
        }
        protected string GetCurrentDetail(int detailId) {
            // TODO yupdate connected devices once multiple clients addee
            string info = string.Empty;
            switch(detailId) {
                //# of entries | file size
                case 0:
                    info = MpApplication.Instance.DataModel.CopyItemList.Count + " total items | " + (new FileInfo(MpApplication.Instance.DataModel.Db.DbPath).Length / 1024f) / 1024f + "MB";
                    break;
                //Encryption: On/Off | Device Count:
                case 1:
                    info = "Encryption: ";
                    info += MpApplication.Instance.DataModel.Db.DbPassword == string.Empty ? "Off" : "On" + " | 1 connected devices";
                    break;
                //Created: Date/Time | Modified Date/Time
                case 2:
                    info = "Created " + new FileInfo(MpApplication.Instance.DataModel.Db.DbPath).CreationTime.ToString() +" | Modified: "+ new FileInfo(MpApplication.Instance.DataModel.Db.DbPath).LastWriteTime.ToString();
                    break;
                default:
                    info = "UNKNOWN";
                    break;
            }

            return info;
        }
    }
}